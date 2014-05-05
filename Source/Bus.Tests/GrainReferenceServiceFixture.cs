using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class GrainReferenceServiceFixture
    {
        GrainReferenceService service;

        [SetUp]
        public void SetUp()
        {
            service = GrainReferenceService.Instance;
        }
        
        [Test]
        public void Getting_reference_by_guid()
        {
            var id = Guid.NewGuid();

            var grain = TestGrainWithGuidIdFactory.GetGrain(id);
            Assert.AreEqual(id, grain.GetPrimaryKey());
            
            Assert.NotNull(service.Get<ITestGrainWithGuidId>(id));
        }

        [Test]
        public void Getting_reference_by_long()
        {
            const long id = 123L;

            var grain = TestGrainWithInt64IdFactory.GetGrain(id);
            Assert.AreEqual(id, grain.GetPrimaryKeyLong());

            Assert.NotNull(service.Get<ITestGrainWithInt64Id>(id));
        }

        [Test]
        public void Getting_reference_by_string()
        {
            const string id = "some-string-grain-id";
            var grain = TestGrainWithStringIdFactory.GetGrain(0, id);
            
            string returnedId;
            var longId = grain.GetPrimaryKeyLong(out returnedId);
            
            Assert.AreEqual(0, longId);
            Assert.AreEqual(id, returnedId);

            Assert.NotNull(service.Get<ITestGrainWithStringId>(id));
        }

        [Test]
        public void Getting_reference_by_string_with_missing_extended_primary_key_attribute()
        {
            var s = new GrainReferenceService();
            
            Assert.Throws<GrainReferenceService.MissingExtendedPrimaryKeyAttributeException>(() =>
                 s.Initialize(InjectGrainWithStringIdWhichMissesExtendedPkAttribute));
        }

        static IEnumerable<FactoryProductBinding> InjectGrainWithStringIdWhichMissesExtendedPkAttribute(Type type)
        {
            var binding = new FactoryProductBinding(null, typeof(ITestGrainWithStringIdMissingExtendedPkAttribute));
            return type == typeof(IHaveStringId) ? new[] {binding} : Enumerable.Empty<FactoryProductBinding>();
        }

        interface ITestGrainWithStringIdMissingExtendedPkAttribute : IGrain, IHaveStringId
        {
            Task Foo();
        }
    }
}
