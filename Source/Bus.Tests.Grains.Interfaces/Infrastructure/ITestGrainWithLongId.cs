using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestGrainWithLongId : IGrain, IGrainWithLongId
    {
        Task Foo();
    }
}
