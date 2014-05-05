using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

using Fasterflect;

namespace Orleans.Bus
{
    class GrainReferenceService
    {
        public static readonly GrainReferenceService Instance = new GrainReferenceService().Initialize();

        readonly IDictionary<Type, MethodInvoker> castFactoryMethods = new Dictionary<Type, MethodInvoker>();
        readonly IDictionary<Type, MethodInvoker> getByGuidFactoryMethods = new Dictionary<Type, MethodInvoker>();
        readonly IDictionary<Type, MethodInvoker> getByLongFactoryMethods = new Dictionary<Type, MethodInvoker>();
        readonly IDictionary<Type, MethodInvoker> getByStringFactoryMethods = new Dictionary<Type, MethodInvoker>();

        GrainReferenceService Initialize()
        {
            var bindings = OrleansStaticFactories.WhereProduct(IsServicedGrain);

            foreach (var binding in bindings)
            {
                BindCast(binding);

                if (BindGetByString(binding))
                    continue;

                BindGetByGuid(binding);
                BindGetByLong(binding);
            }

            return this;
        }

        static bool IsServicedGrain(Type type)
        {
            return type.Implements(typeof(IHaveGuidId)) ||
                   type.Implements(typeof(IHaveInt64Id)) ||
                   type.Implements(typeof(IHaveStringId));
        }

        void BindCast(FactoryProductBinding binding)
        {
            castFactoryMethods[binding.Product] = 
                binding.FactoryMethodInvoker("Cast", typeof(IAddressable));
        }

        void BindGetByGuid(FactoryProductBinding binding)
        {
            getByGuidFactoryMethods[binding.Product] = 
                binding.FactoryMethodInvoker("GetGrain", typeof(Guid));
        }

        void BindGetByLong(FactoryProductBinding binding)
        {
            getByLongFactoryMethods[binding.Product] = 
                binding.FactoryMethodInvoker("GetGrain", typeof(long));
        }

        bool BindGetByString(FactoryProductBinding binding)
        {
            if (!binding.Product.HasAttribute<ExtendedPrimaryKeyAttribute>())
                return false;

            getByStringFactoryMethods[binding.Product] = 
                binding.FactoryMethodInvoker("GetGrain", typeof(long), typeof(string));

            return true;
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
            var invoker = getByGuidFactoryMethods.Find(@interface);
            Debug.Assert(invoker != null);

            return (IHaveGuidId)invoker.Invoke(null, id);
        }

        public IHaveInt64Id Get(Type @interface, long id)
        {
            var invoker = getByLongFactoryMethods.Find(@interface);
            Debug.Assert(invoker != null);

            return (IHaveInt64Id)invoker.Invoke(null, id);
        }

        public IHaveStringId Get(Type @interface, string id)
        {
            var invoker = getByStringFactoryMethods.Find(@interface);
            Debug.Assert(invoker != null);
            
            return (IHaveStringId)invoker.Invoke(null, 0L, id);
        }

        public IEnumerable<Type> RegisteredGrainTypes()
        {
            return castFactoryMethods.Keys;
        }  
    }
}