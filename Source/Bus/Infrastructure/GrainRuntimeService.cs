using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Provides runtime services
    /// </summary>
    public interface IGrainRuntime : IGrainReferenceService, IGrainIdentityService, IGrainObserverReferenceService
    {
    }

    /// <summary>
    /// Default implementation of <see cref="IGrainRuntime"/>
    /// </summary>
    public sealed partial class GrainRuntime
    {
        /// <summary>
        /// Globally accessible instance of  the <see cref="IGrainRuntime"/>.
        /// </summary>
        public static readonly IGrainRuntime Instance = new GrainRuntime().Initialize();

        IGrainRuntime Initialize()
        {
            InitializeReferenceService();
            InitializeObserverReferenceService();

            return this;
        }
    }
}