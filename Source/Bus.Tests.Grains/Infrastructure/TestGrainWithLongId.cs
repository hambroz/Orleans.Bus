using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.Bus
{
    public class TestGrainWithLongId : MessageBasedGrain, ITestGrainWithLongId
    {
        public Task Foo() { return TaskDone.Done; }
    }
}
