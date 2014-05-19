using System;
using System.Linq;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class IdentityFixture
    {
        [Test]
        public void Id_from_reference()
        {
            var grain = TestGrainFactory.GetGrain(0, "test");
            var reference = grain.AsReference();
            Assert.AreEqual("test", Identity.Of(reference));
        } 
    }
}
