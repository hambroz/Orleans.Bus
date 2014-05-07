using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestGrain : Grain, ITestGrain
    {
        TestActor actor;

        public override Task ActivateAsync()
        {
            actor = new TestActor(this.Id());
            return actor.Activate();
        }

        public Task Handle(DoFoo cmd)
        {
            return actor.Handle(cmd);
        }

        public Task Handle(DoBar cmd)
        {
            return actor.Handle(cmd);
        }

        public Task<string> Answer(GetFoo query)
        {
            return actor.Answer(query);
        }

        public Task<string> Answer(GetBar query)
        {
            return actor.Answer(query);
        }
    }

    public class TestActor
    {
        readonly string id;
        string fooText = "";
        string barText = "";

        public TestActor(string id)
        {
            this.id = id;
        }

        public Task Activate()
        {
            return TaskDone.Done;
        }

        public Task Handle(DoFoo cmd)
        {
            Console.WriteLine(id + " is executing " + cmd.Text);
            fooText = cmd.Text;

            return TaskDone.Done;
        }

        public Task Handle(DoBar cmd)
        {
            Console.WriteLine(id + " is executing " + cmd.Text);
            barText = cmd.Text;

            return TaskDone.Done;
        }

        public Task<string> Answer(GetFoo query)
        {
            return Task.FromResult(fooText + "-" + id);
        }

        public Task<string> Answer(GetBar query)
        {
            return Task.FromResult(barText + "-" + id);
        }
    }
}
