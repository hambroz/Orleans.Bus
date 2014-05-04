using System;
using System.Linq;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class GrainObserverReferenceServiceFixture
    {
        IGrainRuntime runtime;

        [SetUp]
        public void SetUp()
        {
            runtime = GrainRuntime.Instance;
        }

        [Test]
        public async void Getting_observer_reference_by_interface()
        {
            ITestGrainObserver client = new TestClient();

            var observable = runtime.Reference<ITestObservableGrain>(Guid.NewGuid());
            var observer = await runtime.CreateObserverReference(client);

            Assert.DoesNotThrow(
                async ()=> await observable.Subscribe(observer.Proxy));
        }

        [Test]
        public void Getting_observer_reference_by_real_type()
        {
            var client = new TestClient();

            Assert.Throws<GrainRuntime.ObserverFactoryMethodNotFoundException>(
                async () => await runtime.CreateObserverReference(client));
        }

        [Test]
        public async void Deleting_observer_reference_by_interface()
        {
            ITestGrainObserver client = new TestClient();

            var observer = await runtime.CreateObserverReference(client);

            Assert.DoesNotThrow(() => runtime.DeleteObserverReference(observer));
        }

        [Test]
        public async void Non_generic_version()
        {
            var client = new TestClient();

            var observer = await runtime.CreateObserverReference(typeof(ITestGrainObserver), client);

            Assert.DoesNotThrow(() => runtime.DeleteObserverReference(typeof(ITestGrainObserver), observer));
        }

        class TestClient : ITestGrainObserver
        {
            public void On(string e)
            {}
        }
    }
}
