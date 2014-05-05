using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Observer proxy to be used for subscribing to notifications
    /// </summary>
    public interface IObserver
    {}

    internal static class ObserverExtensions
    {
        public static Observes GetProxy(this IObserver observer)
        {
            return ((Observer)observer).Proxy;
        }
    }

    internal sealed class Observer : IObserver
    {
        internal readonly Observes Proxy;

        internal Observer(Observes proxy)
        {
            Proxy = proxy;
        }
    }
}