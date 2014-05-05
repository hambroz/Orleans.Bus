using System;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestObservableMessageBasedGrain : ObservableMessageBasedGrainWithInt64Id, ITestObservableMessageBasedGrain
    {
        public Task Handle(PublishText command)
        {
            Notify(new TextPublished(command.Text));

            return TaskDone.Done;
        }
    }
}