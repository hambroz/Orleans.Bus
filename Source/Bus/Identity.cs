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

        /// <summary>
        /// Returns runtime identity of the given grain reference
        /// </summary>
        /// <param name="reference">Runtime grain reference</param>
        /// <returns>Id assigned to the underlying grain</returns>
        public static string Of(GrainReference reference)
        {
            var key = reference.ToKeyString();
            return key.Substring(key.IndexOf("+", StringComparison.Ordinal) + 1);
        }        
        
        /// <summary>
        /// Returns runtime identity of this grain reference
        /// </summary>
        /// <param name="reference">Runtime grain reference</param>
        /// <returns>Id assigned to the underlying grain</returns>
        public static string Id(this GrainReference reference)
        {
            return Identity.Of(reference);
        }
    }
}
