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

        public Action<string, object> OnCommand = (destination, command) => { };
        public Func<string, object, Type, object> OnQuery;
            
        Task IMessageBus.Send(string destination, object command)
        {
            RecordedCommands.Add(new RecordedCommand(destination, command));
            return TaskDone.Done;
        }

        Task<TResult> IMessageBus.Query<TResult>(string destination, object query)
        {
            RecordedQueries.Add(new RecordedQuery(destination, query, typeof(TResult)));
            return Task.FromResult(default(TResult));
        }

        Task IMessageBus.Subscribe<TEvent>(string source, IObserver observer)
        {
            RecordedSubscriptions.Add(new RecordedSubscription(source, observer));
            return TaskDone.Done;
        }

        Task IMessageBus.Unsubscribe<TEvent>(string source, IObserver observer)
        {
            RecordedSubscriptions.RemoveAll(x => x.Source.Equals(source) && x.Observer == observer);
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
        public readonly string Destination;
        public readonly object Command;

        public RecordedCommand(string destination, object command)
        {
            Destination = destination;
            Command = command;
        }
    }

    public class RecordedQuery
    {
        public readonly string Destination;
        public readonly object Query;
        public readonly Type Result;

        public RecordedQuery(string destination, object query, Type result)
        {
            Destination = destination;
            Query = query;
            Result = result;
        }
    }

    public class RecordedSubscription
    {
        public readonly string Source;
        public readonly IObserver Observer;

        public RecordedSubscription(string source, IObserver observer)
        {
            Source = source;
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
