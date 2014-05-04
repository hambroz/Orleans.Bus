using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.Bus
{
    public class TestGrainWithMissingExtendedPrimaryKeyAttribute : MessageBasedGrain, ITestGrainWithMissingExtendedPrimaryKeyAttribute
    {
        public Task Foo()
        {
            return TaskDone.Done;
        }
    }
}