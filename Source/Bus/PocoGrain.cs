using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Base interface for POCO grains
    /// </summary>
    public interface IPocoGrain : IMessageBasedGrain
    {
        /// <summary>
        /// Generic command handler
        /// </summary>
        [Handler] Task HandleCommand(object cmd);

        /// <summary>
        /// Generic query handler
        /// </summary>
        [Handler] Task<object> AnswerQuery(object query);
    }    
    
    /// <summary>
    /// Base class for POCO grains
    /// </summary>
    /// <typeparam name="TPoco">Type of POCO object</typeparam>
    public abstract class PocoGrain<TPoco> : MessageBasedGrain, IPocoGrain
    {
        /// <summary>
        /// Set this activator delegate in subclass to return and activate new instance of <typeparamref name="TPoco"/>
        /// </summary>
        protected Func<MessageBasedGrain, Task<TPoco>> Activate = 
            x => { throw new InvalidOperationException("Please set activator in subclass constructor"); };

        /// <summary>
        /// Set this handler delegate in subclass to dispatch incoming command to an instance of given <typeparamref name="TPoco"/>
        /// </summary>
        protected Func<TPoco, object, Task> Handle =
            (p, c) => { throw new InvalidOperationException("Please set dispatcher in subclass constructor"); };

        /// <summary>
        /// Set this handler delegate in subclass to dispatch incoming query to an instance of given <typeparamref name="TPoco"/>
        /// </summary>
        protected Func<TPoco, object, Task<object>> Answer =
            (p, q) => { throw new InvalidOperationException("Please set dispatcher in subclass constructor"); };

        TPoco poco;

        /// <summary>
        /// This method is called at the end of the process of activating a grain.
        ///             It is called before any messages have been dispatched to the grain.
        ///             For grains with declared persistent state, this method is called after the State property has been populated.
        /// </summary>
        public override async Task ActivateAsync()
        {            
            poco = await Activate(this);
        }

        Task IPocoGrain.HandleCommand(object cmd)
        {
            return Handle(poco, cmd);
        }

        Task<object> IPocoGrain.AnswerQuery(object query)
        {
            return Answer(poco, query);
        }
    }

    /// <summary>
    /// Base class for persistent POCO grains
    /// </summary>
    /// <typeparam name="TPoco">Type of POCO object</typeparam>
    /// <typeparam name="TState">Type of persistent state</typeparam>
    public abstract class PocoGrain<TPoco, TState> : MessageBasedGrain<TState>, IPocoGrain         
    {
        /// <summary>
        /// Set this activator delegate in subclass to return and activate new instance of <typeparamref name="TPoco"/>
        /// </summary>
        protected Func<MessageBasedGrain<TState>, Task<TPoco>> Activate =
            x => { throw new InvalidOperationException("Please set activator in subclass constructor"); };

        /// <summary>
        /// Set this handler delegate in subclass to dispatch incoming command to an instance of given <typeparamref name="TPoco"/>
        /// </summary>
        protected Func<TPoco, object, Task> Handle =
            (p, c) => { throw new InvalidOperationException("Please set dispatcher in subclass constructor"); };

        /// <summary>
        /// Set this handler delegate in subclass to dispatch incoming query to an instance of given <typeparamref name="TPoco"/>
        /// </summary>
        protected Func<TPoco, object, Task<object>> Answer =
            (p, q) => { throw new InvalidOperationException("Please set dispatcher in subclass constructor"); };

        TPoco poco;

        /// <summary>
        /// This method is called at the end of the process of activating a grain.
        ///             It is called before any messages have been dispatched to the grain.
        ///             For grains with declared persistent state, this method is called after the State property has been populated.
        /// </summary>
        public override async Task ActivateAsync()
        {
            await base.ActivateAsync();
            poco = await Activate(this);            
        }

        Task IPocoGrain.HandleCommand(object cmd)
        {
            return Handle(poco, cmd);
        }

        Task<object> IPocoGrain.AnswerQuery(object query)
        {
            return Answer(poco, query);
        }
    }
}
