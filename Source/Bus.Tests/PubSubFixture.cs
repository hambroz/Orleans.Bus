using System;
using System.Threading;

using NUnit.Framework;
using Orleans.IoC;

namespace Orleans.Bus
{
    [TestFixture]
    public class PubSubFixture
    {
        IMessageBus bus;
        IGrainRuntime runtime;

        TestClient client;
        ObserverReference<IObserve> observer;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
            runtime = GrainRuntime.Instance;

            client = new TestClient();
            observer = runtime.CreateObserverReference<IObserve>(client).Result;
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

    public class TestClient : IObserve
    {
        public readonly EventWaitHandle EventReceived = new ManualResetEvent(false);
        public string PublishedText = "";

        public void On(object sender, object e)
        {
            this.On(sender, (dynamic)e);
        }

        void On(object sender, TextPublished e)
        {
            PublishedText = e.Text;
            EventReceived.Set();
        }
    }
}