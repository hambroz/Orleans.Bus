using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface IObserve<in TEvent>
    {
        void On(object sender, TEvent e);
    }

    public interface IObserve : IGrainObserver
    {
        void On(object sender, object e);
    }

    public interface IObservableGrain : IGrain
    {
        Task Subscribe(Type e, IObserve o);
        Task Unsubscribe(Type e, IObserve o);
    }

    public interface IObservablePublisher
    {
        void Publish<TEvent>(TEvent e);
    }

    public class DynamicObserver : IObserve
    {
        readonly object client;

        public DynamicObserver(object client)
        {
            this.client = client;
        }

        public void On(object sender, object e)
        {
            ((dynamic)client).On(sender, (dynamic)e);
        }
    }

    public class SubscriptionToken
    {
        internal readonly IObserve Reference;

        public SubscriptionToken(IObserve reference)
        {
            Reference = reference;
        }
    }
}