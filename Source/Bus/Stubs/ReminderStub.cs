using System;
using System.Linq;

namespace Orleans.Bus.Stubs
{
    public class ReminderStub : IOrleansReminder
    {
        string IOrleansReminder.ReminderName
        {
            get { throw new NotImplementedException(); }
        }
    }
}