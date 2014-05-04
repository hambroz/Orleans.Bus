using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestGrainWithMissingExtendedPrimaryKeyAttribute : IGrain, IGrainWithStringId
    {
        Task Foo();
    }
}
