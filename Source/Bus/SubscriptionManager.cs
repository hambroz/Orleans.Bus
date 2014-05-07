using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Allows clients to dynamically subscribe/unsubscribe 
    /// to notifications about particular events.
    /// </summary>
    public interface ISubscriptionManager
    {
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

    public class SubscriptionManager : ISubscriptionManager
    {
        public static readonly ISubscriptionManager Instance = 
           new SubscriptionManager(DynamicGrainFactory.Instance).Initialize();

        readonly Dictionary<Type, EventHandler> events =
             new Dictionary<Type, EventHandler>();

        readonly DynamicGrainFactory factory;

        SubscriptionManager(DynamicGrainFactory factory)
        {
            this.factory = factory;
        }

        ISubscriptionManager Initialize()
        {
            foreach (var grain in factory.RegisteredGrainTypes())
                Register(grain);

            return this;
        }

        void Register(Type grain)
        {
            foreach (var publisher in grain.Attributes<PublisherAttribute>())
            {
                RegisterEventHandler(grain, publisher.Event);
            }
        }

        void RegisterEventHandler(Type grain, Type @event)
        {
            var handler = EventHandler.Create(grain, @event);
            events.Add(@event, handler);
        }

        async Task ISubscriptionManager.Subscribe<TEvent>(string source, IObserver observer)
        {
            var handler = events[typeof(TEvent)];

            var reference = factory.GetReference(handler.Grain, source);

            await handler.Subscribe(reference, observer);
        }

        async Task ISubscriptionManager.Unsubscribe<TEvent>(string source, IObserver observer)
        {
            var handler = events[typeof(TEvent)];

            var reference = factory.GetReference(handler.Grain, source);

            await handler.Unsubscribe(reference, observer);
        }

        async Task<IObserver> ISubscriptionManager.CreateObserver(Observes client)
        {
            return new Observer(await ObservesFactory.CreateObjectReference(client));
        }

        void ISubscriptionManager.DeleteObserver(IObserver observer)
        {
            ObservesFactory.DeleteObjectReference(observer.GetProxy());
        }

        class EventHandler
        {
            public readonly Type Grain;
            public readonly Type Event;

            EventHandler(Type grain, Type @event)
            {
                Grain = grain;
                Event = @event;
            }

            public static EventHandler Create(Type grain, Type @event)
            {
                return new EventHandler(grain, @event);
            }

            public async Task Subscribe(object grain, IObserver observer)
            {
                var observable = (IObservableGrain)grain;
                await observable.Attach(observer.GetProxy(), Event);
            }

            public async Task Unsubscribe(object grain, IObserver observer)
            {
                var observable = (IObservableGrain)grain;
                await observable.Detach(observer.GetProxy(), Event);
            }
        }
    }
}