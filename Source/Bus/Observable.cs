using System;
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
}