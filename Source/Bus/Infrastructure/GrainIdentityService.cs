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
        Guid Id(IGrainWithGuidId grain);

        /// <summary>
        /// Gets identifier of the specified grain.
        /// </summary>
        /// <param name="grain">Instance of the grain, for which identifier should be returned</param>
        /// <returns><see cref="Int64"/> identifier</returns>
        long Id(IGrainWithLongId grain);

        /// <summary>
        /// Gets identifier of the specified grain.
        /// </summary>
        /// <param name="grain">Instance of the grain, for which identifier should be returned</param>
        /// <returns><see cref="String"/> identifier</returns>
        string Id(IGrainWithStringId grain);
    }

    public sealed partial class GrainRuntime
    {
        Guid IGrainIdentityService.Id(IGrainWithGuidId grain)
        {
            return ((IGrain)grain).GetPrimaryKey();
        }

        long IGrainIdentityService.Id(IGrainWithLongId grain)
        {
            return ((IGrain)grain).GetPrimaryKeyLong();
        }

        string IGrainIdentityService.Id(IGrainWithStringId grain)
        {
            string id;
            ((IGrain)grain).GetPrimaryKeyLong(out id);
            return id;
        }
    }
}