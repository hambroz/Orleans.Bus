using System;
using System.Linq;

using NSubstitute;
using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class MockabilityFixture
    {
        [Test]
        public void Can_mock_runtime_in_debug_mode()
        {
            var runtime  = Substitute.For<IGrainRuntime>();
            var instance = Substitute.For<IGrainInstance>();

            var thisGrain = new TestGrainWithGuidId
            {
                Runtime = runtime, 
                Instance = instance
            };

            var thisId = Guid.NewGuid();
            const string thatId = "that";

            var thatGrain = Substitute.For<ITestGrainWithStringId>();
            runtime.Reference<ITestGrainWithStringId>(thatId).Returns(thatGrain);
            runtime.Id(thisGrain).Returns(thisId);
            runtime.Id(thatGrain).Returns(thatId);

            thisGrain.Foo();

            instance.Received()
                    .RegisterTimer(thisGrain.OnTimer, null, 
                                   TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
            instance.Received()
                    .DeactivateOnIdle();
        } 
    }
}
