using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Provides services to get identity of any grain
    /// </summary>
    public interface IGrainIdentityService
    {
        /// <summary>
        /// Gets identifier of the specified grain.
        /// </summary>
        /// <param name="grain">Instance of the grain, for which identifier should be returned</param>
        /// <returns><see cref="Guid"/> identifier</returns>
        Guid Id(IHaveGuidId grain);

        /// <summary>
        /// Gets identifier of the specified grain.
        /// </summary>
        /// <param name="grain">Instance of the grain, for which identifier should be returned</param>
        /// <returns><see cref="Int64"/> identifier</returns>
        long Id(IHaveInt64Id grain);

        /// <summary>
        /// Gets identifier of the specified grain.
        /// </summary>
        /// <param name="grain">Instance of the grain, for which identifier should be returned</param>
        /// <returns><see cref="String"/> identifier</returns>
        string Id(IHaveStringId grain);
    }

    public sealed partial class GrainRuntime
    {
        Guid IGrainIdentityService.Id(IHaveGuidId grain)
        {
            return ((IGrain)grain).GetPrimaryKey();
        }

        long IGrainIdentityService.Id(IHaveInt64Id grain)
        {
            return ((IGrain)grain).GetPrimaryKeyLong();
        }

        string IGrainIdentityService.Id(IHaveStringId grain)
        {
            string id;
            ((IGrain)grain).GetPrimaryKeyLong(out id);
            return id;
        }
    }
}