using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    public interface IReminderCollection
    {
        /// <summary>
        /// Registers a persistent, reliable reminder to send regular notifications (Reminders) to the grain.
        ///             The grain must implement the <c>Orleans.IRemindable</c> interface, and Reminders for this grain will be sent to the <c>ReceiveReminder</c> callback method.
        ///             If the current grain is deactivated when the timer fires, a new activation of this grain will be created to receive this reminder.
        ///             If an existing reminder with the same id already exists, that reminder will be overwritten with this new reminder.
        ///             Reminders will always be received by one activation of this grain, even if multiple activations exist for this grain.
        /// 
        /// </summary>
        /// <param name="id">Unique id of the reminder</param>
        /// <param name="due">Due time for this reminder</param>
        /// <param name="period">Frequence period for this reminder</param>
        /// <returns>
        /// Promise for Reminder registration.
        /// </returns>
        Task Register(string id, TimeSpan due, TimeSpan period);

        /// <summary>
        /// Unregister previously registered peristent reminder if any
        /// </summary>
        /// <param name="id">Unique id of the reminder</param>
        Task Unregister(string id);

        /// <summary>
        /// Checks whether reminder with the given id is currently registered
        /// </summary>
        /// <param name="id">Unique id of the reminder</param>
        /// <returns><c>true</c> if reminder with the give name is currently registered, <c>false</c> otherwise </returns>
        Task<bool> IsRegistered(string id);

        /// <summary>
        /// Returns ids of all currently registered reminders
        /// </summary>
        /// <returns>Sequence of <see cref="string"/> elements</returns>
        Task<IEnumerable<string>> Registered();
    }

    class ReminderCollection : IReminderCollection
    {
        readonly IDictionary<string, IOrleansReminder> reminders = new Dictionary<string, IOrleansReminder>();
        readonly IExposeGrainInternals grain;

        public ReminderCollection(IExposeGrainInternals grain)
        {
            this.grain = grain;
        }

        public async Task Register(string id, TimeSpan due, TimeSpan period)
        {
            reminders[id] = await grain.RegisterOrUpdateReminder(id, due, period);
        }

        public async Task Unregister(string id)
        {
            var reminder = reminders.Find(id) ?? await grain.GetReminder(id);
            
            if (reminder != null)
                await grain.UnregisterReminder(reminder);

            reminders.Remove(id);
        }

        public async Task<bool> IsRegistered(string id)
        {
            return reminders.ContainsKey(id) || (await grain.GetReminder(id)) != null;
        }

        public async Task<IEnumerable<string>> Registered()
        {
            return (await grain.GetReminders()).Select(x => x.ReminderName);
        }
    }
}