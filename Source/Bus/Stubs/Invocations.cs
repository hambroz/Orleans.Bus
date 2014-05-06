using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus.Stubs
{
    public abstract class Invocation
    {}

    public class RegisteredTimer<TTimerState> : Invocation
    {
        public readonly string Id;
        public readonly Func<TTimerState, Task> Callback;
        public readonly object State;
        public readonly TimeSpan Due;
        public readonly TimeSpan Period;

        public RegisteredTimer(string id, Func<TTimerState, Task> callback, TTimerState state, TimeSpan due, TimeSpan period)
        {
            Id = id;
            Callback = callback;
            State = state;
            Due = due;
            Period = period;
        }
    }

    public class UnregisteredTimer : Invocation
    {
        public readonly string Id;

        public UnregisteredTimer(string id)
        {
            Id = id;
        }
    }

    public class RegisteredReminder : Invocation
    {
        public readonly string Id;
        public readonly TimeSpan Due;
        public readonly TimeSpan Period;

        public RegisteredReminder(string id, TimeSpan due, TimeSpan period)
        {
            Id = id;
            Due = due;
            Period = period;
        }
    }

    public class UnregisteredReminder : Invocation
    {
        public readonly string Id;

        public UnregisteredReminder(string id)
        {
            Id = id;
        }
    }

    public class RequestedDeactivationOnIdle : Invocation
    {}

    public class RequestedDeactivationDelay : Invocation
    {
        public readonly TimeSpan Period;

        public RequestedDeactivationDelay(TimeSpan period)
        {
            Period = period;
        }
    }
}
