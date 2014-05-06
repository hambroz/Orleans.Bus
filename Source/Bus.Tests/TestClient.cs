using System;
using System.Linq;
using System.Threading;

namespace Orleans.Bus
{
    public class TestClient : Observes
    {
        public readonly EventWaitHandle EventReceived = new ManualResetEvent(false);

        public string Source;
        public string PublishedText;

        public void On(string source, object e)
        {
            this.On(source, (dynamic)e);
        }

        void On(string source, TextPublished e)
        {
            Source = source;
            PublishedText = e.Text;
            EventReceived.Set();
        }
    }
}