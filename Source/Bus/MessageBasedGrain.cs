using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Base interface for all message-based grains
    /// </summary>
    public interface IMessageBasedGrain : IObservableGrain
    {}

    /// <summary>
    /// Base class for all message based grains
    /// </summary>
    public abstract class MessageBasedGrain : GrainBase, IMessageBasedGrain, IExposeGrainInternals
    {
        /// <summary>
        /// Reference to <see cref="IMessageBus"/>. Points to global runtime-bound implementation by default.
        /// </summary>
        public IMessageBus Bus;

        /// <summary>
        /// Returns identity of this grain. Points to runtime-bound implementation by default.
        /// </summary>
        public Func<string> Id;

        /// <summary>
        /// Reference to grain timers collection. Points to runtime-bound implementation by default.
        /// </summary>
        public ITimerCollection Timers;

        /// <summary>
        /// Reference to grain reminders collection. Points to runtime-bound implementation by default.
        /// </summary>
        public IReminderCollection Reminders;

        /// <summary>
        /// Reference to grain activation service. Points to runtime-bound implementation by default.
        /// </summary>
        public IActivation Activation;

        /// <summary>
        /// Reference to observer collection. Points to runtime-bound implementation by default.
        /// </summary>
        public IObserverCollection Observers;

        /// <summary>
        /// Default constructor, which initialize all local services to runtime-bound implementations by default.
        /// </summary>
        protected MessageBasedGrain()
        {
            Bus = MessageBus.Instance;
            Id  = () => Identity.Of(this);
            Timers = new TimerCollection(this);
            Reminders = new ReminderCollection(this);
            Observers = new ObserverCollection();
            Activation = new Activation(this);
        }

        /// <summary>
        /// Attaches untyped observer for the given type of event.
        /// </summary>
        /// <param name="o">The observer proxy.</param>
        /// <param name="e">The type of event</param>
        /// <remarks>The operation is idempotent</remarks>
        public virtual Task Attach(IObserve o, Type e)
        {
            Observers.Attach(o, e);
            return TaskDone.Done;
        }

        /// <summary>
        /// Detaches given untyped observer for the given type of event.
        /// </summary>
        /// <param name="o">The observer proxy.</param>
        /// <param name="e">The type of event</param>
        /// <remarks>The operation is idempotent</remarks>
        public virtual Task Detach(IObserve o, Type e)
        {
            Observers.Detach(o, e);
            return TaskDone.Done;
        }

        /// <summary>
        /// Notifies all attached observers about given event.
        /// </summary>
        /// <param name="e">An event</param>
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(), e);
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
    public abstract class MessageBasedGrain<TState> : GrainBase<TState>, IMessageBasedGrain, IExposeGrainInternals 
        where TState : class, IGrainState
    {
        /// <summary>
        /// Reference to <see cref="IMessageBus"/>. Points to global runtime-bound implementation by default.
        /// </summary>
        public IMessageBus Bus;

        /// <summary>
        /// Returns identity of this grain. Points to runtime-bound implementation by default.
        /// </summary>
        public Func<string> Id;

        /// <summary>
        /// Reference to grain timers collection. Points to runtime-bound implementation by default.
        /// </summary>
        public ITimerCollection Timers;

        /// <summary>
        /// Reference to grain reminders collection. Points to runtime-bound implementation by default.
        /// </summary>
        public IReminderCollection Reminders;

        /// <summary>
        /// Reference to grain activation service. Points to runtime-bound implementation by default.
        /// </summary>
        public IActivation Activation;

        /// <summary>
        /// Reference to observer collection. Points to runtime-bound implementation by default.
        /// </summary>
        public IObserverCollection Observers;

        /// <summary>
        /// Default constructor, which initialize all local services to runtime-bound implementations by default.
        /// </summary>
        protected MessageBasedGrain()
        {
            Bus = MessageBus.Instance;
            Id = () => Identity.Of(this);
            Timers = new TimerCollection(this);
            Reminders = new ReminderCollection(this);
            Observers = new ObserverCollection();
            Activation = new Activation(this);
        }

        /// <summary>
        /// Attaches untyped observer for the given type of event.
        /// </summary>
        /// <param name="o">The observer proxy.</param>
        /// <param name="e">The type of event</param>
        /// <remarks>The operation is idempotent</remarks>
        public virtual Task Attach(IObserve o, Type e)
        {
            Observers.Attach(o, e);
            return TaskDone.Done;
        }

        /// <summary>
        /// Detaches given untyped observer for the given type of event.
        /// </summary>
        /// <param name="o">The observer proxy.</param>
        /// <param name="e">The type of event</param>
        /// <remarks>The operation is idempotent</remarks>
        public virtual Task Detach(IObserve o, Type e)
        {
            Observers.Detach(o, e);
            return TaskDone.Done;
        }

        /// <summary>
        /// Notifies all attached observers about given event.
        /// </summary>
        /// <param name="e">An event</param>
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(), e);
        }

        /// <summary>
        /// Strongly typed accessor for the grain state
        /// </summary>
        /// <remarks>Could be substituted for unit-testing purposes</remarks>
        public new TState State
        {
            get; set;
        }

        /// <summary>
        /// This method is called at the end of the process of activating a grain.
        /// It is called before any messages have been dispatched to the grain.
        /// For grains with declared persistent state, this method is called after the State property has been populated.
        /// </summary>
        public override Task ActivateAsync()
        {
            State = base.State;
            return TaskDone.Done;
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