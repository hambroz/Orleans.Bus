using System;
using System.Linq;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class AggregateExceptionExtensionsFixture
    {
        IGrainRuntime service;

        ITestGrain1 grain1;
        ITestGrain2 grain2;
        ITestGrain3 grain3;

        [SetUp]
        public void SetUp()
        {
            service = GrainRuntime.Instance;

            grain1 = service.Reference<ITestGrain1>(1);
            grain2 = service.Reference<ITestGrain2>(2);
            grain3 = service.Reference<ITestGrain3>(3);
        }

        [Test]
        public async void Should_unwrap_and_rethrow_an_exception_for_void_methods()
        {
            try
            {
                await grain1.CallThrowingTestGrain2VoidMethod(grain2, grain3).UnwrapExceptions();
            }
            catch (ArgumentException e)
            {
                LogToConsole(e);
            }
            catch (Exception e)
            {
                LogToConsole(e);
                Assert.Fail("Exception was uwrapped");
            }
        }

        [Test]
        public async void Should_unwrap_and_rethrow_an_exception_for_nonvoid_methods()
        {
            try
            {
                await grain1.CallThrowingTestGrain2IntMethod(grain2, grain3).UnwrapExceptions();
            }
            catch (ArgumentException e)
            {
                LogToConsole(e);
            }
            catch (Exception e)
            {
                LogToConsole(e);
                Assert.Fail("Exception was unwrapped");
            }
        }

        static void LogToConsole(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}
