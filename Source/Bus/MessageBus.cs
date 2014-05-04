using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Fasterflect;
using Orleans.IoC;

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
        /// Sends command message to a grain with the given id
        /// </summary>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        Task Send(long id, object command);

        /// <summary>
        /// Sends query message to a grain with the given id and casts result to the specified type
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        Task<TResult> Query<TResult>(long id, object query);

        /// <summary>
        /// Subscribes given observer to receive events of the specified type 
        /// from the grain with the given id
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Subscribe<TEvent>(long id, IObserver observer);

        /// <summary>
        /// Unsubscribes given observer from receiving events of the specified type 
        /// from the grain with the given id
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Unsubscribe<TEvent>(long id, IObserver observer);

        /// <summary>
        /// Creates opaque client observer proxy to be used 
        /// for subscribing to event notifications
        /// </summary>
        /// <param name="client">The client</param>
        /// <returns>Promise</returns>
        Task<IObserver> CreateObserver(Observes client);

        /// <summary>
        /// Deletes (make available to GC) an opaque client observer proxy, 
        /// which was previously created
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
           new MessageBus(GrainRuntime.Instance).Initialize();

        readonly Dictionary<Type, CommandHandler> commands = 
             new Dictionary<Type, CommandHandler>();        
        
        readonly Dictionary<Type, QueryHandler> queries =
             new Dictionary<Type, QueryHandler>();

        readonly Dictionary<Type, EventHandler> events =
             new Dictionary<Type, EventHandler>();

        readonly IGrainRuntime runtime;

        MessageBus(IGrainRuntime runtime)
        {
            this.runtime = runtime;
        }

        MessageBus Initialize()
        {
            foreach (var grain in runtime.RegisteredGrainTypes())
            {
                Register(grain);                
            }

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
            var handler = new CommandHandler(grain, method);
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

        Task IMessageBus.Send(long id, object command)
        {
            var handler = commands[command.GetType()];

            var grain = runtime.Reference(handler.Grain, id);

            return handler.Handle(grain, command);
        }

        Task<TResult> IMessageBus.Query<TResult>(long id, object query)
        {
            var handler = (QueryHandler<TResult>) queries[query.GetType()];

            var grain = runtime.Reference(handler.Grain, id);

            return handler.Handle(grain, query);
        }

        async Task IMessageBus.Subscribe<TEvent>(long id, IObserver observer)
        {
            var handler = events[typeof(TEvent)];

            var grain = runtime.Reference(handler.Grain, id);

            await handler.Subscribe(grain, observer);
        }

        async Task IMessageBus.Unsubscribe<TEvent>(long id, IObserver observer)
        {
            var handler = events[typeof(TEvent)];

            var grain = runtime.Reference(handler.Grain, id);

            await handler.Unsubscribe(grain, observer);
        }

        async Task<IObserver> IMessageBus.CreateObserver(Observes client)
        {
            return new Observer(await runtime.CreateObserverReference(client));
        }

        void IMessageBus.DeleteObserver(IObserver observer)
        {
            runtime.DeleteObserverReference(observer.GetReference());
        }
    }
}