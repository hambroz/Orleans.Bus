using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus.Stubs
{
    public class MessageBusStub : IMessageBus
    {
        public readonly List<RecordedCommand> RecordedCommands = new List<RecordedCommand>();
        public readonly List<RecordedQuery> RecordedQueries = new List<RecordedQuery>();
        public readonly List<RecordedObserver> RecordedObservers = new List<RecordedObserver>();
        public readonly List<RecordedSubscription> RecordedSubscriptions = new List<RecordedSubscription>();

        public Action<object, object> OnCommand = (receiver, command) => {};
        public Func<object, object, Type, object> OnQuery;
            
        Task IMessageBus.Send<TCommand>(Guid id, TCommand command)
        {
            RecordedCommands.Add(new RecordedCommand(id, command));
            OnCommand(id, command);
            return TaskDone.Done;
        }

        Task IMessageBus.Send<TCommand>(long id, TCommand command)
        {
            RecordedCommands.Add(new RecordedCommand(id, command));
            return TaskDone.Done;
        }

        Task IMessageBus.Send<TCommand>(string id, TCommand command)
        {
            RecordedCommands.Add(new RecordedCommand(id, command));
            return TaskDone.Done;
        }

        Task<TResult> IMessageBus.Query<TQuery, TResult>(Guid id, TQuery query)
        {
            RecordedQueries.Add(new RecordedQuery(id, query, typeof(TResult)));
            return Task.FromResult(default(TResult));
        }

        Task<TResult> IMessageBus.Query<TQuery, TResult>(long id, TQuery query)
        {
            RecordedQueries.Add(new RecordedQuery(id, query, typeof(TResult)));
            return Task.FromResult(default(TResult));
        }

        Task<TResult> IMessageBus.Query<TQuery, TResult>(string id, TQuery query)
        {
            RecordedQueries.Add(new RecordedQuery(id, query, typeof(TResult)));
            return Task.FromResult(default(TResult));
        }

        Task IMessageBus.Subscribe<TEvent>(Guid id, IObserver observer)
        {
            RecordedSubscriptions.Add(new RecordedSubscription(id, observer));
            return TaskDone.Done;
        }

        Task IMessageBus.Subscribe<TEvent>(long id, IObserver observer)
        {
            RecordedSubscriptions.Add(new RecordedSubscription(id, observer));
            return TaskDone.Done;
        }

        Task IMessageBus.Subscribe<TEvent>(string id, IObserver observer)
        {
            RecordedSubscriptions.Add(new RecordedSubscription(id, observer));
            return TaskDone.Done;
        }

        Task IMessageBus.Unsubscribe<TEvent>(Guid id, IObserver observer)
        {
            RecordedSubscriptions.RemoveAll(x => x.Receiver.Equals(id) && x.Observer == observer);
            return TaskDone.Done;
        }

        Task IMessageBus.Unsubscribe<TEvent>(long id, IObserver observer)
        {
            RecordedSubscriptions.RemoveAll(x => x.Receiver.Equals(id) && x.Observer == observer);
            return TaskDone.Done;
        }

        Task IMessageBus.Unsubscribe<TEvent>(string id, IObserver observer)
        {
            RecordedSubscriptions.RemoveAll(x => x.Receiver.Equals(id) && x.Observer == observer);
            return TaskDone.Done;
        }

        Task<IObserver> IMessageBus.CreateObserver(Observes client)
        {
            var observer = new ObserverStub();
            RecordedObservers.Add(new RecordedObserver(client, observer));
            return Task.FromResult((IObserver)observer);
        }

        void IMessageBus.DeleteObserver(IObserver observer)
        {
            RecordedObservers.RemoveAll(x => x.Observer == observer);
        }
    }

    public class RecordedCommand
    {
        public readonly object Receiver;
        public readonly object Command;

        public RecordedCommand(object receiver, object command)
        {
            Receiver = receiver;
            Command = command;
        }
    }

    public class RecordedQuery
    {
        public readonly object Receiver;
        public readonly object Query;
        public readonly Type Result;

        public RecordedQuery(object receiver, object query, Type result)
        {
            Receiver = receiver;
            Query = query;
            Result = result;
        }
    }

    public class RecordedSubscription
    {
        public readonly object Receiver;
        public readonly IObserver Observer;

        public RecordedSubscription(object receiver, IObserver observer)
        {
            Receiver = receiver;
            Observer = observer;
        }
    }

    public class RecordedObserver
    {
        public readonly Observes Client;
        public readonly ObserverStub Observer;

        public RecordedObserver(Observes client, ObserverStub observer)
        {
            Client = client;
            Observer = observer;
        }
    }
}
