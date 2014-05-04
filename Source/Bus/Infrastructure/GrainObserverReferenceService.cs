using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Fasterflect;

namespace Orleans.Bus
{
    /// <summary>
    /// Grain observer reference fosr external client, that could be used for subscribing to grain notifications
    /// </summary>
    /// <typeparam name="TGrainObserver">Type that implements <see cref="IGrainObserver"/> interface</typeparam>
    [Serializable]
    public sealed class ObserverReference<TGrainObserver> 
        where TGrainObserver : IGrainObserver
    {
        /// <summary>
        /// Actual  proxy grain used to deliver notifications to the client
        /// </summary>
        public readonly TGrainObserver Proxy;

        /// <summary>
        /// For internal use only!
        /// </summary>
        /// <param name="proxy">For internal use only!</param>
        public ObserverReference(TGrainObserver proxy)
        {
            Proxy = proxy;
        }
    }

    /// <summary>
    /// Provides services to obtain observer references for clients (those who don't implement <see cref="IGrain"/> interface)
    /// </summary>
    /// <remarks>
    /// Cast client reference to original observer interface 
    /// before passing it to any of this service generic methods!
    /// </remarks>
    public interface IGrainObserverReferenceService
    {
        /// <summary>
        /// Creates observer reference for given client, which could be used for subscribing to grain notifications
        /// </summary>
        /// <param name="client">Client which implements <typeparamref name="TGrainObserver"/> interface </param>
        /// <typeparam name="TGrainObserver">Type which implements <see cref="IGrainObserver"/> interface</typeparam>
        /// <returns>Task with observer reference which could be used for subscribing to grain notifications</returns>
        Task<ObserverReference<TGrainObserver>> CreateObserverReference<TGrainObserver>(TGrainObserver client) 
            where TGrainObserver : IGrainObserver;

        /// <summary>
        /// Deletes previously created observer reference for given client
        /// </summary>
        /// <param name="reference">Previously obtained observer reference </param>
        /// <typeparam name="TGrainObserver">Type which implements <see cref="IGrainObserver"/> interface</typeparam>
        void DeleteObserverReference<TGrainObserver>(ObserverReference<TGrainObserver> reference) 
            where TGrainObserver : IGrainObserver;

        /// <summary>
        /// Creates observer reference for given client, which could be used for subscribing to grain notifications
        /// </summary>
        /// <param name="interface">Actual type of observer interface</param>
        /// <param name="client">Client which implements <paramref name="interface"/> interface </param>
        /// <returns>Task with observer reference which could be used for subscribing to grain notifications</returns>
        Task<ObserverReference<IGrainObserver>> CreateObserverReference(Type @interface, IGrainObserver client);

        /// <summary>
        /// Deletes previously created observer reference for given client
        /// </summary>
        /// <param name="interface">Actual type of observer interface</param>
        /// <param name="reference">Previously obtained observer reference </param>
        void DeleteObserverReference(Type @interface, ObserverReference<IGrainObserver> reference);

        /// <summary>
        /// Returns all grain observer interface types registered within current runtime
        /// </summary>
        /// <returns>List of <seealso cref="Type"/> which represent grain observer interface types</returns>
        IEnumerable<Type> RegisteredGrainObserverTypes();
    }

    public sealed partial class GrainRuntime
    {
        /// <summary>
        /// Globally accessible instance of  the <see cref="IGrainObserverReferenceService"/>.
        /// </summary>
        /// <remarks>
        /// Could be substituted within a test harness
        /// </remarks>
        readonly IDictionary<Type, MethodInvoker> createObjectReferenceFactoryMethods = new Dictionary<Type, MethodInvoker>();
        readonly IDictionary<Type, MethodInvoker> deleteObjectReferenceFactoryMethods = new Dictionary<Type, MethodInvoker>();

        void InitializeObserverReferenceService()
        {
            var bindings = OrleansStaticFactories.WhereProduct(IsGrainObserver);

            foreach (var binding in bindings)
            {
                BindCreateObjectReference(binding);
                BindDeleteObjectReference(binding);
            }
        }

        static bool IsGrainObserver(Type type)
        {
            return type.Implements(typeof(IGrainObserver)) ||
                   !type.Implements<IGrain>();
        }

