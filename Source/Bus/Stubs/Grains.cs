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
    }

    public interface IStubbedObservableMessageGrain
    {
        ObserverCollectionStub Observers
        {
            get;
        }
    }

    public interface IStubGuidId
    {
        void SetId(Guid id);
    }

    public interface IStubInt64Id
    {
        void SetId(long id);
    }

    public interface IStubStringId
    {
        void SetId(string id);
    }

    public interface IStubState<TState>
    {
        void SetState(TState state);
    }
}
