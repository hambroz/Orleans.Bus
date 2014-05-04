using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.Bus
{
    public class TestGrain3 : MessageBasedGrain, ITestGrain3
    {
        public Task VoidMethodWhichThrowsArgumentException()
        {
            throw new ArgumentException();
        }

        public Task<int> IntMethodWhichThrowsArgumentException()
        {
            throw new ArgumentException();
        }
    }
}
