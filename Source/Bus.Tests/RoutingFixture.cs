using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class RoutingFixture
    {
        IBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = Bus.Instance;
        }

        [Test]
        public void When_sending_command()
        {
            Assert.DoesNotThrow(
                async () => await bus.Send(1, new FooCommand { Text = "foo" }));
        }

        [Test]
        public async void When_sending_query()
        {
            await bus.Send(1, new FooCommand {Text = "foo"});

            var result = await bus.Query<string>(1, new FooQuery());
            Assert.AreEqual("foo-1", result);
        }  
    }
}