using System;
using System.Linq;

namespace Orleans.Bus.Stubs
{
    public class ObserverCollectionStub : IObserverCollection
    {
        void IObserverCollection.Attach(Observes observer, Type @event)
        {
            throw new NotImplementedException();
        }

        void IObserverCollection.Detach(Observes observer, Type @event)
        {
            throw new NotImplementedException();
        }

        void IObserverCollection.Notify(object sender, object @event)
        {
            throw new NotImplementedException();
        }
    }
}
