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