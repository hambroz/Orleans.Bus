using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#if GRAIN_STUBBING_ENABLED
using Orleans.Bus.Stubs;
#endif 

namespace Orleans.Bus
{
    /// <summary>
    /// Base class for all kinds of message based grains
    /// </summary>
    public abstract class MessageBasedGrain : GrainBase, IGrain
        #if GRAIN_STUBBING_ENABLED
            , IStubbedMessageGrain
        #endif
    {
        IMessageBus bus = 
        #if GRAIN_STUBBING_ENABLED
            new MessageBusStub();
        #else
            MessageBus.Instance;
        #endif

        #if GRAIN_STUBBING_ENABLED
        
        MessageBusStub IStubbedMessageGrain.Bus
        {
            get {return (MessageBusStub)bus; }
        }
        
        #endif

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
            return bus.Send(id, command);
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
            return bus.Send(id, command);
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
            return bus.Send(id, command);
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
            return bus.Query<TQuery, TResult>(id, query);
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
            return bus.Query<TQuery, TResult>(id, query);
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
            return bus.Query<TQuery, TResult>(id, query);
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

        List<GrainAuditEvent> dispatched = new List<GrainAuditEvent>();

        List<GrainAuditEvent> IStubbedMessageGrain.Dispatched
        {
            get {return dispatched; }
        }

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
            dispatched.Add(new RegisteredTimer<TTimerState>(name, callback, state, due, period));
            
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
            dispatched.Add(new UnregisteredTimer(name));
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

            dispatched.Add(new RegisteredReminder(name, due, period));
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
            dispatched.Add(new UnregisteredReminder(name));
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
            dispatched.Add(new RequestedDeactivationOnIdle());
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
            dispatched.Add(new RequestedDeactivationDelay(period));
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
        #if GRAIN_STUBBING_ENABLED
            , IStubbedMessageGrain
            , IStubState<TState>
        #endif	 
        where TState : class, IGrainState
    {
        IMessageBus bus = 
        #if GRAIN_STUBBING_ENABLED
            new MessageBusStub();
        #else
            MessageBus.Instance;
        #endif

        #if GRAIN_STUBBING_ENABLED
        
        MessageBusStub IStubbedMessageGrain.Bus
        {
            get {return (MessageBusStub)bus; }
        }
        
        #endif

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
            return bus.Send(id, command);
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
            return bus.Send(id, command);
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
            return bus.Send(id, command);
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
            return bus.Query<TQuery, TResult>(id, query);
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
            return bus.Query<TQuery, TResult>(id, query);
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
            return bus.Query<TQuery, TResult>(id, query);
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

        List<GrainAuditEvent> dispatched = new List<GrainAuditEvent>();

        List<GrainAuditEvent> IStubbedMessageGrain.Dispatched
        {
            get {return dispatched; }
        }

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
            dispatched.Add(new RegisteredTimer<TTimerState>(name, callback, state, due, period));
            
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
            dispatched.Add(new UnregisteredTimer(name));
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

            dispatched.Add(new RegisteredReminder(name, due, period));
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
            dispatched.Add(new UnregisteredReminder(name));
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
            dispatched.Add(new RequestedDeactivationOnIdle());
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
            dispatched.Add(new RequestedDeactivationDelay(period));
            #else
            base.DelayDeactivation(period);
            #endif
        }

        #endregion
		        
		#if GRAIN_STUBBING_ENABLED

        void IStubState<TState>.SetState(TState state)
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
        #if GRAIN_STUBBING_ENABLED
            , IStubGuidId
        #endif	
    {
		#if GRAIN_STUBBING_ENABLED
		
        void IStubGuidId.SetId(Guid id)
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
        #if GRAIN_STUBBING_ENABLED
            , IStubInt64Id
        #endif	
    {
		#if GRAIN_STUBBING_ENABLED
		
        void IStubInt64Id.SetId(Int64 id)
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
        #if GRAIN_STUBBING_ENABLED
            , IStubStringId
        #endif	
    {
		#if GRAIN_STUBBING_ENABLED
		
        void IStubStringId.SetId(String id)
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
        #if GRAIN_STUBBING_ENABLED
            , IStubGuidId
        #endif
	        where TState : class, IGrainState
    {
		#if GRAIN_STUBBING_ENABLED
		
        void IStubGuidId.SetId(Guid id)
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
        #if GRAIN_STUBBING_ENABLED
            , IStubInt64Id
        #endif
	        where TState : class, IGrainState
    {
		#if GRAIN_STUBBING_ENABLED
		
        void IStubInt64Id.SetId(Int64 id)
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
        #if GRAIN_STUBBING_ENABLED
            , IStubStringId
        #endif
	        where TState : class, IGrainState
    {
		#if GRAIN_STUBBING_ENABLED
		
        void IStubStringId.SetId(String id)
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
    /// Base class for observable message based grains identifiable by <see cref="Guid"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithGuidId : MessageBasedGrain, IObservableGrain, IHaveGuidId
        #if GRAIN_STUBBING_ENABLED
            , IStubbedObservableMessageGrain
            , IStubGuidId
        #endif
    {
        readonly IObserverCollection observers = 
        #if GRAIN_STUBBING_ENABLED
            new ObserverCollectionStub();
        #else
            new ObserverCollection();
        #endif

        #if GRAIN_STUBBING_ENABLED
        
        ObserverCollectionStub IStubbedObservableMessageGrain.Observers
        {
            get {return (ObserverCollectionStub)observers; }
        }
        
        #endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Attach(Observes o, Type e)
        {
            observers.Attach(o, e);
            return TaskDone.Done;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Detach(Observes o, Type e)
        {
            observers.Detach(o, e);
            return TaskDone.Done;
        }
		#if GRAIN_STUBBING_ENABLED
		
        void IStubGuidId.SetId(Guid id)
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
            observers.Notify(Id(), e);
        }
		
    }

    /// <summary>
    /// Base class for observable message based grains identifiable by <see cref="Int64"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithInt64Id : MessageBasedGrain, IObservableGrain, IHaveInt64Id
        #if GRAIN_STUBBING_ENABLED
            , IStubbedObservableMessageGrain
            , IStubInt64Id
        #endif
    {
        readonly IObserverCollection observers = 
        #if GRAIN_STUBBING_ENABLED
            new ObserverCollectionStub();
        #else
            new ObserverCollection();
        #endif

        #if GRAIN_STUBBING_ENABLED
        
        ObserverCollectionStub IStubbedObservableMessageGrain.Observers
        {
            get {return (ObserverCollectionStub)observers; }
        }
        
        #endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Attach(Observes o, Type e)
        {
            observers.Attach(o, e);
            return TaskDone.Done;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Detach(Observes o, Type e)
        {
            observers.Detach(o, e);
            return TaskDone.Done;
        }
		#if GRAIN_STUBBING_ENABLED
		
        void IStubInt64Id.SetId(Int64 id)
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
            observers.Notify(Id(), e);
        }
		
    }

    /// <summary>
    /// Base class for observable message based grains identifiable by <see cref="String"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithStringId : MessageBasedGrain, IObservableGrain, IHaveStringId
        #if GRAIN_STUBBING_ENABLED
            , IStubbedObservableMessageGrain
            , IStubStringId
        #endif
    {
        readonly IObserverCollection observers = 
        #if GRAIN_STUBBING_ENABLED
            new ObserverCollectionStub();
        #else
            new ObserverCollection();
        #endif

        #if GRAIN_STUBBING_ENABLED
        
        ObserverCollectionStub IStubbedObservableMessageGrain.Observers
        {
            get {return (ObserverCollectionStub)observers; }
        }
        
        #endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Attach(Observes o, Type e)
        {
            observers.Attach(o, e);
            return TaskDone.Done;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Detach(Observes o, Type e)
        {
            observers.Detach(o, e);
            return TaskDone.Done;
        }
		#if GRAIN_STUBBING_ENABLED
		
        void IStubStringId.SetId(String id)
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
            observers.Notify(Id(), e);
        }
		
    }

    /// <summary>
    /// Base class for persistent observable message based grains identifiable by <see cref="Guid"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithGuidId<TGrainState> : MessageBasedGrain<TGrainState>, IObservableGrain, IHaveGuidId
        #if GRAIN_STUBBING_ENABLED
            , IStubbedObservableMessageGrain
            , IStubGuidId
        #endif
        where TGrainState : class, IGrainState
    {
        readonly IObserverCollection observers = 
        #if GRAIN_STUBBING_ENABLED
            new ObserverCollectionStub();
        #else
            new ObserverCollection();
        #endif

        #if GRAIN_STUBBING_ENABLED
        
        ObserverCollectionStub IStubbedObservableMessageGrain.Observers
        {
            get {return (ObserverCollectionStub)observers; }
        }
        
        #endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Attach(Observes o, Type e)
        {
            observers.Attach(o, e);
            return TaskDone.Done;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Detach(Observes o, Type e)
        {
            observers.Detach(o, e);
            return TaskDone.Done;
        }
		#if GRAIN_STUBBING_ENABLED
		
        void IStubGuidId.SetId(Guid id)
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
            observers.Notify(Id(), e);
        }
	}

    /// <summary>
    /// Base class for persistent observable message based grains identifiable by <see cref="Int64"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithInt64Id<TGrainState> : MessageBasedGrain<TGrainState>, IObservableGrain, IHaveInt64Id
        #if GRAIN_STUBBING_ENABLED
            , IStubbedObservableMessageGrain
            , IStubInt64Id
        #endif
        where TGrainState : class, IGrainState
    {
        readonly IObserverCollection observers = 
        #if GRAIN_STUBBING_ENABLED
            new ObserverCollectionStub();
        #else
            new ObserverCollection();
        #endif

        #if GRAIN_STUBBING_ENABLED
        
        ObserverCollectionStub IStubbedObservableMessageGrain.Observers
        {
            get {return (ObserverCollectionStub)observers; }
        }
        
        #endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Attach(Observes o, Type e)
        {
            observers.Attach(o, e);
            return TaskDone.Done;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Detach(Observes o, Type e)
        {
            observers.Detach(o, e);
            return TaskDone.Done;
        }
		#if GRAIN_STUBBING_ENABLED
		
        void IStubInt64Id.SetId(Int64 id)
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
            observers.Notify(Id(), e);
        }
	}

    /// <summary>
    /// Base class for persistent observable message based grains identifiable by <see cref="String"/> identifier
    /// </summary>
    public abstract class ObservableMessageBasedGrainWithStringId<TGrainState> : MessageBasedGrain<TGrainState>, IObservableGrain, IHaveStringId
        #if GRAIN_STUBBING_ENABLED
            , IStubbedObservableMessageGrain
            , IStubStringId
        #endif
        where TGrainState : class, IGrainState
    {
        readonly IObserverCollection observers = 
        #if GRAIN_STUBBING_ENABLED
            new ObserverCollectionStub();
        #else
            new ObserverCollection();
        #endif

        #if GRAIN_STUBBING_ENABLED
        
        ObserverCollectionStub IStubbedObservableMessageGrain.Observers
        {
            get {return (ObserverCollectionStub)observers; }
        }
        
        #endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Attach(Observes o, Type e)
        {
            observers.Attach(o, e);
            return TaskDone.Done;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Task IObservableGrain.Detach(Observes o, Type e)
        {
            observers.Detach(o, e);
            return TaskDone.Done;
        }
		#if GRAIN_STUBBING_ENABLED
		
        void IStubStringId.SetId(String id)
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
            observers.Notify(Id(), e);
        }
	}

}
