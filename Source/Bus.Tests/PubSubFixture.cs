using System;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class PubSubFixture
    {
        IMessageBus bus;
        ISubscriptionManager subscriptions;

        TestClient client;
        IObserver observer;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
            subscriptions = SubscriptionManager.Instance;

            client = new TestClient();
            observer = subscriptions.CreateObserver(client).Result;
        }

        [Test]
        public void When_subscribed()
        {
            const string grainId = "11";

            subscriptions.Subscribe<TextPublished>(grainId, observer).Wait();
            bus.Send(grainId, new PublishText("sub")).Wait();
            
            client.EventReceived
                  .WaitOne(TimeSpan.FromSeconds(5));
            
            Assert.AreEqual("sub", client.PublishedText);
            Assert.AreEqual(grainId, client.Source);
        }

        [Test]
        public void Creating_duplicate_observer_for_the_same_client_reference()
        {
            var duplicate = subscriptions.CreateObserver(client).Result;

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