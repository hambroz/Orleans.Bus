using System;
using System.Linq;

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
    }
}