        void BindCreateObjectReference(FactoryProductBinding binding)
        {
            createObjectReferenceFactoryMethods[binding.Product] = 
                binding.FactoryMethodInvoker("CreateObjectReference", binding.Product);
        }

        void BindDeleteObjectReference(FactoryProductBinding binding)
        {
            deleteObjectReferenceFactoryMethods[binding.Product] = 
                binding.FactoryMethodInvoker("DeleteObjectReference", binding.Product);
        }

        async Task<ObserverReference<TGrainObserver>> IGrainObserverReferenceService.CreateObserverReference<TGrainObserver>(TGrainObserver client)
        {
            var invoker = createObjectReferenceFactoryMethods.Find(typeof(TGrainObserver));

            if (invoker == null)
                throw ObserverFactoryMethodNotFoundException.Create(typeof(TGrainObserver), "CreateObjectReference()");

            return new ObserverReference<TGrainObserver>(await (Task<TGrainObserver>)invoker.Invoke(null, client));
        }

        void IGrainObserverReferenceService.DeleteObserverReference<TGrainObserver>(ObserverReference<TGrainObserver> reference)
        {
            var invoker = deleteObjectReferenceFactoryMethods.Find(typeof(TGrainObserver));

            if (invoker == null)
                throw ObserverFactoryMethodNotFoundException.Create(typeof(TGrainObserver), "DeleteObjectReference()");

            invoker.Invoke(null, reference.Proxy);
        }

        async Task<ObserverReference<IGrainObserver>> IGrainObserverReferenceService.CreateObserverReference(Type @interface, IGrainObserver client)
        {
            var invoker = createObjectReferenceFactoryMethods.Find(@interface);

            if (invoker == null)
                throw ObserverFactoryMethodNotFoundException.Create(@interface, "CreateObjectReference()");

            var task = (Task) invoker.Invoke(null, client);
            await task;

            return new ObserverReference<IGrainObserver>((IGrainObserver)task.GetPropertyValue("Result"));
        }

        void IGrainObserverReferenceService.DeleteObserverReference(Type @interface, ObserverReference<IGrainObserver> reference)
        {
            var invoker = deleteObjectReferenceFactoryMethods.Find(@interface);

            if (invoker == null)
                throw ObserverFactoryMethodNotFoundException.Create(@interface, "DeleteObjectReference()");

            invoker.Invoke(null, reference.Proxy);
        }

        IEnumerable<Type> IGrainObserverReferenceService.RegisteredGrainObserverTypes()
        {
            return createObjectReferenceFactoryMethods.Keys;
        }

        [Serializable]
        internal class ObserverFactoryMethodNotFoundException : ApplicationException
        {
            const string notFoundMessage = "Can't find factory method {0} for {1}. Factory class was not found during initial scan";
            const string badCastMessage = "Can't find factory method {0} for {1}. Try casting client to actual observer interface {2}";

            public static ObserverFactoryMethodNotFoundException Create(Type observerType, string factoryMethod)
            {
                var actualObserverInterface = FindObserverInterface(observerType);

                if (actualObserverInterface != observerType)
                    return new ObserverFactoryMethodNotFoundException(
                        string.Format(badCastMessage, factoryMethod, observerType, actualObserverInterface));

                return new ObserverFactoryMethodNotFoundException(
                    string.Format(notFoundMessage, factoryMethod, observerType));
            }

            static Type FindObserverInterface(Type observerType)
            {
                var interfaces = observerType.GetInterfaces();

                return interfaces.Single(x => WhichImplementsOnly2Interfaces(x) && 
                                              AndOnlyIGrainObserverAndIAddressableInterfaces(x));
            }

            static bool WhichImplementsOnly2Interfaces(Type x)
            {
                return x.GetInterfaces().Length == 2;
            }

            static bool AndOnlyIGrainObserverAndIAddressableInterfaces(Type x)
            {
                return x.GetInterfaces().ElementAt(0) == typeof(IGrainObserver) &&
                       x.GetInterfaces().ElementAt(1) == typeof(IAddressable);
            }

            ObserverFactoryMethodNotFoundException(string message)
                : base(message)
            {}

            protected ObserverFactoryMethodNotFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }
    }
}
