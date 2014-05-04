using System;
using System.Threading;

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
            await bus.Subscribe<TextPublished>(11, observer);
            await bus.Send(11, new PublishText("sub"));
            
            client.EventReceived.WaitOne(TimeSpan.FromSeconds(10));
            Assert.AreEqual("sub", client.PublishedText);
        }
    }

    public class TestClient : Observes
    {
        public readonly EventWaitHandle EventReceived = new ManualResetEvent(false);
        public string PublishedText = "";

        public void On(object sender, object e)
        {
            this.On((long)sender, (dynamic)e);
        }

        void On(long sender, TextPublished e)
        {
            PublishedText = e.Text;
            EventReceived.Set();
        }
    }
}