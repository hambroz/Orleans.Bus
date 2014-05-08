using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Callback interface need to be implemented by any client, 
    /// which want to be notified about particluar events.
    /// </summary>
    public interface IObserve : IGrainObserver
    {
        /// <summary>
        /// Event notifications will be delivered to this callback method
        /// </summary>
        /// <param name="source">An id of the source grain</param>
        /// <param name="e">An event</param>
        void On(string source, object e);
    }

    /// <summary>
    /// Internal iterface used only by infrastructure!
    /// </summary>
    public interface IObservableGrain : IGrain
    {
        /// <summary>
        /// Attaches given selective observer for the given type of event.
        /// </summary>
        /// <param name="o">The observer proxy.</param>
        /// <param name="e">The type of event</param>
        Task Attach(IObserve o, Type e);

        /// <summary>
        /// Detaches given selective observer for the given type of event.
        /// </summary>
        /// <param name="o">The observer proxy.</param>
        /// <param name="e">The type of event</param>
        Task Detach(IObserve o, Type e);
    }
}