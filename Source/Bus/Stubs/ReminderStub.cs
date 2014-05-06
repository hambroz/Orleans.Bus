using System;
using System.Linq;

namespace Orleans.Bus.Stubs
{
    public class ReminderStub : IOrleansReminder
    {
        readonly string name;

        public ReminderStub(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        string IOrleansReminder.ReminderName
        {
            get { return name; }
        }
    }
}