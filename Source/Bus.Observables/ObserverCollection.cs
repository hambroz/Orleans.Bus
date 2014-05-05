using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.Bus
{
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
}