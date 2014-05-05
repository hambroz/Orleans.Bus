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

// TODO: move everything related to identity checking to Bus
/*

           if (!@interface.GetCustomAttributes(typeof(ExtendedPrimaryKeyAttribute), true).Any())
                throw new MissingExtendedPrimaryKeyAttributeException(@interface);
 
        [Test]
        public void Getting_reference_by_string_with_missing_extended_primary_key_attribute()
        {
            Assert.Throws<GrainReferenceService.MissingExtendedPrimaryKeyAttributeException>(() =>
                references.Get<ITestGrainWithMissingExtendedPrimaryKeyAttribute>("some-id-missing-EPK-attribute"));
        }
 
        [Serializable]
        internal class MissingExtendedPrimaryKeyAttributeException : ApplicationException
        {
            const string message = "Can't get {0} by string id. Make sure that interface is marked with [ExtendedPrimaryKey] attribute.";

            internal MissingExtendedPrimaryKeyAttributeException(Type grainType)
                : base(string.Format(message, grainType))
            {}

            protected MissingExtendedPrimaryKeyAttributeException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }
*/
    }
}
