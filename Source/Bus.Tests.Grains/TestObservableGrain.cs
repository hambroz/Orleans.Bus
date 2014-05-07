using System;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestObservableGrain : ObservableGrain, ITestObservableGrain
    {
        TestObservablePoco poco;

        public override Task ActivateAsync()
        {
            poco = new TestObservablePoco(this.Id(), Notify);
            return poco.Activate();
        }

        public Task Handle(PublishText cmd)
        {
            return poco.Handle(cmd);
        }
    }

    class TestObservablePoco
    {
        readonly string id;
        readonly Action<Event> notify;

        public TestObservablePoco(string id, Action<Event> notify)
        {
            this.id = id;
            this.notify = notify;
        }

        public Task Activate()
        {
            return TaskDone.Done;
        }

        public Task Handle(PublishText cmd)
        {
            notify(new TextPublished(cmd.Text));

            return TaskDone.Done;
        }
    }
}