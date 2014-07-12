using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    [StorageProvider(ProviderName = "PersistentGrainStorageProvider")]
    public class PersistentGrain : MessageBasedGrain<IPersistentGrainState>, IPersistentGrain
    {
        Task IPersistentGrain.HandleCommand(object cmd)
        {
            return this.Handle((dynamic) cmd);
        }

        async Task<object> IPersistentGrain.AnswerQuery(object query)
        {
            return await this.Answer((dynamic)query);
        }

        public Task Handle(SetValue cmd)
        {
            State.Total = cmd.Value;
            return State.WriteStateAsync();
        }

        public async Task Handle(ClearValue cmd)
        {
            await State.ClearStateAsync();
            State.Total = -1;
        }

        public Task<int> Answer(GetValue query)
        {
            return Task.FromResult(State.Total);
        }
    }
}
