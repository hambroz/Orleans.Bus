using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus.Stubs
{
    public abstract class GrainAuditEvent
    {}

    public class RegisteredTimer<TTimerState> : GrainAuditEvent
    {
        public readonly string Name;
        public readonly Func<TTimerState, Task> Callback;
        public readonly object State;
        public readonly TimeSpan Due;
        public readonly TimeSpan Period;

        public RegisteredTimer(string name, Func<TTimerState, Task> callback, TTimerState state, TimeSpan due, TimeSpan period)
        {
            Name = name;
            Callback = callback;
            State = state;
            Due = due;
            Period = period;
        }
    }

    public class UnregisteredTimer : GrainAuditEvent
    {
        public readonly string Name;

        public UnregisteredTimer(string name)
        {
            Name = name;
        }
    }

    public class RegisteredReminder : GrainAuditEvent
    {
        public readonly string Name;
        public readonly TimeSpan Due;
        public readonly TimeSpan Period;

        public RegisteredReminder(string name, TimeSpan due, TimeSpan period)
        {
            Name = name;
            Due = due;
            Period = period;
        }
    }

    public class UnregisteredReminder : GrainAuditEvent
    {
        public readonly string Name;

        public UnregisteredReminder(string name)
        {
            Name = name;
        }
    }

    public class RequestedDeactivationOnIdle : GrainAuditEvent
    {}

    public class RequestedDeactivationDelay : GrainAuditEvent
    {
        public readonly TimeSpan Period;

        public RequestedDeactivationDelay(TimeSpan period)
        {
            Period = period;
        }
    }
}
