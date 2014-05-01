using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    public class TestGrain : GrainWithGuidId, ITestGrain
    {
        public Task Foo()
        {
            return TaskDone.Done;
        }
    }
}
