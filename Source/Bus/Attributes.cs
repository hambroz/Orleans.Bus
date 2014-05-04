using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Marker interface that should be placed on methods
    /// which handle commands or queries
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HandlerAttribute : Attribute
    {}

    /// <summary>
    /// Marker interface that should be used to advertise what kind of events
    /// could be published by the grain
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class PublisherAttribute : Attribute
    {
        /// <summary>
        /// The type of event
        /// </summary>
        public readonly Type Event;

        /// <summary>
        /// Creates new instance of <see cref="PublisherAttribute"/>
        /// </summary>
        /// <param name="event">The type of published event</param>
        public PublisherAttribute(Type @event)
        {
            Event = @event;
        }
    }
}
