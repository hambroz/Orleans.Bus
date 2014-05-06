using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Central communication hub for message exchanges
    /// between grains and between clients and grains.
    /// 
    /// Allows clients to dynamically subscribe/unsubscribe 
    /// to notifications about particular events.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Sends command message to the specified grain
        /// </summary>
        /// <param name="destination">Id of the destination grain</param>
        /// <param name="command">Command message to send</param>
        /// <returns>Promise</returns>
        Task Send(string destination, object command);

        /// <summary>
        /// Sends query message to the specified grain
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="destination">Id of the destination grain</param>
        /// <param name="query">Query message to send</param>
        /// <returns>Promise</returns>
        Task<TResult> Query<TResult>(string destination, object query);

        /// <summary>
        /// Subscribes given observer proxy to receive events from the specified grain
        /// </summary>
        /// <typeparam name="TEvent">Type of the event message</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Subscribe<TEvent>(string source, IObserver observer);

        /// <summary>
        /// Unsubscribes given observer proxy from receiving events from the specified grain
        /// </summary>
        /// <typeparam name="TEvent">Type of the event message</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Unsubscribe<TEvent>(string source, IObserver observer);

        /// <summary>
        /// Creates opaque client observer proxy to be used 
        /// for subscribing to event notifications
        /// </summary>
        /// <param name="client">The client</param>
        /// <returns>Promise</returns>
        Task<IObserver> CreateObserver(Observes client);

        /// <summary>
        /// Deletes (make available to GC) previously created client observer proxy, 
        /// </summary>
        /// <param name="observer">Client observer proxy</param>
        void DeleteObserver(IObserver observer);
    }

    /// <summary>
    /// Default implementation of <see cref="IMessageBus"/>
    /// </summary>
    public class MessageBus : IMessageBus
    {
        /// <summary>
        /// Globally available default instance of <see cref="IMessageBus"/>
        /// </summary>
        public static readonly IMessageBus Instance = 
            new MessageBus(GrainReferenceService.Instance, GrainObserverService.Instance)
                .Initialize();

        readonly Dictionary<Type, CommandHandler> commands = 
             new Dictionary<Type, CommandHandler>();        
        
        readonly Dictionary<Type, QueryHandler> queries =
             new Dictionary<Type, QueryHandler>();

        readonly Dictionary<Type, EventHandler> events =
             new Dictionary<Type, EventHandler>();

        readonly GrainReferenceService references;
        readonly GrainObserverService observers;

        MessageBus(GrainReferenceService references, GrainObserverService observers)
        {
            this.references = references;
            this.observers = observers;
        }

        MessageBus Initialize()
        {
            foreach (var grain in references.RegisteredGrainTypes())
                Register(grain);

            return this;
        }

        void Register(Type grain)
        {
            var handlers = grain.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Where(method => method.HasAttribute<HandlerAttribute>());

            foreach (var handler in handlers)
            {
                if (CommandHandler.Satisfies(handler))
                {
                    RegisterCommandHandler(grain, handler);
                    continue;
                }

                if (QueryHandler.Satisfies(handler))
                {
                    RegisterQueryHandler(grain, handler);
                    continue;
                }

                throw new NotSupportedException("Unsupported handler signature: " + handler);
            }

            foreach (var publisher in grain.Attributes<PublisherAttribute>())
            {
                RegisterEventHandler(grain, publisher.Event);
            }
        }

        void RegisterCommandHandler(Type grain, MethodInfo method)
        {
            var handler = CommandHandler.Create(grain, method);
            commands.Add(handler.Command, handler);
        }

        void RegisterQueryHandler(Type grain, MethodInfo method)
        {
            var handler = QueryHandler.Create(grain, method);
            queries.Add(handler.Query, handler);
        }

        void RegisterEventHandler(Type grain, Type @event)
        {
            var handler = EventHandler.Create(grain, @event);
            events.Add(@event, handler);
        }

        Task IMessageBus.Send(string destination, object command)
        {
            var handler = commands[command.GetType()];

            var grain = references.Get(handler.Grain, destination);

            return handler.Handle(grain, command);
        }

        Task<TResult> IMessageBus.Query<TResult>(string destination, object query)
        {
            var handler = (QueryHandler<TResult>) queries[query.GetType()];

            var reference = references.Get(handler.Grain, destination);

            return handler.Handle(reference, query);
        }

        async Task IMessageBus.Subscribe<TEvent>(string source, IObserver observer)
        {
            var handler = events[typeof(TEvent)];

            var reference = references.Get(handler.Grain, source);

            await handler.Subscribe(reference, observer);
        }

        async Task IMessageBus.Unsubscribe<TEvent>(string source, IObserver observer)
        {
            var handler = events[typeof(TEvent)];

            var reference = references.Get(handler.Grain, source);

            await handler.Unsubscribe(reference, observer);
        }

        async Task<IObserver> IMessageBus.CreateObserver(Observes client)
        {
            return new Observer(await observers.CreateProxy(client));
        }

        void IMessageBus.DeleteObserver(IObserver observer)
        {
            observers.DeleteProxy(observer.GetProxy());
        }

        [Serializable]
        internal class HandlerNotFoundException : ApplicationException
        {
            const string message = "Can't find handler for '{0}'.\r\nCheck that you've put [ExtendedPrimaryKey] on a grain and [Handler] on method";

            internal HandlerNotFoundException(Type messageType)
                : base(string.Format(message, messageType))
            {}

            protected HandlerNotFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }
    }
}