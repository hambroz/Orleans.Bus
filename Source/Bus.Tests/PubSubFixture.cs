using System;
using System.Threading;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class PubSubFixture
    {
        IMessageBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
        }

        [Test]
        public async void Subscription()
        {
            const string grainId = "11";

            using (var proxy = await ObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                TextPublished @event = null;

                await proxy.Attach<TextPublished>(grainId, (s, e) =>
                {
                    source = s;
                    @event = e;
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
        public async void Generic_subscription()
        {
            const string grainId = "11";

            using (var proxy = await GenericObservableProxy.Create())
            {
                var received = new AutoResetEvent(false);

                string source = null;
                TextPublished @event = null;

                await proxy.Attach<TextPublished>(grainId, (s, e) =>
                {
                    source = s;
                    @event = (TextPublished)e;
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