using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace Orleans.Bus
{
    class GrainReferenceService
    {
        public static readonly GrainReferenceService Instance = new GrainReferenceService().Initialize();

        readonly IDictionary<Type, ByGuidInvoker> byGuid     = new Dictionary<Type, ByGuidInvoker>();
        readonly IDictionary<Type, ByInt64Invoker> byInt64   = new Dictionary<Type, ByInt64Invoker>();
        readonly IDictionary<Type, ByStringInvoker> byString = new Dictionary<Type, ByStringInvoker>();

        readonly List<FactoryProductBinding> bindings = new List<FactoryProductBinding>();

        GrainReferenceService Initialize()
        {
            Initialize(GrainFactoryBindings.WhereProductImplements);
            return this;
        }

        internal void Initialize(Func<Type, IEnumerable<FactoryProductBinding>> getBindings)
        {
            foreach (var binding in getBindings(typeof(IHaveGuidId)))
                BindByGuidId(binding);
            
            foreach (var binding in getBindings(typeof(IHaveInt64Id)))
                BindByInt64Id(binding);

            foreach (var binding in getBindings(typeof(IHaveStringId)))
                BindByStringId(binding);
        }

        void BindByGuidId(FactoryProductBinding binding)
        {
            byGuid[binding.Product] = new ByGuidInvoker(binding);
            bindings.Add(binding);
        }

        void BindByInt64Id(FactoryProductBinding binding)
        {
            byInt64[binding.Product] = new ByInt64Invoker(binding);
            bindings.Add(binding);
        }

        void BindByStringId(FactoryProductBinding binding)
        {
            if (!binding.Product.GetCustomAttributes(typeof(ExtendedPrimaryKeyAttribute), true).Any())
                throw new MissingExtendedPrimaryKeyAttributeException(binding.Product);

            byString[binding.Product] = new ByStringInvoker(binding);
            bindings.Add(binding);
        }

        public T Get<T>(Guid id)
        {
            return (T)Get(typeof(T), id);
        }

        public T Get<T>(long id)
        {
            return (T)Get(typeof(T), id);
        }

        public T Get<T>(string id)
        {
            return (T)Get(typeof(T), id);
        }

        public IHaveGuidId Get(Type @interface, Guid id)
        {
            var invoker = byGuid.Find(@interface);
            Debug.Assert(invoker != null);
            return invoker.Invoke(id);
        }

        public IHaveInt64Id Get(Type @interface, long id)
        {
            var invoker = byInt64.Find(@interface);
            Debug.Assert(invoker != null);
            return invoker.Invoke(id);
        }

        public IHaveStringId Get(Type @interface, string id)
        {
            var invoker = byString.Find(@interface);
            Debug.Assert(invoker != null);
            return invoker.Invoke(id);
        }

        public IEnumerable<Type> RegisteredGrainTypes()
        {
            return bindings.Select(x => x.Product);
        }

        class ByGuidInvoker
        {
            readonly Func<Guid, object> invoker;

            public ByGuidInvoker(FactoryProductBinding binding)
            {
                var method = binding.FactoryMethod("GetGrain", typeof(Guid));
                var argument = Expression.Parameter(typeof(Guid), "id");

                var call = Expression.Call(method, new Expression[] { argument });
                var lambda = Expression.Lambda<Func<Guid, object>>(call, argument);

                invoker = lambda.Compile();
            }

            public IHaveGuidId Invoke(Guid id)
            {
                return (IHaveGuidId)invoker(id);
            }
        }

        class ByInt64Invoker
        {
            readonly Func<long, object> invoker;

            public ByInt64Invoker(FactoryProductBinding binding)
            {
                var method = binding.FactoryMethod("GetGrain", typeof(long));
                var argument = Expression.Parameter(typeof(long), "id");

                var call = Expression.Call(method, new Expression[] { argument });
                var lambda = Expression.Lambda<Func<long, object>>(call, argument);

                invoker = lambda.Compile();
            }

            public IHaveInt64Id Invoke(long id)
            {
                return (IHaveInt64Id)invoker(id);
            }
        }

        class ByStringInvoker
        {
            readonly Func<string, object> invoker;

            public ByStringInvoker(FactoryProductBinding binding)
            {
                var method = binding.FactoryMethod("GetGrain", typeof(long), typeof(string));
                var argument = Expression.Parameter(typeof(string), "ext");

                var call = Expression.Call(method, new Expression[] { Expression.Constant(0L), argument});
                var lambda = Expression.Lambda<Func<string, object>>(call, argument);

                invoker = lambda.Compile();
            }

            public IHaveStringId Invoke(string id)
            {
                return (IHaveStringId) invoker(id);
            }
        }

        [Serializable]
        internal class MissingExtendedPrimaryKeyAttributeException : ApplicationException
        {
            const string message = "Grain '{0}' which implements IHaveStringId interface should be marked with [ExtendedPrimaryKey] attribute.";

            internal MissingExtendedPrimaryKeyAttributeException(Type grainType)
                : base(string.Format(message, grainType))
            {}

            protected MissingExtendedPrimaryKeyAttributeException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }
    }
}