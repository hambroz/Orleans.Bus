using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestGrain3 : IGrain, IHaveInt64Id
    {
        Task VoidMethodWhichThrowsArgumentException();
        Task<int> IntMethodWhichThrowsArgumentException();
    }
}
