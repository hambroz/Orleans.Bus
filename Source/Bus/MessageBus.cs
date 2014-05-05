using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Fasterflect;

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
        /// Sends command message to a grain with the given <see cref="Guid"/> id
        /// </summary>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        Task Send(Guid id, object command);
        
        /// <summary>
        /// Sends command message to a grain with the given <see cref="Int64"/> id
        /// </summary>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        Task Send(long id, object command);
        
        /// <summary>
        /// Sends command message to a grain with the given <see cref="string"/> id
        /// </summary>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        Task Send(string id, object command);

        /// <summary>
        /// Sends query message to a grain with the given  <see cref="Guid"/> id and casts result to the specified type
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        Task<TResult> Query<TResult>(Guid id, object query);

        /// <summary>
        /// Sends query message to a grain with the given  <see cref="Int64"/> id
        /// and casts result to the specified type
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        Task<TResult> Query<TResult>(long id, object query);
        
        /// <summary>
        /// Sends query message to a grain with the given  <see cref="String"/> id and casts result to the specified type
        /// </summary>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        Task<TResult> Query<TResult>(string id, object query);

        /// <summary>
        /// Subscribes given observer to receive events of the specified type 
        /// from the grain with the given <see cref="Guid"/> id
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Subscribe<TEvent>(Guid id, IObserver observer);

        /// <summary>
        /// Subscribes given observer to receive events of the specified type 
        /// from the grain with the given <see cref="Int64"/> id
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Subscribe<TEvent>(long id, IObserver observer);

        /// <summary>
        /// Subscribes given observer to receive events of the specified type 
        /// from the grain with the given <see cref="String"/> id
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Subscribe<TEvent>(string id, IObserver observer);

        /// <summary>
        /// Unsubscribes given observer from receiving events of the specified type 
        /// from the grain with the given <see cref="Guid"/> id
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Unsubscribe<TEvent>(Guid id, IObserver observer);

        /// <summary>
        /// Unsubscribes given observer from receiving events of the specified type 
        /// from the grain with the given <see cref="Int64"/> id
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Unsubscribe<TEvent>(long id, IObserver observer);

        /// <summary>
        /// Unsubscribes given observer from receiving events of the specified type 
        /// from the grain with the given <see cref="string"/> id
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="observer">Client observer proxy</param>
        /// <returns>Promise</returns>
        Task Unsubscribe<TEvent>(string id, IObserver observer);

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

        Task IMessageBus.Send(Guid id, object command)
        {
            return Send(grain => references.Get(grain, id), command);
        }

        Task IMessageBus.Send(long id, object command)
        {
            return Send(grain => references.Get(grain, id), command);
        }

        Task IMessageBus.Send(string id, object command)
        {
            return Send(grain => references.Get(grain, id), command);
        }

        Task Send(Func<Type, object> getReference, object command)
        {
            var handler = commands[command.GetType()];

            var grain = getReference(handler.Grain);

            return handler.Handle(grain, command);            
        }

        Task<TResult> IMessageBus.Query<TResult>(Guid id, object query)
        {
            return Query<TResult>(grain => references.Get(grain, id), query);
        }

        Task<TResult> IMessageBus.Query<TResult>(long id, object query)
        {
            return Query<TResult>(grain => references.Get(grain, id), query);
        }

        Task<TResult> IMessageBus.Query<TResult>(string id, object query)
        {
            return Query<TResult>(grain => references.Get(grain, id), query);
        }

        Task<TResult> Query<TResult>(Func<Type, object> getReference, object query)
        {
            var handler = (QueryHandler<TResult>) queries[query.GetType()];

            var reference = getReference(handler.Grain);

            return handler.Handle(reference, query);
        }

        Task IMessageBus.Subscribe<TEvent>(Guid id, IObserver observer)
        {
            return Subscribe<TEvent>(grain => references.Get(grain, id), observer);
        }
        
        Task IMessageBus.Subscribe<TEvent>(long id, IObserver observer)
        {
            return Subscribe<TEvent>(grain => references.Get(grain, id), observer);
        }
        
        Task IMessageBus.Subscribe<TEvent>(string id, IObserver observer)
        {
            return Subscribe<TEvent>(grain => references.Get(grain, id), observer);
        }

        async Task Subscribe<TEvent>(Func<Type, object> getReference, IObserver observer)
        {
            var handler = events[typeof(TEvent)];

            var reference = getReference(handler.Grain);

            await handler.Subscribe(reference, observer);
        }

        Task IMessageBus.Unsubscribe<TEvent>(Guid id, IObserver observer)
        {
            return Unsubscribe<TEvent>(grain => references.Get(grain, id), observer);
        }        
        
        Task IMessageBus.Unsubscribe<TEvent>(long id, IObserver observer)
        {
            return Unsubscribe<TEvent>(grain => references.Get(grain, id), observer);
        }        
        
        Task IMessageBus.Unsubscribe<TEvent>(string id, IObserver observer)
        {
            return Unsubscribe<TEvent>(grain => references.Get(grain, id), observer);
        }

        async Task Unsubscribe<TEvent>(Func<Type, object> getReference, IObserver observer)
        {
            var handler = events[typeof(TEvent)];

            var reference = getReference(handler.Grain);

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
    }
}