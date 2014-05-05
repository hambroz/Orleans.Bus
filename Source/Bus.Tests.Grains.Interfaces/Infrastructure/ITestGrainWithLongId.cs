using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestGrainWithInt64Id : IGrain, IHaveInt64Id
    {
        Task Foo();
    }
}
