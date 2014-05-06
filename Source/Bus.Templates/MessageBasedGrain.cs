﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Base class for all kinds of message based grains
    /// </summary>
    public abstract class MessageBasedGrain : GrainBase, IGrain
    {
        /// <summary>
        /// An instance of <see cref="IMessageBus"/> pointing to global instance by default
        /// </summary>
        public IMessageBus Bus = MessageBus.Instance;

        #region Message exchange shortcuts

        /// <summary>
        /// Sends command message to a grain with the given <see cref="Guid"/> id
        /// </summary>
        /// <typeparam name="TCommand">The type of command mesage</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task Send<TCommand>(Guid id, TCommand command)
        {
            return Bus.Send(id, command);
        }
        
        /// <summary>
        /// Sends command message to a grain with the given <see cref="Int64"/> id
        /// </summary>
        /// <typeparam name="TCommand">The type of command mesage</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task Send<TCommand>(long id, TCommand command)
        {
            return Bus.Send(id, command);
        }
        
        /// <summary>
        /// Sends command message to a grain with the given <see cref="string"/> id
        /// </summary>
        /// <typeparam name="TCommand">The type of command mesage</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task Send<TCommand>(string id, TCommand command)
        {
            return Bus.Send(id, command);
        }

        /// <summary>
        /// Sends query message to a grain with the given  <see cref="Guid"/> id and casts result to the specified type
        /// </summary>
        /// <typeparam name="TQuery">The type of query mesage</typeparam>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> Query<TQuery, TResult>(Guid id, TQuery query)        
        {
            return Bus.Query<TQuery, TResult>(id, query);
        }
        
        /// <summary>
        /// Sends query message to a grain with the given  <see cref="Int64"/> id
        /// and casts result to the specified type
        /// </summary>
        /// <typeparam name="TQuery">The type of query mesage</typeparam>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> Query<TQuery, TResult>(long id, TQuery query)        
        {
            return Bus.Query<TQuery, TResult>(id, query);
        }
        
        /// <summary>
        /// Sends query message to a grain with the given  <see cref="String"/> id and casts result to the specified type
        /// </summary>
        /// <typeparam name="TQuery">The type of query mesage</typeparam>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> Query<TQuery, TResult>(string id, TQuery query)        
        {
            return Bus.Query<TQuery, TResult>(id, query);
        }

        #endregion

        #region Obsolete base class methods

        /// <summary>
        /// OBSOLETE! Use <see cref="RegisterTimer{TTimerState}(string,System.Func{TTimerState,System.Threading.Tasks.Task}, TTimerState, TimeSpan,TimeSpan)"/> overload which enables named Timers 
        /// and  automatically tracks all registered timers
        /// </summary>
        protected new IOrleansTimer RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            throw new InvalidOperationException("Use RegisterTimer() overload which automatically tracks all registered timers");
        }

        /// <summary>
        /// OBSOLETE! Use <see cref="RegisterReminder"/> overload which 
        /// automatically tracks all registered reminders
        /// </summary>
        protected new Task<IOrleansReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            throw new InvalidOperationException("Use RegisterReminder() overload which automatically tracks all registered reminders");
        }

        /// <summary>
        /// OBSOLETE! Use <see cref="UnregisterReminder(string)"/> overload which takes reminder name argument
        /// </summary>
        protected new Task UnregisterReminder(IOrleansReminder reminder)
        {
            throw new InvalidOperationException("Use UnregisterReminder() overload which takes reminder name argument");
        }

        /// <summary>
        /// OBSOLETE! Use combination of <see cref="IsReminderRegistered"/> method and <see cref="UnregisterReminder(string)"/> overload to check and unregister reminder
        /// </summary>
        protected new Task<IOrleansReminder> GetReminder(string reminderName)
        {
            throw new InvalidOperationException("Use combination of IsReminderRegistered() method and UnregisterReminder() overload to check and unregister reminder");
        }

        /// <summary>
        /// OBSOLETE! Use <see cref="RegisteredReminders"/> method to get names of all currently registered reminders
        /// </summary>
        protected new Task<List<IOrleansReminder>> GetReminders()
        {
            throw new InvalidOperationException("Use RegisteredReminders() method to get names of all currently registered reminders");
        }

        #endregion
        
        #if GRAIN_STUBBING_ENABLED

        /// <summary>
        /// Track all instance-level events (like timer registrations, deactivation request, etc). Useful for testing purposes. 
        /// Works only if build specifies <c>GRAIN_STUBBING_ENABLED</c> constant!
        /// </summary>
        public IList<object> Dispatched = new List<object>();

        #endif

        #region Timers

        /// <summary>
        /// Tracks all currently registered timers
        /// </summary>
        readonly IDictionary<string, IOrleansTimer> timers = new Dictionary<string, IOrleansTimer>();

        /// <summary>
        /// Registers a timer to send periodic callbacks to this grain.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>
        /// This timer will not prevent the current grain from being deactivated.
        ///             If the grain is deactivated, then the timer will be discarded.
        /// 
        /// </para>
        /// 
        /// <para>
        /// Until the Task returned from the <paramref name="callback"/> is resolved,
        ///             the next timer tick will not be scheduled.
        ///             That is to say, timer callbacks never interleave their turns.
        /// 
        /// </para>
        /// 
        /// <para>
        /// The timer may be stopped at any time by calling the <see cref="UnregisterTimer(string)"/> method
        /// 
        /// </para>
        /// 
        /// <para>
        /// Any exceptions thrown by or faulted Task's returned from the  <paramref name="callback"/>
        ///             will be logged, but will not prevent the next timer tick from being queued.
        /// 
        /// </para>
        /// 
        /// </remarks>
        /// <param name="name">Name of the timer</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        /// <param name="state">State object that will be passed as argument when calling the  <paramref name="callback"/>.</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        protected void RegisterTimer<TTimerState>(string name, Func<TTimerState, Task> callback, TTimerState state, TimeSpan due, TimeSpan period)
        {
            IOrleansTimer timer;

            #if GRAIN_STUBBING_ENABLED

            timer = new TimerStub(name);
            Dispatched.Add(new RegisteredTimer<TTimerState>(name, callback, state, due, period));
            
            #else
            timer = base.RegisterTimer(s => callback((TTimerState) s), state, due, period);
            #endif

            timers.Add(name, timer);
        }

        /// <summary>
        /// Unregister previously registered timer. 
        /// </summary>
        /// <param name="name">Name of the timer</param>
        protected void UnregisterTimer(string name)
        {
            var timer = timers[name];
            timer.Dispose();

            #if GRAIN_STUBBING_ENABLED
            Dispatched.Add(new UnregisteredTimer(name));
            #endif
        }

        /// <summary>
        /// Checks whether timer with the given name was registered before
        /// </summary>
        /// <param name="name">Name of the timer</param>
        /// <returns><c>true</c> if timer was the give name was previously registered, <c>false</c> otherwise </returns>
        protected bool IsTimerRegistered(string name)
        {
            return timers.ContainsKey(name);
        }

        /// <summary>
        /// Returns names of all currently registered timers
        /// </summary>
        /// <returns>A sequence of <see cref="string"/> names</returns>
        protected IEnumerable<string> RegisteredTimers()
        {
            return timers.Keys;
        }

        #endregion

        #region Reminders

        /// <summary>
        /// Tracks all currently registered reminders
        /// </summary>
        readonly IDictionary<string, IOrleansReminder> reminders = new Dictionary<string, IOrleansReminder>();

        /// <summary>
        /// Registers a persistent, reliable reminder to send regular notifications (Reminders) to the grain.
        ///             The grain must implement the <c>Orleans.IRemindable</c> interface, and Reminders for this grain will be sent to the <c>ReceiveReminder</c> callback method.
        ///             If the current grain is deactivated when the timer fires, a new activation of this grain will be created to receive this reminder.
        ///             If an existing reminder with the same name already exists, that reminder will be overwritten with this new reminder.
        ///             Reminders will always be received by one activation of this grain, even if multiple activations exist for this grain.
        /// 
        /// </summary>
        /// <param name="name">Name of this reminder</param>
        /// <param name="due">Due time for this reminder</param>
        /// <param name="period">Frequence period for this reminder</param>
        /// <returns>
        /// Promise for Reminder registration.
        /// </returns>
        protected 
             #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task RegisterReminder(string name, TimeSpan due, TimeSpan period)
        {
            #if GRAIN_STUBBING_ENABLED
            
            var reminder = new ReminderStub(name);
            reminders[name] = reminder;

            Dispatched.Add(new RegisteredReminder(name, due, period));
            return TaskDone.Done;
            
            #else
            var reminder = await base.RegisterOrUpdateReminder(name, due, period);
            reminders[name] = reminder;
            #endif
        }

        /// <summary>
        /// Unregister previously registered peristent reminder if any
        /// </summary>
        /// <param name="name">Name of the reminder</param>
        protected
            #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task UnregisterReminder(string name)
        {
            #if GRAIN_STUBBING_ENABLED
            
            reminders.Remove(name);
            Dispatched.Add(new UnregisteredReminder(name));
            return TaskDone.Done;
            
            #else
            
            reminders.Remove(name);
            var reminder = await base.GetReminder(name);
            if (reminder != null)
                await base.UnregisterReminder(reminder);

            #endif
        }

        /// <summary>
        /// Checks whether reminder with the given name is currently registered
        /// </summary>
        /// <param name="name">Name of the reminder</param>
        /// <returns><c>true</c> if reminder with the give name is currently registered, <c>false</c> otherwise </returns>
        protected
            #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task<bool> IsReminderRegistered(string name)
        {
            #if GRAIN_STUBBING_ENABLED
            return Task.FromResult(reminders.ContainsKey(name));
            #else
            return reminders.ContainsKey(name) || (await base.GetReminder(name)) != null;
            #endif
        }

        /// <summary>
        /// Returns names of all currently registered reminders
        /// </summary>
        /// <returns>A sequence of <see cref="string"/> names</returns>
        protected
            #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task<IEnumerable<string>> RegisteredReminders()
        {
            #if GRAIN_STUBBING_ENABLED
            return Task.FromResult(reminders.Keys.AsEnumerable());
            #else
            return (await base.GetReminders()).Select(x => x.ReminderName);
            #endif
        }

        #endregion

        #region Deactivation

        /// <summary>
        /// Deactivate this activation of the grain after the current grain method call is completed.
        ///             This call will mark this activation of the current grain to be deactivated and removed at the end of the current method.
        ///             The next call to this grain will result in a different activation to be used, which typical means a new activation will be created automatically by the runtime.
        /// 
        /// </summary>
        protected new void DeactivateOnIdle()
        {
            #if GRAIN_STUBBING_ENABLED
            Dispatched.Add(new RequestedDeactivationOnIdle());
            #else
            base.DeactivateOnIdle();
            #endif
        }

        /// <summary>
        /// Delay Deactivation of this activation at least for the specified time duration.
        ///             DeactivateOnIdle method would undo / override any current “keep alive” setting,
        ///             making this grain immediately available  for deactivation.
        /// 
        /// </summary>
        /// <param name="period">
        /// <para>A positive value means “prevent GC of this activation for that time span”</para> 
        /// <para>A negative value means “unlock, and make this activation available for GC again”</para>
        /// </param>
        protected new void DelayDeactivation(TimeSpan period)
        {
            #if GRAIN_STUBBING_ENABLED
            Dispatched.Add(new RequestedDeactivationDelay(period));
            #else
            base.DelayDeactivation(period);
            #endif
        }

        #endregion
	}

    /// <summary>
    /// Base class for all kinds of persistent message based grains
    /// </summary>
    public abstract class MessageBasedGrain<TState> : GrainBase<TState>, IGrain 
        where TState : class, IGrainState
    {
        /// <summary>
        /// An instance of <see cref="IMessageBus"/> pointing to global instance by default
        /// </summary>
        public IMessageBus Bus = MessageBus.Instance;

        #region Message exchange shortcuts

        /// <summary>
        /// Sends command message to a grain with the given <see cref="Guid"/> id
        /// </summary>
        /// <typeparam name="TCommand">The type of command mesage</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task Send<TCommand>(Guid id, TCommand command)
        {
            return Bus.Send(id, command);
        }
        
        /// <summary>
        /// Sends command message to a grain with the given <see cref="Int64"/> id
        /// </summary>
        /// <typeparam name="TCommand">The type of command mesage</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task Send<TCommand>(long id, TCommand command)
        {
            return Bus.Send(id, command);
        }
        
        /// <summary>
        /// Sends command message to a grain with the given <see cref="string"/> id
        /// </summary>
        /// <typeparam name="TCommand">The type of command mesage</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="command">The command to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task Send<TCommand>(string id, TCommand command)
        {
            return Bus.Send(id, command);
        }

        /// <summary>
        /// Sends query message to a grain with the given  <see cref="Guid"/> id and casts result to the specified type
        /// </summary>
        /// <typeparam name="TQuery">The type of query mesage</typeparam>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> Query<TQuery, TResult>(Guid id, TQuery query)        
        {
            return Bus.Query<TQuery, TResult>(id, query);
        }
        
        /// <summary>
        /// Sends query message to a grain with the given  <see cref="Int64"/> id
        /// and casts result to the specified type
        /// </summary>
        /// <typeparam name="TQuery">The type of query mesage</typeparam>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> Query<TQuery, TResult>(long id, TQuery query)        
        {
            return Bus.Query<TQuery, TResult>(id, query);
        }
        
        /// <summary>
        /// Sends query message to a grain with the given  <see cref="String"/> id and casts result to the specified type
        /// </summary>
        /// <typeparam name="TQuery">The type of query mesage</typeparam>
        /// <typeparam name="TResult">The type of result</typeparam>
        /// <param name="id">Id of a grain</param>
        /// <param name="query">The query to send</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Task<TResult> Query<TQuery, TResult>(string id, TQuery query)        
        {
            return Bus.Query<TQuery, TResult>(id, query);
        }

        #endregion

        #region Obsolete base class methods

        /// <summary>
        /// OBSOLETE! Use <see cref="RegisterTimer{TTimerState}(string,System.Func{TTimerState,System.Threading.Tasks.Task}, TTimerState, TimeSpan,TimeSpan)"/> overload which enables named Timers 
        /// and  automatically tracks all registered timers
        /// </summary>
        protected new IOrleansTimer RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            throw new InvalidOperationException("Use RegisterTimer() overload which automatically tracks all registered timers");
        }

        /// <summary>
        /// OBSOLETE! Use <see cref="RegisterReminder"/> overload which 
        /// automatically tracks all registered reminders
        /// </summary>
        protected new Task<IOrleansReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            throw new InvalidOperationException("Use RegisterReminder() overload which automatically tracks all registered reminders");
        }

        /// <summary>
        /// OBSOLETE! Use <see cref="UnregisterReminder(string)"/> overload which takes reminder name argument
        /// </summary>
        protected new Task UnregisterReminder(IOrleansReminder reminder)
        {
            throw new InvalidOperationException("Use UnregisterReminder() overload which takes reminder name argument");
        }

        /// <summary>
        /// OBSOLETE! Use combination of <see cref="IsReminderRegistered"/> method and <see cref="UnregisterReminder(string)"/> overload to check and unregister reminder
        /// </summary>
        protected new Task<IOrleansReminder> GetReminder(string reminderName)
        {
            throw new InvalidOperationException("Use combination of IsReminderRegistered() method and UnregisterReminder() overload to check and unregister reminder");
        }

        /// <summary>
        /// OBSOLETE! Use <see cref="RegisteredReminders"/> method to get names of all currently registered reminders
        /// </summary>
        protected new Task<List<IOrleansReminder>> GetReminders()
        {
            throw new InvalidOperationException("Use RegisteredReminders() method to get names of all currently registered reminders");
        }

        #endregion
        
        #if GRAIN_STUBBING_ENABLED

        /// <summary>
        /// Track all instance-level events (like timer registrations, deactivation request, etc). Useful for testing purposes. 
        /// Works only if build specifies <c>GRAIN_STUBBING_ENABLED</c> constant!
        /// </summary>
        public IList<object> Dispatched = new List<object>();

        #endif

        #region Timers

        /// <summary>
        /// Tracks all currently registered timers
        /// </summary>
        readonly IDictionary<string, IOrleansTimer> timers = new Dictionary<string, IOrleansTimer>();

        /// <summary>
        /// Registers a timer to send periodic callbacks to this grain.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>
        /// This timer will not prevent the current grain from being deactivated.
        ///             If the grain is deactivated, then the timer will be discarded.
        /// 
        /// </para>
        /// 
        /// <para>
        /// Until the Task returned from the <paramref name="callback"/> is resolved,
        ///             the next timer tick will not be scheduled.
        ///             That is to say, timer callbacks never interleave their turns.
        /// 
        /// </para>
        /// 
        /// <para>
        /// The timer may be stopped at any time by calling the <see cref="UnregisterTimer(string)"/> method
        /// 
        /// </para>
        /// 
        /// <para>
        /// Any exceptions thrown by or faulted Task's returned from the  <paramref name="callback"/>
        ///             will be logged, but will not prevent the next timer tick from being queued.
        /// 
        /// </para>
        /// 
        /// </remarks>
        /// <param name="name">Name of the timer</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        /// <param name="state">State object that will be passed as argument when calling the  <paramref name="callback"/>.</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        protected void RegisterTimer<TTimerState>(string name, Func<TTimerState, Task> callback, TTimerState state, TimeSpan due, TimeSpan period)
        {
            IOrleansTimer timer;

            #if GRAIN_STUBBING_ENABLED

            timer = new TimerStub(name);
            Dispatched.Add(new RegisteredTimer<TTimerState>(name, callback, state, due, period));
            
            #else
            timer = base.RegisterTimer(s => callback((TTimerState) s), state, due, period);
            #endif

            timers.Add(name, timer);
        }

        /// <summary>
        /// Unregister previously registered timer. 
        /// </summary>
        /// <param name="name">Name of the timer</param>
        protected void UnregisterTimer(string name)
        {
            var timer = timers[name];
            timer.Dispose();

            #if GRAIN_STUBBING_ENABLED
            Dispatched.Add(new UnregisteredTimer(name));
            #endif
        }

        /// <summary>
        /// Checks whether timer with the given name was registered before
        /// </summary>
        /// <param name="name">Name of the timer</param>
        /// <returns><c>true</c> if timer was the give name was previously registered, <c>false</c> otherwise </returns>
        protected bool IsTimerRegistered(string name)
        {
            return timers.ContainsKey(name);
        }

        /// <summary>
        /// Returns names of all currently registered timers
        /// </summary>
        /// <returns>A sequence of <see cref="string"/> names</returns>
        protected IEnumerable<string> RegisteredTimers()
        {
            return timers.Keys;
        }

        #endregion

        #region Reminders

        /// <summary>
        /// Tracks all currently registered reminders
        /// </summary>
        readonly IDictionary<string, IOrleansReminder> reminders = new Dictionary<string, IOrleansReminder>();

        /// <summary>
        /// Registers a persistent, reliable reminder to send regular notifications (Reminders) to the grain.
        ///             The grain must implement the <c>Orleans.IRemindable</c> interface, and Reminders for this grain will be sent to the <c>ReceiveReminder</c> callback method.
        ///             If the current grain is deactivated when the timer fires, a new activation of this grain will be created to receive this reminder.
        ///             If an existing reminder with the same name already exists, that reminder will be overwritten with this new reminder.
        ///             Reminders will always be received by one activation of this grain, even if multiple activations exist for this grain.
        /// 
        /// </summary>
        /// <param name="name">Name of this reminder</param>
        /// <param name="due">Due time for this reminder</param>
        /// <param name="period">Frequence period for this reminder</param>
        /// <returns>
        /// Promise for Reminder registration.
        /// </returns>
        protected 
             #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task RegisterReminder(string name, TimeSpan due, TimeSpan period)
        {
            #if GRAIN_STUBBING_ENABLED
            
            var reminder = new ReminderStub(name);
            reminders[name] = reminder;

            Dispatched.Add(new RegisteredReminder(name, due, period));
            return TaskDone.Done;
            
            #else
            var reminder = await base.RegisterOrUpdateReminder(name, due, period);
            reminders[name] = reminder;
            #endif
        }

        /// <summary>
        /// Unregister previously registered peristent reminder if any
        /// </summary>
        /// <param name="name">Name of the reminder</param>
        protected
            #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task UnregisterReminder(string name)
        {
            #if GRAIN_STUBBING_ENABLED
            
            reminders.Remove(name);
            Dispatched.Add(new UnregisteredReminder(name));
            return TaskDone.Done;
            
            #else
            
            reminders.Remove(name);
            var reminder = await base.GetReminder(name);
            if (reminder != null)
                await base.UnregisterReminder(reminder);

            #endif
        }

        /// <summary>
        /// Checks whether reminder with the given name is currently registered
        /// </summary>
        /// <param name="name">Name of the reminder</param>
        /// <returns><c>true</c> if reminder with the give name is currently registered, <c>false</c> otherwise </returns>
        protected
            #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task<bool> IsReminderRegistered(string name)
        {
            #if GRAIN_STUBBING_ENABLED
            return Task.FromResult(reminders.ContainsKey(name));
            #else
            return reminders.ContainsKey(name) || (await base.GetReminder(name)) != null;
            #endif
        }

        /// <summary>
        /// Returns names of all currently registered reminders
        /// </summary>
        /// <returns>A sequence of <see cref="string"/> names</returns>
        protected
            #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task<IEnumerable<string>> RegisteredReminders()
        {
            #if GRAIN_STUBBING_ENABLED
            return Task.FromResult(reminders.Keys.AsEnumerable());
            #else
            return (await base.GetReminders()).Select(x => x.ReminderName);
            #endif
        }

        #endregion

        #region Deactivation

        /// <summary>
        /// Deactivate this activation of the grain after the current grain method call is completed.
        ///             This call will mark this activation of the current grain to be deactivated and removed at the end of the current method.
        ///             The next call to this grain will result in a different activation to be used, which typical means a new activation will be created automatically by the runtime.
        /// 
        /// </summary>
        protected new void DeactivateOnIdle()
        {
            #if GRAIN_STUBBING_ENABLED
            Dispatched.Add(new RequestedDeactivationOnIdle());
            #else
            base.DeactivateOnIdle();
            #endif
        }

        /// <summary>
        /// Delay Deactivation of this activation at least for the specified time duration.
        ///             DeactivateOnIdle method would undo / override any current “keep alive” setting,
        ///             making this grain immediately available  for deactivation.
        /// 
        /// </summary>
        /// <param name="period">
        /// <para>A positive value means “prevent GC of this activation for that time span”</para> 
        /// <para>A negative value means “unlock, and make this activation available for GC again”</para>
        /// </param>
        protected new void DelayDeactivation(TimeSpan period)
        {
            #if GRAIN_STUBBING_ENABLED
            Dispatched.Add(new RequestedDeactivationDelay(period));
            #else
            base.DelayDeactivation(period);
            #endif
        }

        #endregion
		        
		#if GRAIN_STUBBING_ENABLED

		/// <summary>
        /// Sets grain's state for testing purposes
        /// </summary>
        public void SetState(TState state)
        {
			#if DEBUG
			explicitState = state;
			#endif
		}
				
		TState explicitState;
       
		/// <summary>
        /// Gets grain's state
        /// </summary>
        protected new TState State
        {
            get { return explicitState; }
        }

		#endif
	}

    /// <summary>
    /// Base class for message based grains identifiable by <see cref="Guid"/> identifier
    /// </summary>
    public abstract class MessageBasedGrainWithGuidId : MessageBasedGrain, IHaveGuidId
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Guid id)
        {
			explicitId = id;
		}
		        
		Guid explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Guid"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }
    }

    /// <summary>
    /// Base class for message based grains identifiable by <see cref="Int64"/> identifier
    /// </summary>
    public abstract class MessageBasedGrainWithInt64Id : MessageBasedGrain, IHaveInt64Id
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Int64 id)
        {
			explicitId = id;
		}
		        
		Int64 explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Int64"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Int64 Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }
    }

    /// <summary>
    /// Base class for message based grains identifiable by <see cref="String"/> identifier
    /// </summary>
    public abstract class MessageBasedGrainWithStringId : MessageBasedGrain, IHaveStringId
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(String id)
        {
			explicitId = id;
		}
		        
		String explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="String"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected String Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }
    }

    /// <summary>
    /// Base class for persistent message based grains identifiable by <see cref="Guid"/> identifier
    /// </summary>
    public abstract class MessageBasedGrainWithGuidId<TState> : MessageBasedGrain<TState>, IHaveGuidId
	        where TState : class, IGrainState
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Guid id)
        {
			explicitId = id;
		}
		        
		Guid explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Guid"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }
		
    }

    /// <summary>
    /// Base class for persistent message based grains identifiable by <see cref="Int64"/> identifier
    /// </summary>
    public abstract class MessageBasedGrainWithInt64Id<TState> : MessageBasedGrain<TState>, IHaveInt64Id
	        where TState : class, IGrainState
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Int64 id)
        {
			explicitId = id;
		}
		        
		Int64 explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Int64"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Int64 Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }
		
    }

    /// <summary>
    /// Base class for persistent message based grains identifiable by <see cref="String"/> identifier
    /// </summary>
    public abstract class MessageBasedGrainWithStringId<TState> : MessageBasedGrain<TState>, IHaveStringId
	        where TState : class, IGrainState
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(String id)
        {
			explicitId = id;
		}
		        
		String explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="String"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected String Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }
		
    }

	/// <summary>
    /// Base class for all kinds of observable message based grains
    /// </summary>
    public abstract class ObservableMessageBasedGrain : MessageBasedGrain, IObservableGrain
    {
        /// <summary>
        /// Default instance of <see cref="IObserverCollection"/> of the current grain.
        /// </summary>
        /// <remarks>You can substitute it within a test harness</remarks>
        public IObserverCollection Observers = new ObserverCollection();

        /// <summary>
        /// Attaches given observer to receive events of the specified type 
        /// </summary>
        /// <param name="o">Client observer proxy</param>
        /// <param name="e">The type of event</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task Attach(Observes o, Type e)
        {
            Observers.Attach(o, e);
            return TaskDone.Done;
        }

        /// <summary>
        /// Detaches given observer from receiving events of the specified type 
        /// </summary>
        /// <param name="o">Client observer proxy</param>
        /// <param name="e">The type of event</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task Detach(Observes o, Type e)
        {
            Observers.Detach(o, e);
            return TaskDone.Done;
        }
    }

    /// <summary>
    /// Base class for all kinds of persitent observable message based grains
    /// </summary>
    public abstract class ObservableMessageBasedGrain<TGrainState> : MessageBasedGrain<TGrainState>, IObservableGrain
        where TGrainState : class, IGrainState
    {
        /// <summary>
        /// Default instance of <see cref="IObserverCollection"/> of the current grain.
        /// </summary>
        /// <remarks>You can substitute it within a test harness</remarks>
        public IObserverCollection Observers = new ObserverCollection();

        /// <summary>
        /// Attaches given observer to receive events of the specified type 
        /// </summary>
        /// <param name="o">Client observer proxy</param>
        /// <param name="e">The type of event</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task Attach(Observes o, Type e)
        {
            Observers.Attach(o, e);
            return TaskDone.Done;
        }

        /// <summary>
        /// Detaches given observer from receiving events of the specified type 
        /// </summary>
        /// <param name="o">Client observer proxy</param>
        /// <param name="e">The type of event</param>
        /// <returns>Promise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task Detach(Observes o, Type e)
        {
            Observers.Detach(o, e);
            return TaskDone.Done;
        }
    }

    /// <summary>
    /// Base class for observable message based grains identifiable by <see cref="Guid"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithGuidId : ObservableMessageBasedGrain, IHaveGuidId
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Guid id)
        {
			explicitId = id;
		}
		        
		Guid explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Guid"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }

	    /// <summary>
        /// Notifies all attached observers registered for a particular type of event,
		/// passing given event to each of them.
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="e">The event of <typeparamref name="TEvent"/> type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(), e);
        }
		
    }

    /// <summary>
    /// Base class for observable message based grains identifiable by <see cref="Int64"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithInt64Id : ObservableMessageBasedGrain, IHaveInt64Id
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Int64 id)
        {
			explicitId = id;
		}
		        
		Int64 explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Int64"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Int64 Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }

	    /// <summary>
        /// Notifies all attached observers registered for a particular type of event,
		/// passing given event to each of them.
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="e">The event of <typeparamref name="TEvent"/> type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(), e);
        }
		
    }

    /// <summary>
    /// Base class for observable message based grains identifiable by <see cref="String"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithStringId : ObservableMessageBasedGrain, IHaveStringId
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(String id)
        {
			explicitId = id;
		}
		        
		String explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="String"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected String Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }

	    /// <summary>
        /// Notifies all attached observers registered for a particular type of event,
		/// passing given event to each of them.
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="e">The event of <typeparamref name="TEvent"/> type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(), e);
        }
		
    }

    /// <summary>
    /// Base class for persistent observable message based grains identifiable by <see cref="Guid"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithGuidId<TGrainState> : ObservableMessageBasedGrain<TGrainState>, IHaveGuidId
        where TGrainState : class, IGrainState
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Guid id)
        {
			explicitId = id;
		}
		        
		Guid explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Guid"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }

	    /// <summary>
        /// Notifies all attached observers registered for a particular type of event,
		/// passing given event to each of them.
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="e">The event of <typeparamref name="TEvent"/> type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(), e);
        }
	}

    /// <summary>
    /// Base class for persistent observable message based grains identifiable by <see cref="Int64"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithInt64Id<TGrainState> : ObservableMessageBasedGrain<TGrainState>, IHaveInt64Id
        where TGrainState : class, IGrainState
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Int64 id)
        {
			explicitId = id;
		}
		        
		Int64 explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Int64"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Int64 Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }

	    /// <summary>
        /// Notifies all attached observers registered for a particular type of event,
		/// passing given event to each of them.
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="e">The event of <typeparamref name="TEvent"/> type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(), e);
        }
	}

    /// <summary>
    /// Base class for persistent observable message based grains identifiable by <see cref="String"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithStringId<TGrainState> : ObservableMessageBasedGrain<TGrainState>, IHaveStringId
        where TGrainState : class, IGrainState
    {
		#if GRAIN_STUBBING_ENABLED
		
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(String id)
        {
			explicitId = id;
		}
		        
		String explicitId;       	

		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="String"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected String Id()
        {
			#if GRAIN_STUBBING_ENABLED
			return explicitId;
			#else
			return Identity.Of(this);
			#endif
        }

	    /// <summary>
        /// Notifies all attached observers registered for a particular type of event,
		/// passing given event to each of them.
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="e">The event of <typeparamref name="TEvent"/> type</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(), e);
        }
	}

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
}
