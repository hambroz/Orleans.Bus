using System;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestObservableGrain : MessageBasedGrain, ITestObservableGrain
    {
        ObserverSubscriptionManager<ITestGrainObserver> subscribers;

        public override Task ActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<ITestGrainObserver>();
            return TaskDone.Done;
        }

        public override Task DeactivateAsync()
        {
            subscribers.Clear();
            return TaskDone.Done;
        }

        public Task Publish(string e)
        {
            subscribers.Notify(s => s.On(e));
            return TaskDone.Done;
        }

        public Task Subscribe(ITestGrainObserver observer)
        {
            subscribers.Subscribe(observer);
            return TaskDone.Done;
        }
        public Task Unsubscribe(ITestGrainObserver observer)
        {
            subscribers.Unsubscribe(observer);
            return TaskDone.Done;
        }
    }
}