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
        /// <remarks>The operation is idempotent</remarks>
        void Attach(IObserve observer, Type @event);

        /// <summary>
        /// Detaches given observer for the given type of event.
        /// </summary>
        /// <param name="observer">The observer proxy.</param>
        /// <param name="event">The type of event</param>
        /// <remarks>The operation is idempotent</remarks>
        void Detach(IObserve observer, Type @event);

        /// <summary>
        /// Notifies all attached observers about given event.
        /// </summary>
        /// <param name="source">An id of the source grain</param>
        /// <param name="event">An event</param>
        void Notify(string source, object @event);
    }

    /// <summary>
    /// Default implementation of <see cref="IObserverCollection"/>
    /// </summary>
    public class ObserverCollection : IObserverCollection
    {
        readonly IDictionary<Type, HashSet<IObserve>> subscriptions = new Dictionary<Type, HashSet<IObserve>>();

        void IObserverCollection.Attach(IObserve observer, Type @event)
        {
            var observers = Observers(@event);

            if (observers == null)
            {
                observers = new HashSet<IObserve>();
                subscriptions.Add(@event, observers);
            }

            observers.Add(observer);
        }

        void IObserverCollection.Detach(IObserve observer, Type @event)
        {
            var observers = Observers(@event);
            
            if (observers != null)
                observers.Remove(observer);
        }

        void IObserverCollection.Notify(string source, object @event)
        {
            var failed = new List<IObserve>();

            var observers = Observers(@event.GetType());
            if (observers == null)
                return;

            foreach (var observer in observers)
            {
                try
                {
                    observer.On(source, @event);
                }
                catch (Exception)
                {
                    failed.Add(observer);
                }
            }

            observers.RemoveWhere(failed.Contains);
        }

        internal HashSet<IObserve> Observers(Type @event)
        {
            HashSet<IObserve> result;
            return subscriptions.TryGetValue(@event, out result) ? result : null;
        }
    }
}