using System;
using System.Linq;

namespace Orleans.Bus
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HandlerAttribute : Attribute
    {}
}
