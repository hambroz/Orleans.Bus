using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public class TestGrain : PocoGrain<Poco>, ITestGrain
    {
        public TestGrain()
        {
            Activate = grain =>
            {
                var poco = new Poco(Id(), Notify);
                return poco.Activate();
            };

            Handle = (poco, m) => poco.Handle((dynamic)m);
            Answer = async (poco, m) => await poco.Answer((dynamic)m);
        }
    }

    public class Poco
    {
        readonly string id;
        readonly Action<Event> notify;

        string fooText = "";
        string barText = "";

        public Poco(string id, Action<Event> notify)
        {
            this.id = id;
            this.notify = notify;
        }

        public Task<Poco> Activate()
        {
            return Task.FromResult(this);
        }

        public Task Handle(DoFoo cmd)
        {
            Console.WriteLine(id + " is executing " + cmd.Text);
            fooText = cmd.Text;

            return TaskDone.Done;
        }

        public Task Handle(DoBar cmd)
        {
            Console.WriteLine(id + " is executing " + cmd.Text);
            barText = cmd.Text;

            return TaskDone.Done;
        }        
        
        public Task Handle(ThrowException cmd)
        {
            throw new ApplicationException("Test exception unwrapping");
        }

        public Task<string> Answer(GetFoo query)
        {
            return Task.FromResult(fooText + "-" + id);
        }

        public Task<string> Answer(GetBar query)
        {
            return Task.FromResult(barText + "-" + id);
        }

        public Task Handle(PublishText cmd)
        {
            notify(new TextPublished(cmd.Text));

            return TaskDone.Done;
        }
    }
}
