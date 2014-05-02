using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    [Immutable]
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

    [Publisher(typeof(TextPublished))]
    public interface ITestObservableGrain : IObservableGrain, IGrainWithLongId
    {
        [Handler] Task Handle(PublishText cmd);
    }
}
