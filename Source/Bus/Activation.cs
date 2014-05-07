using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Manages grain activation lifetime
    /// </summary>
    public interface IActivation
    {
        /// <summary>
        /// Deactivate this activation of the grain after the current grain method call is completed.
        ///             This call will mark this activation of the current grain to be deactivated and removed at the end of the current method.
        ///             The next call to this grain will result in a different activation to be used, which typical means a new activation will be created automatically by the runtime.
        /// 
        /// </summary>
        void DeactivateOnIdle();

        /// <summary>
        /// Delay Deactivation of this activation at least for the specified time duration.
        ///             DeactivateOnIdle method would undo / override any current “keep alive” setting,
        ///             making this grain immediately available  for deactivation.
        /// 
        /// </summary>
        /// <param name="period">
        /// <para>A positive value means “prevent GC of this activation for that time span”</para> 
        /// <para>A negative value means “unlock, and make this activation available for GC again”</para>
        /// </param>
        void DelayDeactivation(TimeSpan period);
    }

    class Activation : IActivation
    {
        readonly IExposeGrainInternals grain;

        public Activation(IExposeGrainInternals grain)
        {
            this.grain = grain;
        }

        public void DeactivateOnIdle()
        {
            grain.DeactivateOnIdle();
        }

        public void DelayDeactivation(TimeSpan period)
        {
            grain.DelayDeactivation(period);
        } 
    }
}