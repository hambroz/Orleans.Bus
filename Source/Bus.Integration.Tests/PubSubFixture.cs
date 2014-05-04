using System;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class PubSubFixture
    {
        IMessageBus bus;

        TestClient client;
        IObserver observer;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
            client = new TestClient();
            observer = bus.CreateObserver(client).Result;
        }

        [Test]
        public async void When_subscribed()
        {
            const int grainId = 11;

            await bus.Subscribe<TextPublished>(grainId, observer);
            await bus.Send(grainId, new PublishText("sub"));
            
            client.EventReceived
                  .WaitOne(TimeSpan.FromSeconds(2));
            
            Assert.AreEqual("sub", client.PublishedText);
            Assert.AreEqual(grainId, client.SenderId);
        }

        [Test]
        public void Creating_duplicate_observer_for_the_same_client_reference()
        {
            var duplicate = bus.CreateObserver(client).Result;

            Assert.AreNotSame(observer, duplicate, 
                "Observers should be different");

            Assert.AreNotSame(observer, duplicate,
                "Observer references will also be different");

            Assert.AreNotSame(observer.GetProxy(), duplicate.GetProxy(),
                "And underlying proxies are also different");
            
            // NOTE: so ideally you should create observer only once
        }
    }
}