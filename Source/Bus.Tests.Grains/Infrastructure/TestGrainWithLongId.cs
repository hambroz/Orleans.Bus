using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.Bus
{
    public class TestGrainWithInt64Id : MessageBasedGrain, ITestGrainWithInt64Id
    {
        public Task Foo() { return TaskDone.Done; }
    }
}
