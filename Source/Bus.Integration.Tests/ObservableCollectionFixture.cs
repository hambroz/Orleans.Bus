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
        TestClient client;
        IObserver observer;

        IObserverCollection collection;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;

            client = new TestClient();
            observer = bus.CreateObserver(client).Result;

            collection = new ObserverCollection();
        }

        [Test]
        public void Attach_is_idempotent()
        {
            collection.Attach(observer.GetProxy(), @event);
            
            Assert.DoesNotThrow(() => 
                collection.Attach(observer.GetProxy(), @event));

            Assert.AreEqual(1, GetObservers(@event).Count);
        }

        [Test]
        public void Detach_is_idempotent()
        {
            collection.Attach(observer.GetProxy(), @event);
            collection.Detach(observer.GetProxy(), @event);
            
            Assert.DoesNotThrow(() => 
                collection.Detach(observer.GetProxy(), @event));

            Assert.AreEqual(0, GetObservers(@event).Count);
        }

        HashSet<Observes> GetObservers(Type @event)
        {
            return ((ObserverCollection)collection).Observers(@event);
        }
    }
}