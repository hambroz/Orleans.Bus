using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITestPersistentGrain : IGrain, IGrainWithGuidId
    {
        Task<string> GetData();
        Task SetData(string data);
    }

    public interface ITestPersistentGrainState : IGrainState
    {
        string Data { get; set; }
    }
}
