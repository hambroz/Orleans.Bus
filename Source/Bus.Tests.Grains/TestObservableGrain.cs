using System;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestObservableGrain : ObservableGrain, ITestObservableGrain
    {
        TestObservableActor actor;

        public override Task ActivateAsync()
        {
            actor = new TestObservableActor(this.Id(), Notify);
            return actor.Activate();
        }

        public Task Handle(PublishText cmd)
        {
            return actor.Handle(cmd);
        }
    }

    class TestObservableActor
    {
        readonly string id;
        readonly Action<Event> notify;

        public TestObservableActor(string id, Action<Event> notify)
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