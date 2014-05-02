using System;
using System.Linq;

namespace Orleans.Bus
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HandlerAttribute : Attribute
    {}

    [AttributeUsage(AttributeTargets.Interface)]
    public class PublisherAttribute : Attribute
    {
        public readonly Type Event;

        public PublisherAttribute(Type @event)
        {
            Event = @event;
        }
    }
}