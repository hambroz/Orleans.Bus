using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestGrain1 : IGrain, IHaveInt64Id
    {
        Task CallThrowingTestGrain2VoidMethod(ITestGrain2 grain2, ITestGrain3 grain3);
        Task<int> CallThrowingTestGrain2IntMethod(ITestGrain2 grain2, ITestGrain3 grain3);
    }
}
