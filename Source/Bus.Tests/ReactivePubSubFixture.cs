using System;
using System.Threading;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class ReactivePubSubFixture
    {
        IMessageBus bus;
        
        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
        }

        [Test]
        public async void Strict_subscription()
        {
            const string grainId = "11";

            using (var proxy = await ReactiveObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                TextPublished @event = null;

                var observable = await proxy.Attach<TextPublished>(grainId);
                observable.Subscribe(e =>
                {
                    source = e.Source;
                    @event = e.Message;
                    received.Set();
                });

                await bus.Send(grainId, new PublishText("sub"));
                received.WaitOne(TimeSpan.FromSeconds(5));

                Assert.NotNull(@event);
                Assert.AreEqual("sub", @event.Text);
                Assert.AreEqual(grainId, source);
            }
        }

        [Test]
        public async void Loose_subscription()
        {
            const string grainId = "11";

            using (var proxy = await ReactiveObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                TextPublished @event = null;

                var observable = await proxy.AttachLoose<TextPublished>(grainId);
                observable.Subscribe(e =>
                {
                    source = e.Source;
                    @event = (TextPublished) e.Message;
                    received.Set();
                });

                await bus.Send(grainId, new PublishText("sub"));
                received.WaitOne(TimeSpan.FromSeconds(5));

                Assert.NotNull(@event);
                Assert.AreEqual("sub", @event.Text);
                Assert.AreEqual(grainId, source);
            }
        }
    }
}