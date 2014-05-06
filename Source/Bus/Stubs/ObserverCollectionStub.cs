using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.Bus.Stubs
{
    public class ObserverCollectionStub : IObserverCollection
    {
        public readonly List<RecordedEvent> RecordedEvents = new List<RecordedEvent>();

        void IObserverCollection.Notify(string source, object @event)
        {
            RecordedEvents.Add(new RecordedEvent(source, @event));
        }

        #region Unused

        void IObserverCollection.Attach(Observes observer, Type @event)
        {
            throw new NotImplementedException();
        }

        void IObserverCollection.Detach(Observes observer, Type @event)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class RecordedEvent
    {
        public readonly object Sender;
        public readonly object Event;

        public RecordedEvent(object sender, object @event)
        {
            Sender = sender;
            Event = @event;
        }
    }
}
