using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    public abstract class ObservableGrain : Grain, IObservableGrain
    {
        readonly IDictionary<Type, IGrainObserverSubscriptionManager<Observes>> subscriptions = 
                    new Dictionary<Type, IGrainObserverSubscriptionManager<Observes>>();

        protected Action<Observes, object> Notify;

        public Task Subscribe(Type e, ObserverReference<Observes> o)
        {
            IGrainObserverSubscriptionManager<Observes> manager;
            if (!subscriptions.TryGetValue(e, out manager))
            {
                manager = Runtime.CreateSubscriptionManager<Observes>();
                subscriptions.Add(e, manager);
            }

            manager.Subscribe(o);
            return TaskDone.Done;
        }

        public Task Unsubscribe(Type e, ObserverReference<Observes> o)
        {
            IGrainObserverSubscriptionManager<Observes> manager;
            if (subscriptions.TryGetValue(e, out manager))
                manager.Unsubscribe(o);
            
            return TaskDone.Done;
        }

        protected void Publish<TEvent>(TEvent e)
        {
            IGrainObserverSubscriptionManager<Observes> manager;
            if (subscriptions.TryGetValue(typeof(TEvent), out manager))
                manager.Notify(x => Notify(x, e));
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

    public abstract class ObservableGrain<TGrainState> : Grain<TGrainState>, IObservableGrain
        where TGrainState : class, IGrainState
    {
        readonly IDictionary<Type, IGrainObserverSubscriptionManager<Observes>> subscriptions =
                    new Dictionary<Type, IGrainObserverSubscriptionManager<Observes>>();

        protected Action<Observes, object> Notify;


        public Task Subscribe(Type e, ObserverReference<Observes> o)
        {
            IGrainObserverSubscriptionManager<Observes> manager;
            if (!subscriptions.TryGetValue(e, out manager))
            {
                manager = Runtime.CreateSubscriptionManager<Observes>();
                subscriptions.Add(e, manager);
            }

            manager.Subscribe(o);
            return TaskDone.Done;
        }

        public Task Unsubscribe(Type e, ObserverReference<Observes> o)
        {
            IGrainObserverSubscriptionManager<Observes> manager;
            if (subscriptions.TryGetValue(e, out manager))
                manager.Unsubscribe(o);

            return TaskDone.Done;
        }

        protected void Publish<TEvent>(TEvent e)
        {
            IGrainObserverSubscriptionManager<Observes> manager;
            if (subscriptions.TryGetValue(typeof(TEvent), out manager))
                manager.Notify(x => Notify(x, e));
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
