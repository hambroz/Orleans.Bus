using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    [Immutable]
    public class DoFoo : Command
    {
        public readonly string Text;

        public DoFoo(string text)
        {
            Text = text;
        }
    }

    [Immutable]
    public class DoBar : Command
    {
        public readonly string Text;

        public DoBar(string text)
        {
            Text = text;
        }
    }

    [Immutable]
    public class GetFoo : Query<string>
    {}

    [Immutable]
    public class GetBar : Query<string>
    {}

    [ExtendedPrimaryKey]
    public interface ITestMessageBasedGrain : IGrain
    {
        [Handler] Task Handle(DoFoo cmd);
        [Handler] Task Handle(DoBar cmd);

        [Handler] Task<string> Answer(GetFoo query);
        [Handler] Task<string> Answer(GetBar query);
    }
}