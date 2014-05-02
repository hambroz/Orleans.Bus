using System;
using System.Threading;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class PubSubFixture
    {
        IClientMessageBus bus;

        TestClient client;
        SubscriptionToken token;

        [SetUp]
        public async void SetUp()
        {
            bus = MessageBus.Client;

            client = new TestClient();
            token  = await bus.CreateToken(client);
        }

        [Test]
        public async void When_subscribed()
        {
            await bus.Subscribe<TextPublished>(11, client, token);
            await bus.Send(11, new PublishText("sub"));
            
            client.EventReceived.WaitOne(TimeSpan.FromSeconds(10));
            Assert.AreEqual("sub", client.PublishedText);
        }
    }

    public class TestClient : IObserve<TextPublished>
    {
        public readonly EventWaitHandle EventReceived = new ManualResetEvent(false);
        public string PublishedText = "";
            
        public void On(object sender, TextPublished e)
        {
            PublishedText = e.Text;
            EventReceived.Set();
        }
    }
}