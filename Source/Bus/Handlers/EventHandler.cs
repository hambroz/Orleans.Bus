using System;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    class EventHandler
    {
        public readonly Type Grain;
        public readonly Type Event;

        EventHandler(Type grain, Type @event)
        {
            Grain = grain;
            Event = @event;
        }

        public static EventHandler Create(Type grain, Type @event)
        {
            return new EventHandler(grain, @event);
        }

        public async Task Subscribe(object grain, IObserver observer)
        {
            var observable = (IObservableGrain) grain;
            await observable.Attach(observer.GetProxy(), Event);
        }

        public async Task Unsubscribe(object grain, IObserver observer)
        {
            var observable = (IObservableGrain)grain;
            await observable.Detach(observer.GetProxy(), Event);
        }
    }
}
