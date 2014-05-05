        /// <summary>
        /// An instance of <see cref="IMessageBus"/> pointing to global instance by default
        /// </summary>
        public IMessageBus Bus = MessageBus.Instance;
        
        /// <summary>
        /// A mockabe instance of this grain. You can substitute it within a test harness
        /// </summary>
        /// <remarks>
        /// WARNING! This will work only if DEBUG constant is defined for build.
        /// In RELEASE mode all magic will gone, and original GrainBase methods 
        /// will be bound by the comiler
        /// </remarks>
        public IGrainInstance Instance;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected MessageBasedGrain()
        {
            Instance = this;
        }

        #if DEBUG
        
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
        /// Until the Task returned from the asyncCallback is resolved,
        ///             the next timer tick will not be scheduled.
        ///             That is to say, timer callbacks never interleave their turns.
        /// 
        /// </para>
        /// 
        /// <para>
        /// The timer may be stopped at any time by calling the <c>Dispose</c> method
        ///             on the timer handle returned from this call.
        /// 
        /// </para>
        /// 
        /// <para>
        /// Any exceptions thrown by or faulted Task's returned from the asyncCallback
        ///             will be logged, but will not prevent the next timer tick from being queued.
        /// 
        /// </para>
        /// 
        /// </remarks>
        /// <param name="asyncCallback">Callback function to be invoked when timr ticks.</param>
        /// <param name="state">State object that will be passed as argument when calling the asyncCallback.</param>
        /// <param name="dueTime">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        /// <returns>
        /// Handle for this Timer.
        /// </returns>
        /// <seealso cref="T:Orleans.IOrleansTimer"/>
        protected new IOrleansTimer RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
           return Instance.RegisterTimer(asyncCallback, state, dueTime, period);
        }

        /// <summary>
        /// Registers a persistent, reliable reminder to send regular notifications (reminders) to the grain.
        ///             The grain must implement the <c>Orleans.IRemindable</c> interface, and reminders for this grain will be sent to the <c>ReceiveReminder</c> callback method.
        ///             If the current grain is deactivated when the timer fires, a new activation of this grain will be created to receive this reminder.
        ///             If an existing reminder with the same name already exists, that reminder will be overwritten with this new reminder.
        ///             Reminders will always be received by one activation of this grain, even if multiple activations exist for this grain.
        /// 
        /// </summary>
        /// <param name="reminderName">Name of this reminder</param>
        /// <param name="dueTime">Due time for this reminder</param>
        /// <param name="period">Frequence period for this reminder</param>
        /// <returns>
        /// Promise for Reminder handle.
        /// </returns>
        protected new Task<IOrleansReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return Instance.RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        /// <summary>
        /// Unregisters a previously registered reminder.
        /// 
        /// </summary>
        /// <param name="reminder">Reminder to unregister.</param>
        /// <returns>
        /// Completion promise for this operation.
        /// </returns>
        protected new Task UnregisterReminder(IOrleansReminder reminder)
        {
            return Instance.UnregisterReminder(reminder);
        }

        /// <summary>
        /// Returns a previously registered reminder.
        /// 
        /// </summary>
        /// <param name="reminderName">Reminder to return</param>
        /// <returns>
        /// Promise for Reminder handle.
        /// </returns>
        protected new Task<IOrleansReminder> GetReminder(string reminderName)
        {
            return Instance.GetReminder(reminderName);
        }

        /// <summary>
        /// Returns a list of all reminders registered by the grain.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// Promise for list of Reminders registered for this grain.
        /// </returns>
        protected new Task<List<IOrleansReminder>> GetReminders()
        {
            return Instance.GetReminders();
        }

        /// <summary>
        /// Deactivate this activation of the grain after the current grain method call is completed.
        ///             This call will mark this activation of the current grain to be deactivated and removed at the end of the current method.
        ///             The next call to this grain will result in a different activation to be used, which typical means a new activation will be created automatically by the runtime.
        /// 
        /// </summary>
        protected new void DeactivateOnIdle()
        {
            Instance.DeactivateOnIdle();
        }

        /// <summary>
        /// Delay Deactivation of this activation at least for the specified time duration.
        ///             A positive <c>timeSpan</c> value means “prevent GC of this activation for that time span”.
        ///             A negative <c>timeSpan</c> value means “unlock, and make this activation available for GC again”.
        ///             DeactivateOnIdle method would undo / override any current “keep alive” setting,
        ///             making this grain immediately available  for deactivation.
        /// 
        /// </summary>
        protected new void DelayDeactivation(TimeSpan timeSpan)
        {
            Instance.DelayDeactivation(timeSpan);
        }

        #endif

        #region IGrainInstance

        IOrleansTimer IGrainInstance.RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterTimer(asyncCallback, state, dueTime, period);
        }

        Task<IOrleansReminder> IGrainInstance.RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period)
        {
            return base.RegisterOrUpdateReminder(reminderName, dueTime, period);
        }

        Task IGrainInstance.UnregisterReminder(IOrleansReminder reminder)
        {
            return base.UnregisterReminder(reminder);
        }

        Task<IOrleansReminder> IGrainInstance.GetReminder(string reminderName)
        {
            return base.GetReminder(reminderName);
        }

        Task<List<IOrleansReminder>> IGrainInstance.GetReminders()
        {
            return base.GetReminders();
        }

        void IGrainInstance.DeactivateOnIdle()
        {
            base.DeactivateOnIdle();
        }

        void IGrainInstance.DelayDeactivation(TimeSpan timeSpan)
        {
            base.DelayDeactivation(timeSpan);
        }

        #endregion