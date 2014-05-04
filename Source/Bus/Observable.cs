using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    public interface Observes : IGrainObserver
    {
        void On(object sender, object e);
    }

    public interface IObservableGrain : IGrain
    {
        Task Subscribe(Type e, ObserverReference<Observes> o);
        Task Unsubscribe(Type e, ObserverReference<Observes> o);
    }

    public interface IObserver
    {}

    public sealed class Observer : IObserver
    {
        internal readonly ObserverReference<Observes> Reference;

        internal Observer(ObserverReference<Observes> reference)
        {
            Reference = reference;
        }
    }

    internal static class ObserverExtensions
    {
        public static ObserverReference<Observes> GetReference(this IObserver observer)
        {
            return ((Observer)observer).Reference;
        }
    }

    public interface IObserverCollection
    {
        void Attach(ObserverReference<Observes> observer, Type @event);
        void Detach(ObserverReference<Observes> observer, Type @event);
        void Notify<TEvent>(object id, TEvent e);
    }

    public class ObserverCollection : IObserverCollection
    {
        readonly IGrainRuntime runtime;

        readonly IDictionary<Type, IGrainObserverSubscriptionManager<Observes>> subscriptions =
           new Dictionary<Type, IGrainObserverSubscriptionManager<Observes>>();

        public ObserverCollection(IGrainRuntime runtime)
        {
            this.runtime = runtime;
        }

        public void Attach(ObserverReference<Observes> observer, Type @event)
        {
            IGrainObserverSubscriptionManager<Observes> manager;
            if (!subscriptions.TryGetValue(@event, out manager))
            {
                manager = runtime.CreateSubscriptionManager<Observes>();
                subscriptions.Add(@event, manager);
            }

            manager.Subscribe(observer);
        }

        public void Detach(ObserverReference<Observes> observer, Type @event)
        {
            IGrainObserverSubscriptionManager<Observes> manager;
            if (subscriptions.TryGetValue(@event, out manager))
                manager.Unsubscribe(observer);
        }

        public void Notify<TEvent>(object id, TEvent e)
        {
            IGrainObserverSubscriptionManager<Observes> manager;
            if (subscriptions.TryGetValue(typeof(TEvent), out manager))
                manager.Notify(x => x.On(id, e));
        }
    }
}