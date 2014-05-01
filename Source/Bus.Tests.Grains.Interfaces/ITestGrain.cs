using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    public interface ITestGrain : IGrain, IGrainWithLongId
    {
        Task Foo();
    }
}
