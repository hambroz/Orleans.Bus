using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.Bus.Stubs
{
    public interface IStubbedMessageGrain
    {
        MessageBusStub Bus
        {
            get;
        }

        List<Invocation> Invocations
        {
            get;
        }

        void SetId(Guid id);
    }

    public interface IStubbedObservableMessageGrain
    {
        ObserverCollectionStub Observers
        {
            get;
        }
    }

    public interface IStubState<TState>
    {
        void SetState(TState state);
    }
}
