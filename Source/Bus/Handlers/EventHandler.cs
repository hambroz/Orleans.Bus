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

        public async Task Subscribe(IObservableGrain observable, IObserve reference)
        {
            await observable.Subscribe(Event, reference);
        }

        public async Task Unsubscribe(IObservableGrain observable, IObserve reference)
        {
            await observable.Unsubscribe(Event, reference);
        }
    }
}
