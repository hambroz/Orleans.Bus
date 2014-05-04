using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using Fasterflect;

namespace Orleans.Bus
{
    internal static class OrleansStaticFactories
    {
        static readonly IList<Type> factories;

        static OrleansStaticFactories()
        {
            factories = LoadAssemblies()
                .SelectMany(assembly => assembly.ExportedTypes)
                .Where(IsOrleansCodegenedFactory)
                .ToList();
        }

        public static IEnumerable<FactoryProductBinding> WhereProduct(Func<Type, bool> predicate)
        {
            return from factory in factories
                   let product = factory.GetMethod("Cast").ReturnType
                   where predicate(product)
                   select new FactoryProductBinding(factory, product);
        }

        static IEnumerable<Assembly> LoadAssemblies()
        {
            var dir = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            Debug.Assert(dir != null);
            var dlls = Directory.GetFiles(dir, "*.dll");

            return dlls.Where(ContainsOrleansGeneratedCode)
                       .Select(Assembly.LoadFrom);
        }

        static bool ContainsOrleansGeneratedCode(string dll)
        {
            var info = FileVersionInfo.GetVersionInfo(dll);

            return info.Comments.ToLower() == "contains.orleans.generated.code";
        }

        static bool IsOrleansCodegenedFactory(Type type)
        {
            return type.GetCustomAttributes(typeof(GeneratedCodeAttribute), true)
                       .Cast<GeneratedCodeAttribute>()
                       .Any(x => x.Tool == "Orleans-CodeGenerator") 
                   && type.Name.EndsWith("Factory");
        }
    }

    internal class FactoryProductBinding
    {
        public readonly Type Factory;
        public readonly Type Product;

        public FactoryProductBinding(Type factory, Type product)
        {
            Factory = factory;
            Product = product;
        }

        public MethodInvoker FactoryMethodInvoker(string methodName, params Type[] parameterTypes)
        {
            return Factory.DelegateForCallMethod(methodName, Flags.StaticPublic, parameterTypes);
        }
    }
}