using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Orleans.Bus
{
    internal static class ReflectionExtensions
    {
        public static bool HasAttribute<TAttribute>(
            this ICustomAttributeProvider provider, bool inherit = true) 
            where TAttribute : Attribute
        {
            return provider.GetCustomAttributes(typeof(TAttribute), inherit).Any();
        }

        public static IEnumerable<TAttribute> Attributes<TAttribute>(
            this ICustomAttributeProvider provider, bool inherit = true) 
            where TAttribute : Attribute
        {
            return provider.GetCustomAttributes(typeof(TAttribute), inherit).Cast<TAttribute>();
        }

        public static bool Implements<TInterface>(this Type type)
        {
            return type.Implements(typeof(TInterface));
        }

        public static bool Implements(this Type type, Type @interface)
        {
            return @interface.IsAssignableFrom(type);
        }
    }
}
