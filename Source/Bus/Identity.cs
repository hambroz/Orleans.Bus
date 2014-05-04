using System;
using System.Linq;

namespace Orleans.Bus
{
    /// <summary>
    /// Base interface for grains identifiable by GUID identifier
    /// </summary>
    public interface IGrainWithGuidId
    { }

    /// <summary>
    /// Base interface for grains identifiable by long identifier
    /// </summary>
    public interface IGrainWithLongId
    { }

    /// <summary>
    /// Base interface for grains identifiable by string identifier
    /// </summary>
    public interface IGrainWithStringId
    { }
}
