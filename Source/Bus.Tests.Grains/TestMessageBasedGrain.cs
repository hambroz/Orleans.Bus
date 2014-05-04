using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestMessageBasedGrain : MessageBasedGrainWithLongId, ITestMessageBasedGrain
    {
        string fooText = "";
        string barText = "";

        public Task Handle(DoFoo cmd)
        {
            Console.WriteLine(Id() + " is executing " + cmd.Text);
            fooText = cmd.Text;

            return TaskDone.Done;
        }

        public Task Handle(DoBar cmd)
        {
            Console.WriteLine(Id() + " is executing " + cmd.Text);
            barText = cmd.Text;

            return TaskDone.Done;
        }

        public Task<string> Answer(GetFoo query)
        {
            return Task.FromResult(fooText + "-" + Id());
        }

        public Task<string> Answer(GetBar query)
        {
            return Task.FromResult(barText + "-" + Id());
        }
    }
}
