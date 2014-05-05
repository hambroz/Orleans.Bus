using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestObservableGrain : IGrain, IHaveGuidId
    {
        Task Publish(string e);
        Task Subscribe(ITestGrainObserver observer);
        Task Unsubscribe(ITestGrainObserver observer);
    }

    public interface ITestGrainObserver : IGrainObserver
    {
        void On(string e);
    }
}
