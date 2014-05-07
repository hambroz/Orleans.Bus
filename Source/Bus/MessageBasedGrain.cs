using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Base class for all message based grains
    /// </summary>
    public abstract class MessageBasedGrain : GrainBase, IExposeGrainInternals
    {
        /// <summary>
        /// Reference to <see cref="IMessageBus"/>. Points to global runtime-bound implementation by default.
        /// </summary>
        public IMessageBus Bus;

        /// <summary>
        /// Reference to grain activation service. Points to runtime-bound implementation by default.
        /// </summary>
        public IActivation Activation;

        /// <summary>
        /// Reference to grain timers collection. Points to runtime-bound implementation by default.
        /// </summary>
        public ITimerCollection Timers;

        /// <summary>
        /// Reference to grain reminders collection. Points to runtime-bound implementation by default.
        /// </summary>
        public IReminderCollection Reminders;

        /// <summary>
        /// Default constructor, which initialize all local services to runtime-bound implementations by default.
        /// </summary>
        protected MessageBasedGrain()
        {
            Bus = MessageBus.Instance;
            Activation = new Activation(this);
            Timers = new TimerCollection(this);
            Reminders = new ReminderCollection(this);
        }

        void IExposeGrainInternals.DeactivateOnIdle()
        {
            DeactivateOnIdle();
        }

        void IExposeGrainInternals.DelayDeactivation(TimeSpan timeSpan)
        {
            DelayDeactivation(timeSpan);
        }

        Task<IOrleansReminder> IExposeGrainInternals.GetReminder(string reminderName)
        {
            return GetReminder(reminderName);
        }

        Task<List<IOrleansReminder>> IExposeGrainInternals.GetReminders()
        {
            return GetReminders();
        }

        Task<IOrleansReminder> IExposeGrainInternals.RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        Task IExposeGrainInternals.UnregisterReminder(IOrleansReminder reminder)
        {
            return UnregisterReminder(reminder);
        }

        IOrleansTimer IExposeGrainInternals.RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterTimer(asyncCallback, state, dueTime, period);
        }
    }

    /// <summary>
    /// Base class for all persistent message based grains
    /// </summary>
    public abstract class MessageBasedGrain<TState> : GrainBase<TState>, IExposeGrainInternals 
        where TState : class, IGrainState
    {
        /// <summary>
        /// Reference to <see cref="IMessageBus"/>. Points to global runtime-bound implementation by default.
        /// </summary>
        public IMessageBus Bus;

        /// <summary>
        /// Reference to grain activation service. Points to runtime-bound implementation by default.
        /// </summary>
        public IActivation Activation;

        /// <summary>
        /// Reference to grain timers collection. Points to runtime-bound implementation by default.
        /// </summary>
        public ITimerCollection Timers;

        /// <summary>
        /// Reference to grain reminders collection. Points to runtime-bound implementation by default.
        /// </summary>
        public IReminderCollection Reminders;

        /// <summary>
        /// Default constructor, which initialize all local services to runtime-bound implementations by default.
        /// </summary>
        protected MessageBasedGrain()
        {
            Bus = MessageBus.Instance;
            Activation = new Activation(this);
            Timers = new TimerCollection(this);
            Reminders = new ReminderCollection(this);
        }

        void IExposeGrainInternals.DeactivateOnIdle()
        {
            DeactivateOnIdle();
        }

        void IExposeGrainInternals.DelayDeactivation(TimeSpan timeSpan)
        {
            DelayDeactivation(timeSpan);
        }

        Task<IOrleansReminder> IExposeGrainInternals.GetReminder(string reminderName)
        {
            return GetReminder(reminderName);
        }

        Task<List<IOrleansReminder>> IExposeGrainInternals.GetReminders()
        {
            return GetReminders();
        }

        Task<IOrleansReminder> IExposeGrainInternals.RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        Task IExposeGrainInternals.UnregisterReminder(IOrleansReminder reminder)
        {
            return UnregisterReminder(reminder);
        }

        IOrleansTimer IExposeGrainInternals.RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return RegisterTimer(asyncCallback, state, dueTime, period);
        }
    }

    interface IExposeGrainInternals
    {
        void DeactivateOnIdle();
        void DelayDeactivation(TimeSpan timeSpan);

        Task<IOrleansReminder> GetReminder(string reminderName);
        Task<List<IOrleansReminder>> GetReminders();
        Task<IOrleansReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period);
        Task UnregisterReminder(IOrleansReminder reminder);

        IOrleansTimer RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period);         
    }
}