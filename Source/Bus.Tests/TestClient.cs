using System;
using System.Linq;
using System.Threading;

namespace Orleans.Bus
{
    public class TestClient : Observes
    {
        public readonly EventWaitHandle EventReceived = new ManualResetEvent(false);

        public long SenderId = -1;
        public string PublishedText = "";

        public void On(object sender, object e)
        {
            this.On((long)sender, (dynamic)e);
        }

        void On(long sender, TextPublished e)
        {
            SenderId = sender;
            PublishedText = e.Text;
            EventReceived.Set();
        }
    }
}