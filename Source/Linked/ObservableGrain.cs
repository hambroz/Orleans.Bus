using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    public abstract class ObservableGrain : Grain, IObservableGrain
    {
        public ObserverCollection Observers;

        protected ObservableGrain()
        {
            Observers = new ObserverCollection(Runtime);
        }

        public Task Subscribe(Type e, ObserverReference<Observes> o)
        {
            Observers.Attach(o, e);
            return TaskDone.Done;
        }

        public Task Unsubscribe(Type e, ObserverReference<Observes> o)
        {
            Observers.Detach(o, e);
            return TaskDone.Done;
        }
    }

    public abstract class ObservableGrainWithGuidId : ObservableGrain, IGrainWithGuidId
    {
        protected void Publish<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithLongId : ObservableGrain, IGrainWithLongId
    {
        protected void Publish<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithStringId : ObservableGrain, IGrainWithStringId
    {
        protected void Publish<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrain<TGrainState> : Grain<TGrainState>, IObservableGrain
        where TGrainState : class, IGrainState
    {
        public ObserverCollection Observers;

        protected ObservableGrain()
        {
            Observers = new ObserverCollection(Runtime);
        }

        public Task Subscribe(Type e, ObserverReference<Observes> o)
        {
            Observers.Attach(o, e);
            return TaskDone.Done;
        }

        public Task Unsubscribe(Type e, ObserverReference<Observes> o)
        {
            Observers.Detach(o, e);
            return TaskDone.Done;
        }
    }

    public abstract class ObservableGrainWithGuidId<TGrainState> : ObservableGrain<TGrainState>, IGrainWithGuidId
        where TGrainState : class, IGrainState
    {
        protected void Publish<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithLongId<TGrainState> : ObservableGrain<TGrainState>, IGrainWithLongId
        where TGrainState : class, IGrainState
    {
        protected void Publish<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithStringId<TGrainState> : ObservableGrain<TGrainState>, IGrainWithStringId
        where TGrainState : class, IGrainState
    {
        protected void Publish<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }
}
