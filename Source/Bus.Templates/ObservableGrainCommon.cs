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
        public Task Detach(Observes o, Type e)
        {
            Observers.Detach(o, e);
            return TaskDone.Done;
        }