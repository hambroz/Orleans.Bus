using System;
using System.Linq;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class GrainReferenceServiceFixture
    {
        GrainReferenceService references;

        [SetUp]
        public void SetUp()
        {
            references = GrainReferenceService.Instance;
        }
        
        [Test]
        public void Getting_reference_by_guid()
        {
            var id = Guid.NewGuid();

            var grain = TestGrainWithGuidIdFactory.GetGrain(id);
            Assert.AreEqual(id, grain.GetPrimaryKey());
            
            Assert.NotNull(references.Get<ITestGrainWithGuidId>(id));
        }

        [Test]
        public void Getting_reference_by_long()
        {
            const long id = 123L;

            var grain = TestGrainWithInt64IdFactory.GetGrain(id);
            Assert.AreEqual(id, grain.GetPrimaryKeyLong());

            Assert.NotNull(references.Get<ITestGrainWithInt64Id>(id));
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

            Assert.NotNull(references.Get<ITestGrainWithStringId>(id));
        }

        [Test]
        public void Getting_reference_by_string_with_missing_extended_primary_key_attribute()
        {
            Assert.Throws<GrainReferenceService.MissingExtendedPrimaryKeyAttributeException>(() =>
                references.Get<ITestGrainWithMissingExtendedPrimaryKeyAttribute>("some-id-missing-EPK-attribute"));
        }

        [Test]
        public void Getting_reference_by_class_type_instead_of_interface()
        {
            Assert.Throws<GrainReferenceService.AccessByClassTypeException>(() =>
                references.Get<TestGrainWithInt64Id>(1));

            Assert.Throws<GrainReferenceService.AccessByClassTypeException>(() =>
                references.Get<TestGrainWithGuidId>(Guid.NewGuid()));

            Assert.Throws<GrainReferenceService.AccessByClassTypeException>(() =>
                references.Get<TestGrainWithStringId>("some-id"));
        }
    }
}
