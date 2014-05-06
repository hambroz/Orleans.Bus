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