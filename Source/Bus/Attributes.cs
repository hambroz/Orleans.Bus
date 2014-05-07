using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Marker interface for grain to advertise which commands it handles
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class HandlesAttribute : Attribute
    {
        /// <summary>
        /// The type of command
        /// </summary>
        public readonly Type Command;

        /// <summary>
        /// Creates new instance of <see cref="HandlesAttribute"/>
        /// </summary>
        /// <param name="command">The type of command</param>
        public HandlesAttribute(Type command)
        {
            Command = command;
        }
    }

    /// <summary>
    /// Marker interface for grain to advertise which queries it answers
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class AnswersAttribute : Attribute
    {
        /// <summary>
        /// The type of query
        /// </summary>
        public readonly Type Query;

        /// <summary>
        /// Creates new instance of <see cref="AnswersAttribute"/>
        /// </summary>
        /// <param name="query">The type of query</param>
        public AnswersAttribute(Type query)
        {
            Query = query;
        }
    }

    /// <summary>
    /// Marker interface for grain to advertise events about which it notifes 
    ///  </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
    public sealed class NotifiesAttribute : Attribute
    {
        /// <summary>
        /// The type of event
        /// </summary>
        public readonly Type Event;

        /// <summary>
        /// Creates new instance of <see cref="NotifiesAttribute"/>
        /// </summary>
        /// <param name="event">The type of event</param>
        public NotifiesAttribute(Type @event)
        {
            Event = @event;
        }
    }

    /// <summary>
    /// Marker interface that should be placed on methods
    /// which will internally dispatch commands or queries
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DispatcherAttribute : Attribute
    {}
}
