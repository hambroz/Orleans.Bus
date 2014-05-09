using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    class SubscriptionManager
    {
        public static readonly SubscriptionManager Instance = 
           new SubscriptionManager(DynamicGrainFactory.Instance)
               .Initialize();

        readonly Dictionary<Type, EventSubscriber> events =
             new Dictionary<Type, EventSubscriber>();

        readonly DynamicGrainFactory factory;

        SubscriptionManager(DynamicGrainFactory factory)
        {
            this.factory = factory;
        }

        SubscriptionManager Initialize()
        {
            foreach (var grain in factory.RegisteredGrainTypes())
                Register(grain);

            return this;
        }

        void Register(Type grain)
        {
            foreach (var attribute in grain.Attributes<NotifiesAttribute>())
            {
                RegisterEventSubscriber(grain, attribute.Event);
            }
        }

        void RegisterEventSubscriber(Type grain, Type @event)
        {
            var handler = EventSubscriber.Create(grain, @event);
            events.Add(@event, handler);
        }

        public async Task<IObserve> CreateProxy(IObserve client)
        {
            return await ObserveFactory.CreateObjectReference(client);
        }

        public void DeleteProxy(IObserve observer)
        {
            ObserveFactory.DeleteObjectReference(observer);
        }

        public async Task Subscribe<TEvent>(string source, IObserve proxy)
        {
            var handler = events[typeof(TEvent)];

            var reference = factory.GetReference(handler.Grain, source);

            await handler.Subscribe(reference, proxy);
        }

        public async Task Unsubscribe<TEvent>(string source, IObserve proxy)
        {
            var handler = events[typeof(TEvent)];

            var reference = factory.GetReference(handler.Grain, source);

            await handler.Unsubscribe(reference, proxy);
        }

        class EventSubscriber
        {
            public readonly Type Grain;
            public readonly Type Event;

            EventSubscriber(Type grain, Type @event)
            {
                Grain = grain;
                Event = @event;
            }

            public static EventSubscriber Create(Type grain, Type @event)
            {
                return new EventSubscriber(grain, @event);
            }

            public async Task Subscribe(object grain, IObserve proxy)
            {
                var observable = (IObservableGrain)grain;
                await observable.Attach(proxy, Event);
            }

            public async Task Unsubscribe(object grain, IObserve proxy)
            {
                var observable = (IObservableGrain)grain;
                await observable.Detach(proxy, Event);
            }
        }
    }
}