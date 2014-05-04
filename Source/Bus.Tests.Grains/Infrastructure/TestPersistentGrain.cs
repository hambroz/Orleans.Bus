using System;
using System.Threading.Tasks;

using Orleans.Bus;

namespace Orleans.Bus
{
    [StorageProvider(ProviderName = "MemoryStore")]
    public class TestPersistentGrain : MessageBasedGrain<ITestPersistentGrainState>, ITestPersistentGrain
    {
        public Task<string> GetData()
        {
            return Task.FromResult(State.Data);
        }

        public Task SetData(string data)
        {
            State.Data = data;
            return State.WriteStateAsync();
        }
    }
}