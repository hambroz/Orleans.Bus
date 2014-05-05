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
        public async void Getting_proxy_by_interface()
        {
            ITestGrainObserver client = new TestClient();

            var observable = references.Get<ITestObservableGrain>(Guid.NewGuid());
            var proxy = await observers.CreateProxy(client);

            Assert.DoesNotThrow(
                async ()=> await observable.Subscribe(proxy));
        }

        [Test]
        public void Getting_proxy_by_real_type()
        {
            var client = new TestClient();

            Assert.Throws<GrainObserverService.ObserverFactoryMethodNotFoundException>(
                async () => await observers.CreateProxy(client));
        }

        [Test]
        public async void Deleting_proxy_by_interface()
        {
            ITestGrainObserver client = new TestClient();

            var observer = await observers.CreateProxy(client);

            Assert.DoesNotThrow(() => observers.DeleteProxy(observer));
        }

        class TestClient : ITestGrainObserver
        {
            public void On(string e)
            {}
        }
    }
}
