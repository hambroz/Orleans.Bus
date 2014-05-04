using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans.IoC;

namespace Orleans.Bus
{
    public abstract class ObservableGrain : Grain, IObservableGrain
    {
        public IObserverCollection Observers = new ObserverCollection();

        public Task Attach(Observes s, Type e)
        {
            Observers.Attach(s, e);
            return TaskDone.Done;
        }

        public Task Detach(Observes o, Type e)
        {
            Observers.Detach(o, e);
            return TaskDone.Done;
        }
    }

    public abstract class ObservableGrainWithGuidId : ObservableGrain, IGrainWithGuidId
    {
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithLongId : ObservableGrain, IGrainWithLongId
    {
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithStringId : ObservableGrain, IGrainWithStringId
    {
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrain<TGrainState> : Grain<TGrainState>, IObservableGrain
        where TGrainState : class, IGrainState
    {
        public IObserverCollection Observers = new ObserverCollection();

        public Task Attach(Observes s, Type e)
        {
            Observers.Attach(s, e);
            return TaskDone.Done;
        }

        public Task Detach(Observes o, Type e)
        {
            Observers.Detach(o, e);
            return TaskDone.Done;
        }
    }

    public abstract class ObservableGrainWithGuidId<TGrainState> : ObservableGrain<TGrainState>, IGrainWithGuidId
        where TGrainState : class, IGrainState
    {
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithLongId<TGrainState> : ObservableGrain<TGrainState>, IGrainWithLongId
        where TGrainState : class, IGrainState
    {
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }

    public abstract class ObservableGrainWithStringId<TGrainState> : ObservableGrain<TGrainState>, IGrainWithStringId
        where TGrainState : class, IGrainState
    {
        protected void Notify<TEvent>(TEvent e)
        {
            Observers.Notify(Id(this), e);
        }
    }
}
