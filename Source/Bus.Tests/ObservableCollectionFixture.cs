using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class ObservableCollectionFixture
    {
        readonly Type @event = typeof(TextPublished);

        IMessageBus bus;
        IObserverCollection collection;

        IObserve client;
        IObserve proxy;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;

            client = new Observe();
            proxy  = SubscriptionManager.Instance.CreateProxy(client).Result;

            collection = new ObserverCollection();
        }

        [Test]
        public void Notify_when_no_observers()
        {
            Assert.DoesNotThrow(() => collection.Notify("test", @event));
        }

        [Test]
        public void Attach_is_idempotent()
        {
            collection.Attach(proxy, @event);
            
            Assert.DoesNotThrow(() => 
                collection.Attach(proxy, @event));

            Assert.AreEqual(1, GetObservers(@event).Count);
        }

        [Test]
        public void Detach_is_idempotent()
        {
            collection.Attach(proxy, @event);
            collection.Detach(proxy, @event);
            
            Assert.DoesNotThrow(() => 
                collection.Detach(proxy, @event));

            Assert.AreEqual(0, GetObservers(@event).Count);
        }

        HashSet<IObserve> GetObservers(Type @event)
        {
            return ((ObserverCollection)collection).Observers(@event);
        }

        class Observe : IObserve
        {
            public void On(string source, object e)
            {}
        }
    }
}