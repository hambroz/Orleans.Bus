using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Grain identity extensions
    /// </summary>
    public static class Identity
    {
        /// <summary>
        /// Returns runtime identity of the given grain
        /// </summary>
        /// <param name="grain">Runtime grain instance</param>
        /// <returns>Id assigned to the grain</returns>
        public static string Of(IGrain grain)
        {
            string id;
            grain.GetPrimaryKeyLong(out id);
            return id;
        }
    }
}
