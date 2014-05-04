using System;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestObservableGrain : ObservableGrainWithLongId, ITestObservableGrain
    {
        public Task Handle(PublishText command)
        {
            Notify(new TextPublished(command.Text));

            return TaskDone.Done;
        }
    }
}