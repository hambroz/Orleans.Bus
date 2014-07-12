using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.Providers;
using Orleans.Storage;

namespace Orleans.Bus
{
    /// <summary>
    /// Generic state holder for internal purposes
    /// </summary>
    public interface IStateHolder : IGrainState
    {
        /// <summary>
        /// Holds actual grain's state
        /// </summary>
        object State
        {
            get; set;
        }
    }

    /// <summary>
    /// Unit of work which controls state checkpointing
    /// </summary>
    public interface IStateStorage
    {
        /// <summary>
        /// Async method to cause refresh of the current grain state data from backin store.
        ///  Any previous contents of the grain state data will be overwritten.
        /// </summary>
        Task ReadStateAsync();

        /// <summary>
        /// Async method to cause write of the current grain state data into backin store.
        /// </summary>
        Task WriteStateAsync();

        /// <summary>
        /// Async method to cause the current grain state data to be cleared and reset.
        /// This will usually mean the state record is deleted from backin store, but the specific behavior is defined by the storage provider instance configured for this grain.
        /// </summary>
        Task ClearStateAsync();
    }

    internal class DefaultStateStorage : IStateStorage
    {
        readonly IGrainState state;

        public DefaultStateStorage(IGrainState state)
        {
            this.state = state;
        }

        public Task ReadStateAsync()
        {
            return state.ReadStateAsync();
        }

        public Task WriteStateAsync()
        {
            return state.WriteStateAsync();
        }

        public Task ClearStateAsync()
        {
            return state.ClearStateAsync();
        }
    }

    /// <summary>
    /// Strongly-typed storage provider
    /// </summary>
    /// <typeparam name="TState">Type of the grain state</typeparam>
    public abstract class StateStorageProvider<TState> : IStorageProvider
    {
        string IOrleansProvider.Name
        {
            get { return GetType().Name; }
        }

        Task IOrleansProvider.Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Log = providerRuntime.GetLogger(name, Logger.LoggerType.Application);
            return Init(config.Properties);
        }

        async Task IStorageProvider.ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            ((IStateHolder) grainState).State = await ReadStateAsync(grainReference.Id(), grainType);
        }

        Task IStorageProvider.WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            return WriteStateAsync(grainReference.Id(), grainType, (TState) ((IStateHolder) grainState).State);
        }

        Task IStorageProvider.ClearStateAsync(string grainType, GrainReference grainReference, GrainState grainState)
        {
            return ClearStateAsync(grainReference.Id(), grainType, (TState) ((IStateHolder)grainState).State);
        }

        /// <summary>
        /// Logger used by this storage provider instance.
        /// </summary>
        /// <returns>
        /// Reference to the Logger object.
        /// </returns>
        /// <seealso cref="T:Orleans.OrleansLogger"/>
        public OrleansLogger Log
        {
            get; set;
        }

        /// <summary>
        /// Closes this storage provider instance. Use for final teardown
        /// </summary>
        /// <returns>
        /// Completion promise for the Close operation on this provider.
        /// </returns>
        public virtual Task Close()
        {
            return TaskDone.Done;
        }

        /// <summary>
        /// Called by Orleans infrastructure when a new provider class instance  is created (initialization)
        /// </summary>
        /// <param name="properties">Configuration metadata to be used for this provider instance</param>
        /// <returns>
        /// Completion promise Task for the inttialization work for this provider
        /// </returns>
        public abstract Task Init(Dictionary<string, string> properties);

        /// <summary>
        /// Returns grain's state stored in backing store.
        /// </summary>
        /// <param name="grainId">Id of the grain.</param>
        /// <param name="grainType">Type of the grain [fully qualified class name]</param>
        /// <returns>
        /// Completion promise which return state for the specified grain.
        /// </returns>
        public abstract Task<TState> ReadStateAsync(string grainId, string grainType);

        /// <summary>
        /// Writes grain's state to backing store
        /// </summary>
        /// <param name="grainId">Id of the grain.</param>
        /// <param name="grainType">Type of the grain [fully qualified class name]</param>
        /// <param name="grainState">Grain state to be written.</param>
        /// <returns>
        /// Completion promise for the Write operation on the specified grain.
        /// </returns>
        public abstract Task WriteStateAsync(string grainId, string grainType, TState grainState);

        /// <summary>
        /// Deletes / Clears grain's state in a backing store.
        /// </summary>
        /// <param name="grainId">Id of the grain.</param>
        /// <param name="grainType">Type of the grain [fully qualified class name]</param>
        /// <param name="grainState">Latest grain's state at the moment when Clear was called.</param>
        /// <returns>
        /// Completion promise for the Clear operation on the specified grain.
        /// </returns>
        public abstract Task ClearStateAsync(string grainId, string grainType, TState grainState);
    }
}
