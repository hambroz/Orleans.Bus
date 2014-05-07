using System;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestObservableGrain : ObservableGrain, ITestObservableGrain
    {
        public Task Handle(PublishText command)
        {
            Notify(new TextPublished(""));

            return TaskDone.Done;
        }
    }
}