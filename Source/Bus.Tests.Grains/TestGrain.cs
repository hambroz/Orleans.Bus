using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestGrain : Grain, ITestGrain
    {
        TestPoco poco;

        public override Task ActivateAsync()
        {
            poco = new TestPoco(this.Id());
            return poco.Activate();
        }

        public Task Handle(object cmd)
        {
            return poco.Handle((dynamic)cmd);
        }

        public async Task<object> Answer(object query)
        {
            return await poco.Answer((dynamic)query);
        }
    }

    public class TestPoco
    {
        readonly string id;

        string fooText = "";
        string barText = "";

        public TestPoco(string id)
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
