using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestGrainWithGuidId : IGrain, IHaveGuidId
    {
        Task Foo();
    }
}
