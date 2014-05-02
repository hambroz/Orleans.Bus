using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    public abstract class ObservableGrain : Grain, IObservableGrain, IObservablePublisher
    {
        public IServerMessageBus Bus = MessageBus.Server;

        readonly IDictionary<Type, IGrainObserverSubscriptionManager<IObserve>> subscriptions = 
                    new Dictionary<Type, IGrainObserverSubscriptionManager<IObserve>>();

        protected Action<IObserve, object> Notify;

        public Task Subscribe(Type e, IObserve o)
        {
            IGrainObserverSubscriptionManager<IObserve> manager;
            if (!subscriptions.TryGetValue(e, out manager))
            {
                manager = Runtime.Create<IObserve>();
                subscriptions.Add(e, manager);
            }

            manager.Subscribe(o);
            return TaskDone.Done;
        }

        public Task Unsubscribe(Type e, IObserve o)
        {
            IGrainObserverSubscriptionManager<IObserve> manager;
            if (subscriptions.TryGetValue(e, out manager))
                manager.Unsubscribe(o);
            
            return TaskDone.Done;
        }

        void IObservablePublisher.Publish<TEvent>(TEvent e)
        {
            IGrainObserverSubscriptionManager<IObserve> manager;
            if (subscriptions.TryGetValue(typeof(TEvent), out manager))
                manager.Notify(x => Notify(x, e));
        }

        protected void Publish<TEvent>(TEvent e)
        {
            Bus.Publish(this, e);
        }
    }

    public abstract class ObservableGrainWithGuidId : ObservableGrain, IGrainWithGuidId
    {
        protected ObservableGrainWithGuidId()
        {
            Notify = (x, e) => x.On(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithLongId : ObservableGrain, IGrainWithLongId
    {
        protected ObservableGrainWithLongId()
        {
            Notify = (x, e) => x.On(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithStringId : ObservableGrain, IGrainWithStringId
    {
        protected ObservableGrainWithStringId()
        {
            Notify = (x, e) => x.On(Id(this), e);
        }
    }

    public abstract class ObservableGrain<TGrainState> : Grain<TGrainState>, IObservableGrain, IObservablePublisher 
        where TGrainState : class, IGrainState
    {
        public IServerMessageBus Bus = MessageBus.Server;

        readonly IDictionary<Type, IGrainObserverSubscriptionManager<IObserve>> subscriptions =
                    new Dictionary<Type, IGrainObserverSubscriptionManager<IObserve>>();

        protected Action<IObserve, object> Notify;

        public Task Subscribe(Type e, IObserve o)
        {
            IGrainObserverSubscriptionManager<IObserve> manager;
            if (!subscriptions.TryGetValue(e, out manager))
            {
                manager = Runtime.Create<IObserve>();
                subscriptions.Add(e, manager);
            }

            manager.Subscribe(o);
            return TaskDone.Done;
        }

        public Task Unsubscribe(Type e, IObserve o)
        {
            IGrainObserverSubscriptionManager<IObserve> manager;
            if (subscriptions.TryGetValue(e, out manager))
                manager.Unsubscribe(o);

            return TaskDone.Done;
        }

        void IObservablePublisher.Publish<TEvent>(TEvent e)
        {
            IGrainObserverSubscriptionManager<IObserve> manager;
            if (subscriptions.TryGetValue(typeof(TEvent), out manager))
                manager.Notify(x => Notify(x, e));
        }

        protected void Publish<TEvent>(TEvent e)
        {
            Bus.Publish(this, e);
        }
    }

    public abstract class ObservableGrainWithGuidId<TGrainState> : ObservableGrain<TGrainState>, IGrainWithGuidId
        where TGrainState : class, IGrainState
    {
        protected ObservableGrainWithGuidId()
        {
            Notify = (x, e) => x.On(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithLongId<TGrainState> : ObservableGrain<TGrainState>, IGrainWithLongId
        where TGrainState : class, IGrainState
    {
        protected ObservableGrainWithLongId()
        {
            Notify = (x, e) => x.On(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithStringId<TGrainState> : ObservableGrain<TGrainState>, IGrainWithStringId
        where TGrainState : class, IGrainState
    {
        protected ObservableGrainWithStringId()
        {
            Notify = (x, e) => x.On(Id(this), e);
        }
    }
}
