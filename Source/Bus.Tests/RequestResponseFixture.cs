using System;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class RequestResponseFixture
    {
        IMessageBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
        }

        [Test]
        public void When_sending_command()
        {
            Assert.DoesNotThrow(
                async () => await bus.Send("test", new DoFoo("foo")));
        }

        [Test]
        public async void When_sending_query()
        {
            await bus.Send("test", new DoFoo("foo"));

            var result = await bus.Query<string>("test", new GetFoo());
            Assert.AreEqual("foo-test", result);
        }

        [Test]
        public void Should_unwrap_exception()
        {
            Assert.Throws<ApplicationException>(async ()=> await bus.Send("test", new ThrowException()));
        }  
    }
}