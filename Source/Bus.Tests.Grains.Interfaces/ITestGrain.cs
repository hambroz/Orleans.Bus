using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    [Immutable, Serializable]
    public class DoFoo : Command
    {
        public readonly string Text;

        public DoFoo(string text)
        {
            Text = text;
        }
    }

    [Immutable, Serializable]
    public class DoBar : Command
    {
        public readonly string Text;

        public DoBar(string text)
        {
            Text = text;
        }
    }

    [Immutable, Serializable]
    public class ThrowException : Command
    {}

    [Immutable, Serializable]
    public class GetFoo : Query<string>
    {}

    [Immutable, Serializable]
    public class GetBar : Query<string>
    {}

    [Handles(typeof(DoFoo))]
    [Handles(typeof(DoBar))]
    [Handles(typeof(ThrowException))]
    [Answers(typeof(GetFoo))]
    [Answers(typeof(GetBar))]
    [ExtendedPrimaryKey]
    public interface ITestGrain : IGrain
    {
        [Dispatcher] Task Handle(object cmd);
        [Dispatcher] Task<object> Answer(object query);
    }
}