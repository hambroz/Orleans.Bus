using System;
using System.Collections.Generic;
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

            if (invoker != null)
                return (IHaveGuidId)invoker.Invoke(null, id);

            if (!@interface.IsInterface)
                throw new AccessByClassTypeException(@interface);

            throw new FactoryMethodNotFoundException(@interface);
        }

        public IHaveInt64Id Get(Type @interface, long id)
        {
            var invoker = getByLongFactoryMethods.Find(@interface);

            if (invoker != null)
                return (IHaveInt64Id)invoker.Invoke(null, id);

            if (!@interface.IsInterface)
                throw new AccessByClassTypeException(@interface);

            throw new FactoryMethodNotFoundException(@interface);
        }

        public IHaveStringId Get(Type @interface, string id)
        {
            var invoker = getByStringFactoryMethods.Find(@interface);

            if (invoker != null)
                return (IHaveStringId)invoker.Invoke(null, 0L, id);

            if (!@interface.IsInterface)
                throw new AccessByClassTypeException(@interface);

            if (!@interface.GetCustomAttributes(typeof(ExtendedPrimaryKeyAttribute), true).Any())
                throw new MissingExtendedPrimaryKeyAttributeException(@interface);

            throw new FactoryMethodNotFoundException(@interface);
        }

        public IEnumerable<Type> RegisteredGrainTypes()
        {
            return castFactoryMethods.Keys;
        }

        [Serializable]
        internal class FactoryMethodNotFoundException : ApplicationException
        {
            const string message = "Can't find factory method for {0}. Factory class was not found during initial scan.";

            internal FactoryMethodNotFoundException(Type grainType)
                : base(string.Format(message, grainType))
            {}

            protected FactoryMethodNotFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        [Serializable]
        internal class AccessByClassTypeException : ApplicationException
        {
            const string message = "Can't get reference by class type {0}. Use original interface instead.";

            internal AccessByClassTypeException(Type grainType)
                : base(string.Format(message, grainType))
            {}

            protected AccessByClassTypeException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        [Serializable]
        internal class MissingExtendedPrimaryKeyAttributeException : ApplicationException
        {
            const string message = "Can't get {0} by string id. Make sure that interface is marked with [ExtendedPrimaryKey] attribute.";

            internal MissingExtendedPrimaryKeyAttributeException(Type grainType)
                : base(string.Format(message, grainType))
            {}

            protected MissingExtendedPrimaryKeyAttributeException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }
    }
}