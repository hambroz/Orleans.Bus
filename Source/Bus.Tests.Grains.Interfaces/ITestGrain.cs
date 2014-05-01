using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    [Immutable]
    public class FooCommand : Command
    {
        public string Text;
    }

    [Immutable]
    public class BarCommand : Command
    {
        public string Text;
    }

    [Immutable]
    public class FooQuery : Query<string>
    {}

    [Immutable]
    public class BarQuery : Query<string>
    {}

    public interface ITestGrain : IGrain, IGrainWithLongId
    {
        [Handler] Task Handle(FooCommand command);
        [Handler] Task Handle(BarCommand command);

        [Handler] Task<string> Answer(FooQuery query);
        [Handler] Task<string> Answer(BarQuery query);
    }
}
