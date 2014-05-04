using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.Bus
{
    public class TestGrainWithGuidId : MessageBasedGrainWithGuidId, ITestGrainWithGuidId
    {
        public Task Foo()
        {
            Console.WriteLine("This id -> {0}", Id());

            RegisterTimer(OnTimer, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
            DeactivateOnIdle();

            return TaskDone.Done;
        }

        public Task OnTimer(object state)
        {
            return TaskDone.Done;
        }
    }
}
