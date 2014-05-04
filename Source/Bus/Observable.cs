using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    /// <summary>
    /// Callback interface need to be implemented by any client, 
    /// which want to be notified about particluar events.
    /// </summary>
    public interface Observes : IGrainObserver
    {
        /// <summary>
        /// Event notifications will be delivered to this callback method
        /// </summary>
        /// <param name="sender">An id of the sender</param>
        /// <param name="e">An event</param>
        void On(object sender, object e);
    }

    /// <summary>
    /// Internal iterface used only by infrastructure!
    /// </summary>
    public interface IObservableGrain : IGrain
    {
        /// <summary>
        /// Attaches given observer for the given type of event.
        /// </summary>
        /// <param name="o">The observer proxy.</param>
        /// <param name="e">The type of event</param>
        Task Attach(Observes o, Type e);

        /// <summary>
        /// Detaches given observer for the given type of event.
        /// </summary>
        /// <param name="o">The observer proxy.</param>
        /// <param name="e">The type of event</param>
        Task Detach(Observes o, Type e);
    }

    /// <summary>
    /// This  is a helper class for grains that support observers.
    /// It provides methods for attaching/detaching observers and for notifying about particular events.
    /// </summary>
    public interface IObserverCollection
    {
        /// <summary>
        /// Attaches given observer for the given type of event.
        /// </summary>
        /// <param name="observer">The observer proxy.</param>
        /// <param name="event">The type of event</param>
        /// <remarks>The operation is dempotent</remarks>
        void Attach(Observes observer, Type @event);

        /// <summary>
        /// Detaches given observer for the given type of event.
        /// </summary>
        /// <param name="observer">The observer proxy.</param>
        /// <param name="event">The type of event</param>
        /// <remarks>The operation is dempotent</remarks>
        void Detach(Observes observer, Type @event);

        /// <summary>
        /// Notifies all attached observers passing given event to each of them.
        /// </summary>
        /// <param name="sender">An id of the sender</param>
        /// <param name="event">An event</param>
        void Notify(object sender, object @event);
    }
    
    /// <summary>
    /// Default implementation of <see cref="IObserverCollection"/>
    /// </summary>
    public class ObserverCollection : IObserverCollection
    {
        readonly IDictionary<Type, HashSet<Observes>> subscriptions = new Dictionary<Type, HashSet<Observes>>();

        void IObserverCollection.Attach(Observes observer, Type @event)
        {
            var observers = Observers(@event);

            if (observers == null)
            {
                observers = new HashSet<Observes>();
                subscriptions.Add(@event, observers);
            }

            observers.Add(observer);
        }

        void IObserverCollection.Detach(Observes observer, Type @event)
        {
            var observers = Observers(@event);
            
            if (observers != null)
                observers.Remove(observer);
        }

        void IObserverCollection.Notify(object sender, object @event)
        {
            var failed = new List<Observes>();
            
            foreach (var observer in Observers(@event.GetType()))
            {
                try
                {
                    observer.On(sender, @event);
                }
                catch (Exception)
                {
                    failed.Add(observer);
                }
            }

            Observers(@event.GetType()).RemoveWhere(failed.Contains);
        }

        internal HashSet<Observes> Observers(Type @event)
        {
            HashSet<Observes> observers;
            return subscriptions.TryGetValue(@event, out observers) ? observers : null;
        }
    }

    /// <summary>
    /// Observer proxy to be used for subscribing to notifications
    /// </summary>
    public interface IObserver
    {}

    internal sealed class Observer : IObserver
    {
        internal readonly ObserverReference<Observes> Reference;

        internal Observer(ObserverReference<Observes> reference)
        {
            Reference = reference;
        }
    }

    internal static class ObserverExtensions
    {
        public static Observes GetProxy(this IObserver observer)
        {
            return observer.GetReference().Proxy;
        }

        public static ObserverReference<Observes> GetReference(this IObserver observer)
        {
            return ((Observer)observer).Reference;
        }
    }
}