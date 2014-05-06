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

        List<Invocation> invocations = new List<Invocation>();

        List<Invocation> IStubbedMessageGrain.Invocations
        {
            get {return invocations; }
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
        /// <param name="id">Unique id of the timer</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        /// <param name="state">State object that will be passed as argument when calling the  <paramref name="callback"/>.</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        protected void RegisterTimer<TTimerState>(string id, Func<TTimerState, Task> callback, TTimerState state, TimeSpan due, TimeSpan period)
        {
            IOrleansTimer timer;

            #if GRAIN_STUBBING_ENABLED

            timer = new TimerStub();
            invocations.Add(new RegisteredTimer<TTimerState>(id, callback, state, due, period));
            
            #else
            timer = base.RegisterTimer(s => callback((TTimerState) s), state, due, period);
            #endif

            timers.Add(id, timer);
        }

        /// <summary>
        /// Unregister previously registered timer. 
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        protected void UnregisterTimer(string id)
        {
            var timer = timers[id];
            timer.Dispose();

            #if GRAIN_STUBBING_ENABLED
            invocations.Add(new UnregisteredTimer(id));
            #endif
        }

        /// <summary>
        /// Checks whether timer with the given name was registered before
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        /// <returns><c>true</c> if timer was the give name was previously registered, <c>false</c> otherwise </returns>
        protected bool IsTimerRegistered(string id)
        {
            return timers.ContainsKey(id);
        }

        /// <summary>
        /// Returns ids of all currently registered timers
        /// </summary>
        /// <returns>A sequence of <see cref="string"/> elements</returns>
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
        ///             If an existing reminder with the same id already exists, that reminder will be overwritten with this new reminder.
        ///             Reminders will always be received by one activation of this grain, even if multiple activations exist for this grain.
        /// 
        /// </summary>
        /// <param name="id">Unique id of the reminder</param>
        /// <param name="due">Due time for this reminder</param>
        /// <param name="period">Frequence period for this reminder</param>
        /// <returns>
        /// Promise for Reminder registration.
        /// </returns>
        protected 
             #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task RegisterReminder(string id, TimeSpan due, TimeSpan period)
        {
            #if GRAIN_STUBBING_ENABLED
            
            var reminder = new ReminderStub();
            reminders[id] = reminder;

            invocations.Add(new RegisteredReminder(id, due, period));
            return TaskDone.Done;
            
            #else
            var reminder = await base.RegisterOrUpdateReminder(id, due, period);
            reminders[id] = reminder;
            #endif
        }

        /// <summary>
        /// Unregister previously registered peristent reminder if any
        /// </summary>
        /// <param name="id">Unique id of the reminder</param>
        protected
            #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task UnregisterReminder(string id)
        {
            #if GRAIN_STUBBING_ENABLED
            
            reminders.Remove(id);
            invocations.Add(new UnregisteredReminder(id));
            return TaskDone.Done;
            
            #else
            
            reminders.Remove(id);
            var reminder = await base.GetReminder(id);
            if (reminder != null)
                await base.UnregisterReminder(reminder);

            #endif
        }

        /// <summary>
        /// Checks whether reminder with the given id is currently registered
        /// </summary>
        /// <param name="id">Unique id of the reminder</param>
        /// <returns><c>true</c> if reminder with the give name is currently registered, <c>false</c> otherwise </returns>
        protected
            #if !GRAIN_STUBBING_ENABLED
                async
            #endif
        Task<bool> IsReminderRegistered(string id)
        {
            #if GRAIN_STUBBING_ENABLED
            return Task.FromResult(reminders.ContainsKey(id));
            #else
            return reminders.ContainsKey(id) || (await base.GetReminder(id)) != null;
            #endif
        }

        /// <summary>
        /// Returns ids of all currently registered reminders
        /// </summary>
        /// <returns>A sequence of <see cref="string"/> elements</returns>
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
            invocations.Add(new RequestedDeactivationOnIdle());
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
            invocations.Add(new RequestedDeactivationDelay(period));
            #else
            base.DelayDeactivation(period);
            #endif
        }

        #endregion