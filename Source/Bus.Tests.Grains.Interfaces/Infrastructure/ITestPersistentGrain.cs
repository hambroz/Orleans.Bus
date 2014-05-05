using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestPersistentGrain : IGrain, IHaveGuidId
    {
        Task<string> GetData();
        Task SetData(string data);
    }

    public interface ITestPersistentGrainState : IGrainState
    {
        string Data { get; set; }
    }
}
