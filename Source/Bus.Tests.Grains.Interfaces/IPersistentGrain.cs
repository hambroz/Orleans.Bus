using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    [Serializable, Immutable]
    public class SetValue : Command
    {
        public readonly int Value;

        public SetValue(int value)
        {
            Value = value;
        }
    }

    [Serializable, Immutable]
    public class ClearValue : Command
    {}

    [Serializable, Immutable]
    public class GetValue : Query<string>
    {}

    [Serializable]
    public class ValueChanged : Event
    {
        public readonly int Value;

        public ValueChanged(int value)
        {
            Value = value;
        }
    }

    [Handles(typeof(SetValue))]
    [Handles(typeof(ClearValue))]
    [Answers(typeof(GetValue))]
    [Notifies(typeof(ValueChanged))]
    [ExtendedPrimaryKey]
    public interface IPersistentGrain : IMessageBasedGrain
    {
        [Handler] Task HandleCommand(object cmd);
        [Handler] Task<object> AnswerQuery(object query);
    }

    public interface IPersistentGrainState : IGrainState
    {
        int Total { get; set; }
    }
}
