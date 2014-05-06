using System;
using System.Collections.Generic;
using System.Linq;

namespace Orleans.Bus.Stubs
{
    public class ObserverCollectionStub : IObserverCollection
    {
        public readonly List<Notification> Notifications = new List<Notification>();

        void IObserverCollection.Notify(object sender, object @event)
        {
            Notifications.Add(new Notification(sender, @event));
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

    public class Notification
    {
        public readonly object Sender;
        public readonly object Event;

        public Notification(object sender, object @event)
        {
            Sender = sender;
            Event = @event;
        }
    }
}
