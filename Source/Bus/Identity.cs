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
}
