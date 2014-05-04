using System;
using System.Linq;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class GrainReferenceServiceFixture
    {
        IGrainRuntime runtime;

        [SetUp]
        public void SetUp()
        {
            runtime = GrainRuntime.Instance;
        }
        
        [Test]
        public void Getting_reference_by_guid()
        {
            var id = Guid.NewGuid();

            var grain = TestGrainWithGuidIdFactory.GetGrain(id);
            Assert.AreEqual(id, grain.GetPrimaryKey());
            
            Assert.NotNull(runtime.Reference<ITestGrainWithGuidId>(id));
        }

        [Test]
        public void Getting_reference_by_long()
        {
            const long id = 123L;

            var grain = TestGrainWithLongIdFactory.GetGrain(id);
            Assert.AreEqual(id, grain.GetPrimaryKeyLong());

            Assert.NotNull(runtime.Reference<ITestGrainWithLongId>(id));
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

            Assert.NotNull(runtime.Reference<ITestGrainWithStringId>(id));
        }

        [Test]
        public void Getting_reference_by_string_with_missing_extended_primary_key_attribute()
        {
            Assert.Throws<GrainRuntime.MissingExtendedPrimaryKeyAttributeException>(() =>
                runtime.Reference<ITestGrainWithMissingExtendedPrimaryKeyAttribute>("some-id-missing-EPK-attribute"));
        }

        [Test]
        public void Getting_reference_by_class_type_instead_of_interface()
        {
            Assert.Throws<GrainRuntime.AccessByClassTypeException>(() =>
                runtime.Reference<TestGrainWithLongId>(1));

            Assert.Throws<GrainRuntime.AccessByClassTypeException>(() =>
                runtime.Reference<TestGrainWithGuidId>(Guid.NewGuid()));

            Assert.Throws<GrainRuntime.AccessByClassTypeException>(() =>
                runtime.Reference<TestGrainWithStringId>("some-id"));
        }
    }
}
