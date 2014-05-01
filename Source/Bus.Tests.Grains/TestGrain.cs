using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    public class TestGrain : GrainWithLongId, ITestGrain
    {
        string fooText = "";
        string barText = "";

        public Task Handle(FooCommand command)
        {
            Console.WriteLine(Id() + " is executing " + command.Text);
            fooText = command.Text;

            return TaskDone.Done;
        }

        public Task Handle(BarCommand command)
        {
            Console.WriteLine(Id() + " is executing " + command.Text);
            barText = command.Text;

            return TaskDone.Done;
        }

        public Task<string> Answer(FooQuery query)
        {
            return Task.FromResult(fooText + "-" + Id());
        }

        public Task<string> Answer(BarQuery query)
        {
            return Task.FromResult(barText + "-" + Id());
        }
    }
}
