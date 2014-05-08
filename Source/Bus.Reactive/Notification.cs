using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Represents strictly-typed event notification
    /// </summary>
    public class Notification<T>
    {
        /// <summary>
        /// Id of the source grain
        /// </summary>
        public readonly string Source;

        /// <summary>
        /// Event message
        /// </summary>
        public readonly T Message;

        internal Notification(string source, T message)
        {
            Source = source;
            Message = message;
        }
    }

    /// <summary>
    /// Represents loosely-typed event notification
    /// </summary>
    public class Notification : Notification<object>
    {
        internal Notification(string source, object message)
            : base(source, message)
        {}
    }
}