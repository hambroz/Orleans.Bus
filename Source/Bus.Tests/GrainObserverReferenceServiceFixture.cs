using System;
using System.Linq;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class GrainObserverReferenceServiceFixture
    {
        GrainReferenceService references;
        GrainObserverService observers;

        [SetUp]
        public void SetUp()
        {
            references = GrainReferenceService.Instance;
            observers = GrainObserverService.Instance;
        }

        [Test]
        public async void Getting_observer_reference_by_interface()
        {
            ITestGrainObserver client = new TestClient();

            var observable = references.Get<ITestObservableGrain>(Guid.NewGuid());
            var observer = await observers.Create(client);

            Assert.DoesNotThrow(
                async ()=> await observable.Subscribe(observer.Proxy));
        }

        [Test]
        public void Getting_observer_reference_by_real_type()
        {
            var client = new TestClient();

            Assert.Throws<GrainObserverService.ObserverFactoryMethodNotFoundException>(
                async () => await observers.Create(client));
        }

        [Test]
        public async void Deleting_observer_reference_by_interface()
        {
            ITestGrainObserver client = new TestClient();

            var observer = await observers.Create(client);

            Assert.DoesNotThrow(() => observers.Delete(observer));
        }

        [Test]
        public async void Non_generic_version()
        {
            var client = new TestClient();

            var observer = await observers.Create(typeof(ITestGrainObserver), client);

            Assert.DoesNotThrow(() => observers.Delete(typeof(ITestGrainObserver), observer));
        }

        class TestClient : ITestGrainObserver
        {
            public void On(string e)
            {}
        }
    }
}
