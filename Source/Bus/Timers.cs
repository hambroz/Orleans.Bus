using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface ITimerCollection
    {
        /// <summary>
        /// Registers a timer to send periodic callbacks to this grain.
        /// 
        /// </summary>
        /// 
        /// <remarks>
        /// 
        /// <para>
        /// This timer will not prevent the current grain from being deactivated.
        ///             If the grain is deactivated, then the timer will be discarded.
        /// 
        /// </para>
        /// 
        /// <para>
        /// Until the Task returned from the <paramref name="callback"/> is resolved,
        ///             the next timer tick will not be scheduled.
        ///             That is to say, timer callbacks never interleave their turns.
        /// 
        /// </para>
        /// 
        /// <para>
        /// The timer may be stopped at any time by calling the <see cref="Unregister(string)"/> method
        /// 
        /// </para>
        /// 
        /// <para>
        /// Any exceptions thrown by or faulted Task's returned from the  <paramref name="callback"/>
        ///             will be logged, but will not prevent the next timer tick from being queued.
        /// 
        /// </para>
        /// 
        /// </remarks>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        void Register(string id, Func<Task> callback, TimeSpan due, TimeSpan period);
        
        /// <summary>
        /// Registers a timer to send periodic callbacks to this grain.
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        /// <param name="callback">Callback function to be invoked when timer ticks.</param>
        /// <param name="state">State object that will be passed as argument when calling the  <paramref name="callback"/>.</param>
        /// <param name="due">Due time for first timer tick.</param>
        /// <param name="period">Period of subsequent timer ticks.</param>
        void Register<TState>(string id, Func<TState, Task> callback, TState state, TimeSpan due, TimeSpan period);

        /// <summary>
        /// Unregister previously registered timer. 
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        void Unregister(string id);

        /// <summary>
        /// Checks whether timer with the given name was registered before
        /// </summary>
        /// <param name="id">Unique id of the timer</param>
        /// <returns><c>true</c> if timer was the give name was previously registered, <c>false</c> otherwise </returns>
        bool IsRegistered(string id);

        /// <summary>
        /// Returns ids of all currently registered timers
        /// </summary>
        /// <returns>Sequence of <see cref="string"/> elements</returns>
        IEnumerable<string> Registered();
    }

    class TimerCollection : ITimerCollection
    {
        readonly IDictionary<string, IOrleansTimer> timers = new Dictionary<string, IOrleansTimer>();
        readonly IExposeGrainInternals grain;

        public TimerCollection(IExposeGrainInternals grain)
        {
            this.grain = grain;
        }

        public void Register(string id, Func<Task> callback, TimeSpan due, TimeSpan period)
        {
            timers.Add(id, grain.RegisterTimer(s => callback(), null, due, period));
        }

        public void Register<TState>(string id, Func<TState, Task> callback, TState state, TimeSpan due, TimeSpan period)
        {
            timers.Add(id, grain.RegisterTimer(s => callback((TState)s), state, due, period));
        }

        public void Unregister(string id)
        {
            var timer = timers[id];
            timer.Dispose();
        }

        public bool IsRegistered(string id)
        {
            return timers.ContainsKey(id);
        }

        public IEnumerable<string> Registered()
        {
            return timers.Keys;
        }
    }
}
