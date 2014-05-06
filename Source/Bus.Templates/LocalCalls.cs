	#if GRAIN_STUBBING_ENABLED

    public class RegisteredTimer<TTimerState>
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

    public class UnregisteredTimer
    {
        public readonly string Name;

        public UnregisteredTimer(string name)
        {
            Name = name;
        }
    }

    public class RegisteredReminder
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

    public class UnregisteredReminder
    {
        public readonly string Name;

        public UnregisteredReminder(string name)
        {
            Name = name;
        }
    }

    public class RequestedDeactivationOnIdle
    {}

    public class RequestedDeactivationDelay
    {
        public readonly TimeSpan Period;

        public RequestedDeactivationDelay(TimeSpan period)
        {
            Period = period;
        }
    }

    class TimerStub : IOrleansTimer
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

    class ReminderStub : IOrleansReminder
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

#endif