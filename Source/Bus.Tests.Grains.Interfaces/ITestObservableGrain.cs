using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
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

    [Handles(typeof(PublishText))]
    [Notifies(typeof(TextPublished))]
    [ExtendedPrimaryKey] 
    public interface ITestObservableGrain : IObservableGrain
    {
        [Dispatcher] Task Handle(object cmd);
    }
}
