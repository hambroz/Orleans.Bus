using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Fasterflect;
using Orleans.IoC;

namespace Orleans.Bus
{
    public interface IMessageBus
    {
        Task Send(long id, object command);
        Task<TResult> Query<TResult>(long id, object query);

        Task Subscribe<TEvent>(long id, ObserverReference<IObserve> reference);
        Task Unsubscribe<TEvent>(long id, ObserverReference<IObserve> reference);
    }

    public class MessageBus : IMessageBus
    {
        public static IMessageBus Instance = new MessageBus(GrainRuntime.Instance).Initialize();

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

        async Task IMessageBus.Subscribe<TEvent>(long id, ObserverReference<IObserve> reference)
        {
            var handler = events[typeof(TEvent)];

            var grain = runtime.Reference(handler.Grain, id);

            await ((IObservableGrain)grain).Subscribe(handler.Event, reference);
        }

        async Task IMessageBus.Unsubscribe<TEvent>(long id, ObserverReference<IObserve> reference)
        {
            var handler = events[typeof(TEvent)];

            var grain = runtime.Reference(handler.Grain, id);

            await ((IObservableGrain)grain).Unsubscribe(handler.Event, reference);
        }
    }
}