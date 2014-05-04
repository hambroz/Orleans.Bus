using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using Fasterflect;

namespace Orleans.Bus
{
    /// <summary>
    /// Provides services to get identity of a grain and reference to it
    /// </summary>
    public interface IGrainReferenceService
    {
        /// <summary>
        /// Gets reference to the activation of the grain by its unique GUID identifier
        /// </summary>
        /// <typeparam name="T">Type of grain</typeparam>
        /// <param name="id">Unique identifier</param>
        /// <returns>Grain reference</returns>
        T Reference<T>(Guid id) where T : IGrainWithGuidId;

        /// <summary>
        /// Gets reference to the activation of the grain by its unique long  identifier
        /// </summary>
        /// <typeparam name="T">Type of grain</typeparam>
        /// <param name="id">Unique identifier</param>
        /// <returns>Grain reference</returns>
        T Reference<T>(long id) where T : IGrainWithLongId;

        /// <summary>
        /// Gets reference to the activation of the grain by its unique string identifier
        /// </summary>
        /// <typeparam name="T">Type of grain</typeparam>
        /// <param name="id">Unique identifier</param>
        /// <returns>Grain reference</returns>
        T Reference<T>(string id) where T : IGrainWithStringId;

        /// <summary>
        /// Gets reference to the activation of the grain by its unique GUID identifier
        /// </summary>
        /// <param name="interface">Type of grain interface</param>
        /// <param name="id">Unique identifier</param>
        /// <returns>Grain reference</returns>
        IGrainWithGuidId Reference(Type @interface, Guid id);

        /// <summary>
        /// Gets reference to the activation of the grain by its unique long  identifier
        /// </summary>
        /// <param name="interface">Type of grain interface</param>
        /// <param name="id">Unique identifier</param>
        /// <returns>Grain reference</returns>
        IGrainWithLongId Reference(Type @interface, long id);

        /// <summary>
        /// Gets reference to the activation of the grain by its unique string identifier
        /// </summary>
        /// <param name="interface">Type of grain interface</param>
        /// <param name="id">Unique identifier</param>
        /// <returns>Grain reference</returns>
        IGrainWithStringId Reference(Type @interface, string id);

        /// <summary>
        /// Returns all grain interface types registered within current runtime
        /// </summary>
        /// <returns>List of <seealso cref="Type"/> which represent grain interface types</returns>
        IEnumerable<Type> RegisteredGrainTypes();
    }

    public sealed partial class GrainRuntime : IGrainRuntime
    {
        readonly IDictionary<Type, MethodInvoker> castFactoryMethods = new Dictionary<Type, MethodInvoker>();
        readonly IDictionary<Type, MethodInvoker> getByGuidFactoryMethods = new Dictionary<Type, MethodInvoker>();
        readonly IDictionary<Type, MethodInvoker> getByLongFactoryMethods = new Dictionary<Type, MethodInvoker>();
        readonly IDictionary<Type, MethodInvoker> getByStringFactoryMethods = new Dictionary<Type, MethodInvoker>();

        void InitializeReferenceService()
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
        }

        static bool IsServicedGrain(Type type)
        {
            return type.Implements(typeof(IGrainWithGuidId)) ||
                   type.Implements(typeof(IGrainWithLongId)) ||
                   type.Implements(typeof(IGrainWithStringId));
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

        T IGrainReferenceService.Reference<T>(Guid id)
        {
            return (T)(this as IGrainRuntime).Reference(typeof(T), id);
        }

        T IGrainReferenceService.Reference<T>(long id)
        {
            return (T)(this as IGrainRuntime).Reference(typeof(T), id);
        }

        T IGrainReferenceService.Reference<T>(string id)
        {
            return (T)(this as IGrainRuntime).Reference(typeof(T), id);
        }

        IGrainWithGuidId IGrainReferenceService.Reference(Type @interface, Guid id)
        {
            var invoker = getByGuidFactoryMethods.Find(@interface);

            if (invoker != null)
                return (IGrainWithGuidId)invoker.Invoke(null, id);

            if (!@interface.IsInterface)
                throw new AccessByClassTypeException(@interface);

            throw new FactoryMethodNotFoundException(@interface);
        }

        IGrainWithLongId IGrainReferenceService.Reference(Type @interface, long id)
        {
            var invoker = getByLongFactoryMethods.Find(@interface);

            if (invoker != null)
                return (IGrainWithLongId)invoker.Invoke(null, id);

            if (!@interface.IsInterface)
                throw new AccessByClassTypeException(@interface);

            throw new FactoryMethodNotFoundException(@interface);
        }

        IGrainWithStringId IGrainReferenceService.Reference(Type @interface, string id)
        {
            var invoker = getByStringFactoryMethods.Find(@interface);

            if (invoker != null)
                return (IGrainWithStringId)invoker.Invoke(null, 0L, id);

            if (!@interface.IsInterface)
                throw new AccessByClassTypeException(@interface);

            if (!@interface.GetCustomAttributes(typeof(ExtendedPrimaryKeyAttribute), true).Any())
                throw new MissingExtendedPrimaryKeyAttributeException(@interface);

            throw new FactoryMethodNotFoundException(@interface);
        }

        IEnumerable<Type> IGrainReferenceService.RegisteredGrainTypes()
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