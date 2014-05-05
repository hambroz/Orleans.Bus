using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Base interface for grains identifiable by <see cref="Guid"/> identifier
    /// </summary>
    public interface IHaveGuidId
    {}

    /// <summary>
    /// Base interface for grains identifiable by <see cref="Int64"/> identifier
    /// </summary>
    public interface IHaveInt64Id
    {}

    /// <summary>
    /// Base interface for grains identifiable by <see cref="String"/> identifier
    /// </summary>
    public interface IHaveStringId
    {}

    /// <summary>
    /// Provides services to get identity of the grain
    /// </summary>
    public static class Identity
    {
        /// <summary>
        /// Gets identifier of the specified grain.
        /// </summary>
        /// <param name="grain">Instance of the grain, for which identifier should be returned</param>
        /// <returns><see cref="Guid"/> identifier</returns>
        public static Guid Of(IHaveGuidId grain)
        {
            return ((IGrain)grain).GetPrimaryKey();
        }

        /// <summary>
        /// Gets identifier of the specified grain.
        /// </summary>
        /// <param name="grain">Instance of the grain, for which identifier should be returned</param>
        /// <returns><see cref="Int64"/> identifier</returns>
        public static long Of(IHaveInt64Id grain)
        {
            return ((IGrain)grain).GetPrimaryKeyLong();
        }

        /// <summary>
        /// Gets identifier of the specified grain.
        /// </summary>
        /// <param name="grain">Instance of the grain, for which identifier should be returned</param>
        /// <returns><see cref="String"/> identifier</returns>
        public static string Of(IHaveStringId grain)
        {
            string id;
            ((IGrain)grain).GetPrimaryKeyLong(out id);
            return id;
        }
    }
}
