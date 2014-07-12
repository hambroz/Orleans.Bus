using System;
using System.Linq;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class PersistenceFixture
    {
        IMessageBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = MessageBus.Instance;
            MockStorageProvider.Reset();
        }
        
        [Test]
        public async void Scratch_state()
        {
            MockStorageProvider.ReadStateReturnValue = 0;
            
            var result = await bus.Query<int>("p1", new GetValue());
            Assert.That(result, Is.EqualTo(0));
            
            Assert.That(MockStorageProvider.ReadStateGrainId, 
                Is.EqualTo("p1"));

            Assert.That(MockStorageProvider.ReadStateGrainType,
                Is.EqualTo("Orleans.Bus.PersistentGrain"));
        }

        [Test]
        public async void Write_state()
        {
            await bus.Send("p2", new SetValue(2));
            
            var result = await bus.Query<int>("p2", new GetValue());
            Assert.That(result, Is.EqualTo(2));

            Assert.That(MockStorageProvider.WriteStatePassedValue,
                Is.EqualTo(2));

            Assert.That(MockStorageProvider.WriteStateGrainId,
                Is.EqualTo("p2"));

            Assert.That(MockStorageProvider.WriteStateGrainType,
                Is.EqualTo("Orleans.Bus.PersistentGrain"));
        }

        [Test]
        public async void Clear_state()
        {
            await bus.Send("p3", new SetValue(3));
            await bus.Send("p3", new ClearValue());

            var result = await bus.Query<int>("p3", new GetValue());
            Assert.That(result, Is.EqualTo(-1));

            Assert.That(MockStorageProvider.ClearStatePassedValue,
                Is.EqualTo(3));

            Assert.That(MockStorageProvider.ClearStateGrainId,
                Is.EqualTo("p3"));

            Assert.That(MockStorageProvider.ClearStateGrainType,
                Is.EqualTo("Orleans.Bus.PersistentGrain"));
        } 
    }
}
