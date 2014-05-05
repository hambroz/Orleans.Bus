using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class PersistentGrainFixture
    {
        [Test]
        public async void Should_return_grain_state_when_state_is_not_set_explicitly()
        {
            var references = GrainReferenceService.Instance;
            
            var grain = references.Get<ITestPersistentGrain>(Guid.NewGuid());
            await grain.SetData("test");
            
            Assert.AreEqual("test", await grain.GetData());
        }

        [Test]
        public async void Should_return_explicit_state_when_set()
        {
            var state = new MockState { Data = "test"};
            
            var grain = new TestPersistentGrain {State = state};
            Assert.AreEqual("test", await grain.GetData());

            await grain.SetData("changed");
            Assert.IsTrue(state.WriteStateAsyncWasCalled);
        }

        class MockState : ITestPersistentGrainState
        {
            public string Data { get; set; }

            public bool WriteStateAsyncWasCalled;
            
            public Task WriteStateAsync()
            {
                WriteStateAsyncWasCalled = true;
                return TaskDone.Done;
            }

            #region Unused

            public Task ClearStateAsync()
            {
                throw new NotImplementedException();
            }

            public Task ReadStateAsync()
            {
                throw new NotImplementedException();
            }

            public Dictionary<string, object> AsDictionary()
            {
                throw new NotImplementedException();
            }

            public void SetAll(Dictionary<string, object> values)
            {
                throw new NotImplementedException();
            }

            public string Etag { get; set; }

            #endregion
        }
    }
}
