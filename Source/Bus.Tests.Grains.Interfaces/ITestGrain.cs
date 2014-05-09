using System;
using System.Linq;

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

    [Immutable, Serializable]
    public class PublishText : Command
    {
        public readonly string Text;

        public PublishText(string text)
        {
            Text = text;
        }
    }

    [Serializable]
    public class TextPublished : Event
    {
        public readonly string Text;

        public TextPublished(string text)
        {
            Text = text;
        }
    }

    [Handles(typeof(DoFoo))]
    [Handles(typeof(DoBar))]
    [Handles(typeof(ThrowException))]
    [Answers(typeof(GetFoo))]
    [Answers(typeof(GetBar))]
    [Handles(typeof(PublishText))]
    [Notifies(typeof(TextPublished))]
    [ExtendedPrimaryKey]
    public interface ITestGrain : IPocoGrain
    {}
}