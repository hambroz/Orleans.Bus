using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Orleans.Bus
{
    internal static class GrainFactoryBindings
    {
        static readonly IList<Type> factories;

        static GrainFactoryBindings()
        {
            factories = LoadAssemblies()
                .SelectMany(assembly => assembly.ExportedTypes)
                .Where(IsOrleansCodegenedFactory)
                .ToList();
        }

        public static IEnumerable<FactoryProductBinding> WhereProductImplements(Type @interface)
        {
            return from factory in factories
                   let product = factory.GetMethod("Cast").ReturnType
                   where product.Implements(@interface)
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

        public MethodInfo FactoryMethod(string name, params Type[] parameters)
        {
            return Factory.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, parameters, null);
        }
    }
}