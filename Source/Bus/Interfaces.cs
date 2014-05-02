using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ICommandSender
    {
        Task Send(long id, object command);
    }

    public interface IQueryHandler
    {
        Task<TResult> Query<TResult>(long id, object query);
    }
    
    public interface IEventPublisher
    {
        void Publish<TEvent>(IObservablePublisher publisher, TEvent @event);
    }

    public interface ISubscriptionManager
    {
        Task<SubscriptionToken> CreateToken<T>(IObserve<T> client);
        void DeleteToken(SubscriptionToken token);

        Task Subscribe<TEvent>(long id, IObserve<TEvent> client, SubscriptionToken token);
        Task Unsubscribe<TEvent>(long id, IObserve<TEvent> client, SubscriptionToken token);
    }

    public interface IServerMessageBus : 
        ICommandSender, 
        IQueryHandler, 
        IEventPublisher
    {}

    public interface IClientMessageBus : 
        ICommandSender, 
        IQueryHandler, 
        ISubscriptionManager
    {}
}
