using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Base class for all kinds of message based grains
    /// </summary>
    public abstract class MessageBasedGrain : GrainBase, IGrain, IGrainInstance
    {
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

        #region Shortcuts

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
	}

    /// <summary>
    /// Base class for all kinds of persistent message based grains
    /// </summary>
    public abstract class MessageBasedGrain<TState> : GrainBase<TState>, IGrain, IGrainInstance 
        where TState : class, IGrainState
    {
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

        #region Shortcuts

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

		/// <summary>
        /// Sets grain's state for testing purposes
        /// </summary>
        public void SetState(TState state)
        {
			#if DEBUG
			explicitState = state;
			#endif
		}
		        
		#if DEBUG
		
		TState explicitState;
       
		/// <summary>
        /// Gets grain's state
        /// </summary>
        protected new TState State
        {
            get { return explicitState ?? base.State; }
        }

		#endif
	}

    /// <summary>
    /// Base class for message based grains identifiable by <see cref="Guid"/> identifier
    /// </summary>
    public abstract class MessageBasedGrainWithGuidId : MessageBasedGrain, IHaveGuidId
    {
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Guid id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		Guid? explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Guid"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId.Value : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Int64 id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		Int64? explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Int64"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Int64 Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId.Value : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(String id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		String explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="String"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected String Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Guid id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		Guid? explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Guid"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId.Value : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Int64 id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		Int64? explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Int64"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Int64 Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId.Value : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(String id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		String explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="String"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected String Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Guid id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		Guid? explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Guid"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId.Value : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Int64 id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		Int64? explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Int64"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Int64 Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId.Value : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(String id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		String explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="String"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected String Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Guid id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		Guid? explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Guid"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Guid Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId.Value : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(Int64 id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		Int64? explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="Int64"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Int64 Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId.Value : Identity.Of(this);
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
		/// <summary>
        /// Sets grain's id for testing purposes
        /// </summary>
        public void SetId(String id)
        {
			#if DEBUG
			explicitId = id;
			#endif
		}
		        
		#if DEBUG
		String explicitId;       	
		#endif

        /// <summary>
        /// Gets identifier of the current grain.
        /// </summary>
        /// <returns><see cref="String"/> identifier</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected String Id()
        {
			#if DEBUG
			return explicitId != null ? explicitId : Identity.Of(this);
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
    /// This interface exists solely for unit testing purposes
    /// </summary>
    public interface IGrainInstance
    {
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
        IOrleansTimer RegisterTimer(Func<object, Task> asyncCallback, object state, TimeSpan dueTime, TimeSpan period);
        
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
        Task<IOrleansReminder> RegisterOrUpdateReminder(string reminderName, TimeSpan dueTime, TimeSpan period);

        /// <summary>
        /// Unregisters a previously registered reminder.
        /// 
        /// </summary>
        /// <param name="reminder">Reminder to unregister.</param>
        /// <returns>
        /// Completion promise for this operation.
        /// </returns>
        Task UnregisterReminder(IOrleansReminder reminder);

        /// <summary>
        /// Returns a previously registered reminder.
        /// 
        /// </summary>
        /// <param name="reminderName">Reminder to return</param>
        /// <returns>
        /// Promise for Reminder handle.
        /// </returns>
        Task<IOrleansReminder> GetReminder(string reminderName);
		
        /// <summary>
        /// Returns a list of all reminders registered by the grain.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// Promise for list of Reminders registered for this grain.
        /// </returns>
        Task<List<IOrleansReminder>> GetReminders();

        /// <summary>
        /// Deactivate this activation of the grain after the current grain method call is completed.
        ///             This call will mark this activation of the current grain to be deactivated and removed at the end of the current method.
        ///             The next call to this grain will result in a different activation to be used, which typical means a new activation will be created automatically by the runtime.
        /// 
        /// </summary>
        void DeactivateOnIdle();

        /// <summary>
        /// Delay Deactivation of this activation at least for the specified time duration.
        ///             A positive <c>timeSpan</c> value means “prevent GC of this activation for that time span”.
        ///             A negative <c>timeSpan</c> value means “unlock, and make this activation available for GC again”.
        ///             DeactivateOnIdle method would undo / override any current “keep alive” setting,
        ///             making this grain immediately available  for deactivation.
        /// 
        /// </summary>
        void DelayDeactivation(TimeSpan timeSpan);
    }
}
