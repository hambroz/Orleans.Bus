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
            const int grainId = 11;

            await bus.Subscribe<TextPublished>(grainId, observer);
            await bus.Send(grainId, new PublishText("sub"));
            
            client.EventReceived
                  .WaitOne(TimeSpan.FromSeconds(2));
            
            Assert.AreEqual("sub", client.PublishedText);
            Assert.AreEqual(grainId, client.SenderId);
        }
    }

    public class TestClient : Observes
    {
        public readonly EventWaitHandle EventReceived = new ManualResetEvent(false);

        public long SenderId = -1;
        public string PublishedText = "";

        public void On(object sender, object e)
        {
            this.On((long)sender, (dynamic)e);
        }

        void On(long sender, TextPublished e)
        {
            SenderId = sender;
            PublishedText = e.Text;
            EventReceived.Set();
        }
    }
}