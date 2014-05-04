using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.Bus
{
    public class TestGrain2 : MessageBasedGrain, ITestGrain2
    {
        public async Task CallThrowingTestGrain3VoidMethod(ITestGrain3 grain3)
        {
            await grain3.VoidMethodWhichThrowsArgumentException();
        }

        public async Task<int> CallThrowingTestGrain3IntMethod(ITestGrain3 grain3)
        {
            return await grain3.IntMethodWhichThrowsArgumentException();
        }
    }
}
