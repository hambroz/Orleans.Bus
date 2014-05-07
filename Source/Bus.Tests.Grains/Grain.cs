using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public abstract class Grain : MessageBasedGrain
    {
        protected Task Send<TCommand>(string destination, TCommand command) where TCommand : Command
        {
            return Bus.Send(destination, command);
        }

        protected Task<TResult> Query<TResult>(string destination, Query<TResult> query)
        {
            return Bus.Query<TResult>(destination, query);
        }
    }

    public abstract class Grain<TState> : MessageBasedGrain<TState> where TState : class, IGrainState
    {
        protected Task Send<TCommand>(string destination, TCommand command)
            where TCommand : Command
        {
            return Bus.Send(destination, command);
        }

        protected Task<TResult> Query<TResult>(string destination, Query<TResult> query)
        {
            return Bus.Query<TResult>(destination, query);
        }
    }

    public abstract class ObservableGrain : ObservableMessageBasedGrain
    {
        protected Task Send<TCommand>(string destination, TCommand command) where TCommand : Command
        {
            return Bus.Send(destination, command);
        }

        protected Task<TResult> Query<TResult>(string destination, Query<TResult> query)
        {
            return Bus.Query<TResult>(destination, query);
        }

        protected new void Notify<TEvent>(TEvent @event) where TEvent : Event
        {
            base.Notify(@event);
        }
    }

    public abstract class ObservableGrain<TState> : ObservableMessageBasedGrain<TState> where TState : class, IGrainState
    {
        protected Task Send<TCommand>(string destination, TCommand command)
            where TCommand : Command
        {
            return Bus.Send(destination, command);
        }

        protected Task<TResult> Query<TResult>(string destination, Query<TResult> query)
        {
            return Bus.Query<TResult>(destination, query);
        }

        protected new void Notify<TEvent>(TEvent @event) where TEvent : Event
        {
            base.Notify(@event);
        }
    }
}
