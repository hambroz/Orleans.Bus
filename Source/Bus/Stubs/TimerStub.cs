using System;
using System.Linq;

namespace Orleans.Bus.Stubs
{
    public class TimerStub : IOrleansTimer
    {
        readonly string name;

        public TimerStub(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        public void Dispose()
        {}
    }
}