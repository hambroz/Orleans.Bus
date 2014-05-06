using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus.Stubs
{
    public class MessageBusStub : IMessageBus
    {
        Task IMessageBus.Send<TCommand>(Guid id, TCommand command)
        {
            throw new NotImplementedException();
        }

        Task IMessageBus.Send<TCommand>(long id, TCommand command)
        {
            throw new NotImplementedException();
        }

        Task IMessageBus.Send<TCommand>(string id, TCommand command)
        {
            throw new NotImplementedException();
        }

        Task<TResult> IMessageBus.Query<TQuery, TResult>(Guid id, TQuery query)
        {
            throw new NotImplementedException();
        }

        Task<TResult> IMessageBus.Query<TQuery, TResult>(long id, TQuery query)
        {
            throw new NotImplementedException();
        }

        Task<TResult> IMessageBus.Query<TQuery, TResult>(string id, TQuery query)
        {
            throw new NotImplementedException();
        }

        Task IMessageBus.Subscribe<TEvent>(Guid id, IObserver observer)
        {
            throw new NotImplementedException();
        }

        Task IMessageBus.Subscribe<TEvent>(long id, IObserver observer)
        {
            throw new NotImplementedException();
        }

        Task IMessageBus.Subscribe<TEvent>(string id, IObserver observer)
        {
            throw new NotImplementedException();
        }

        Task IMessageBus.Unsubscribe<TEvent>(Guid id, IObserver observer)
        {
            throw new NotImplementedException();
        }

        Task IMessageBus.Unsubscribe<TEvent>(long id, IObserver observer)
        {
            throw new NotImplementedException();
        }

        Task IMessageBus.Unsubscribe<TEvent>(string id, IObserver observer)
        {
            throw new NotImplementedException();
        }

        Task<IObserver> IMessageBus.CreateObserver(Observes client)
        {
            throw new NotImplementedException();
        }

        void IMessageBus.DeleteObserver(IObserver observer)
        {
            throw new NotImplementedException();
        }
    }
}
