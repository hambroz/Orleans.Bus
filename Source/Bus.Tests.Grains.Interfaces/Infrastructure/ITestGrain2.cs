using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestGrain2 : IGrain, IGrainWithLongId
    {
        Task CallThrowingTestGrain3VoidMethod(ITestGrain3 grain3);
        Task<int> CallThrowingTestGrain3IntMethod(ITestGrain3 grain3);
    }
}
