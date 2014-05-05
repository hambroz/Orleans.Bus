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
                async () => await bus.Send(1, new DoFoo("foo")));
        }

        [Test]
        public async void When_sending_query()
        {
            await bus.Send(1, new DoFoo("foo"));

            var result = await bus.Query<GetFoo, string>(1, new GetFoo());
            Assert.AreEqual("foo-1", result);
        }  
    }
}