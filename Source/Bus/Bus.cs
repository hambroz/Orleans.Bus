using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Fasterflect;
using Orleans.IoC;

namespace Orleans.Bus
{
    public class MessageBus : IServerMessageBus, IClientMessageBus
    {
        public static IServerMessageBus Server;
        public static IClientMessageBus Client;

        static MessageBus()
        {
            var instance = new MessageBus(GrainRuntime.Instance);
            instance.Initialize();

            Server = instance;
            Client = instance;
        }

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

        void Initialize()
        {
            foreach (var grain in runtime.RegisteredGrainTypes())
            {
                Register(grain);                
            }
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

        Task ICommandSender.Send(long id, object command)
        {
            var handler = commands[command.GetType()];

            var grain = runtime.Reference(handler.Grain, id);

            return handler.Handle(grain, command);
        }

        Task<TResult> IQueryHandler.Query<TResult>(long id, object query)
        {
            var handler = (QueryHandler<TResult>) queries[query.GetType()];

            var grain = runtime.Reference(handler.Grain, id);

            return handler.Handle(grain, query);
        }

        void IEventPublisher.Publish<TEvent>(IObservablePublisher publisher, TEvent @event)
        {
            // TODO : check whether grain had advertised this type of event via [Publisher]

            // double-dispatch
            publisher.Publish(@event);
        }

        async Task<SubscriptionToken> ISubscriptionManager.CreateToken<T>(IObserve<T> client)
        {
            var observer = new DynamicObserver(client);
            
            var reference = await runtime.Create((IObserve) observer);

            return new SubscriptionToken(reference);
        }

        void ISubscriptionManager.DeleteToken(SubscriptionToken token)
        {
            runtime.Delete(token.Reference);
        }

        public Task Subscribe<TEvent>(long id, IObserve<TEvent> client, SubscriptionToken token)
        {
            var handler = events[typeof(TEvent)];

            var grain = runtime.Reference(handler.Grain, id);

            return handler.Subscribe((IObservableGrain)grain, token.Reference);
        }

        public Task Unsubscribe<TEvent>(long id, IObserve<TEvent> client, SubscriptionToken token)
        {
            var handler = events[typeof(TEvent)];

            var grain = runtime.Reference(handler.Grain, id);

            return handler.Unsubscribe((IObservableGrain)grain, token.Reference);
        }
    }
}