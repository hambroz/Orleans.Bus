using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestGrain1 : MessageBasedGrain, ITestGrain1
    {
        public async Task CallThrowingTestGrain2VoidMethod(ITestGrain2 grain2, ITestGrain3 grain3)
        {
            await grain2.CallThrowingTestGrain3IntMethod(grain3);
        }

        public async Task<int> CallThrowingTestGrain2IntMethod(ITestGrain2 grain2, ITestGrain3 grain3)
        {
            return await grain2.CallThrowingTestGrain3IntMethod(grain3);
        }
    }
}
