using System;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Orleans.Bus
{
    [TestFixture]
    public class BusFixture
    {
        MessageBus bus;

        [SetUp]
        public void SetUp()
        {
            bus = new MessageBus(DynamicGrainFactory.Instance);
        }

        [Test]
        public void When_more_than_one_handler_is_specified_for_command_within_the_same_grain()
        {
            Assert.Throws<MessageBus.DuplicateHandlesAttributeException>(() => bus.Register(new[] { 
                    typeof(GrainWhichHasDuplicateHandleAttribute)                     
            }));
        }

        [Test]
        public void When_more_than_one_handler_is_specified_for_command_in_different_grains()
        {
            Assert.Throws<MessageBus.DuplicateHandlesAttributeException>(()=> bus.Register(new[] { 
                    typeof(GrainWhichHandlesTestCommand), 
                    typeof(AnotherGrainWhichAlsoHandlesTestCommand) 
            }));
        }        
        
        [Test]
        public void When_more_than_one_handler_is_specified_for_query_within_the_same_grain()
        {
            Assert.Throws<MessageBus.DuplicateHandlesAttributeException>(() => bus.Register(new[] { 
                    typeof(GrainWhichHasDuplicateAnswersAttribute)                     
            }));
        }

        [Test]
        public void When_more_than_one_handler_is_specified_for_query_in_different_grains()
        {
            Assert.Throws<MessageBus.DuplicateHandlesAttributeException>(() => bus.Register(new[] { 
                    typeof(GrainWhichAnswersTestQuery), 
                    typeof(AnotherGrainWhichAlsoAnswersTestQuery) 
            }));
        }
    }

    class TestCommand
    {}

    class TestQuery
    {}

    [Handles(typeof(TestCommand))]
    [Handles(typeof(TestCommand))]
    [ExtendedPrimaryKey]
    public interface GrainWhichHasDuplicateHandleAttribute : IGrain
    {
        [Handler] Task Dispatch(object cmd);
    }

    [Handles(typeof(TestCommand))]
    [ExtendedPrimaryKey]
    public interface GrainWhichHandlesTestCommand : IGrain
    {
        [Handler] Task Dispatch(object cmd);
    }

    [Handles(typeof(TestCommand))]
    [ExtendedPrimaryKey]
    public interface AnotherGrainWhichAlsoHandlesTestCommand : IGrain
    {
        [Handler] Task Dispatch(object cmd);
    }

    [Answers(typeof(TestQuery))]
    [Answers(typeof(TestQuery))]
    [ExtendedPrimaryKey]
    public interface GrainWhichHasDuplicateAnswersAttribute : IGrain
    {
        [Handler] Task<object> Dispatch(object cmd);
    }

    [Answers(typeof(TestQuery))]
    [ExtendedPrimaryKey]
    public interface GrainWhichAnswersTestQuery : IGrain
    {
        [Handler] Task<object> Dispatch(object cmd);
    }

    [Answers(typeof(TestQuery))]
    [ExtendedPrimaryKey]
    public interface AnotherGrainWhichAlsoAnswersTestQuery : IGrain
    {
        [Handler] Task<object> Dispatch(object cmd);
    }
}