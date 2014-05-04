using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestGrain3 : IGrain, IGrainWithLongId
    {
        Task VoidMethodWhichThrowsArgumentException();
        Task<int> IntMethodWhichThrowsArgumentException();
    }
}
