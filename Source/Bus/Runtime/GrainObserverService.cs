using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Fasterflect;

namespace Orleans.Bus
{
    class ObserverReference<TGrainObserver> where TGrainObserver : IGrainObserver
    {
        public readonly TGrainObserver Proxy;

        public ObserverReference(TGrainObserver proxy)
        {
            Proxy = proxy;
        }
    }

    class GrainObserverService
    {
        public static readonly GrainObserverService Instance = new GrainObserverService().Initialize();

        readonly IDictionary<Type, MethodInvoker> createObjectReferenceFactoryMethods = new Dictionary<Type, MethodInvoker>();
        readonly IDictionary<Type, MethodInvoker> deleteObjectReferenceFactoryMethods = new Dictionary<Type, MethodInvoker>();

        GrainObserverService()
        {}

        GrainObserverService Initialize()
        {
            var bindings = OrleansStaticFactories.WhereProduct(IsGrainObserver);

            foreach (var binding in bindings)
            {
                BindCreateObjectReference(binding);
                BindDeleteObjectReference(binding);
            }

            return this;
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

        public async Task<ObserverReference<TGrainObserver>> Create<TGrainObserver>(TGrainObserver client) where TGrainObserver : IGrainObserver
        {
            var invoker = createObjectReferenceFactoryMethods.Find(typeof(TGrainObserver));

            if (invoker == null)
                throw ObserverFactoryMethodNotFoundException.Create(typeof(TGrainObserver), "CreateObjectReference()");

            return new ObserverReference<TGrainObserver>(await (Task<TGrainObserver>)invoker.Invoke(null, client));
        }

        public void Delete<TGrainObserver>(ObserverReference<TGrainObserver> reference) where TGrainObserver : IGrainObserver
        {
            var invoker = deleteObjectReferenceFactoryMethods.Find(typeof(TGrainObserver));

            if (invoker == null)
                throw ObserverFactoryMethodNotFoundException.Create(typeof(TGrainObserver), "DeleteObjectReference()");

            invoker.Invoke(null, reference.Proxy);
        }

        public async Task<ObserverReference<IGrainObserver>> Create(Type @interface, IGrainObserver client)
        {
            var invoker = createObjectReferenceFactoryMethods.Find(@interface);

            if (invoker == null)
                throw ObserverFactoryMethodNotFoundException.Create(@interface, "CreateObjectReference()");

            var task = (Task) invoker.Invoke(null, client);
            await task;

            return new ObserverReference<IGrainObserver>((IGrainObserver)task.GetPropertyValue("Result"));
        }

        public void Delete(Type @interface, ObserverReference<IGrainObserver> reference)
        {
            var invoker = deleteObjectReferenceFactoryMethods.Find(@interface);

            if (invoker == null)
                throw ObserverFactoryMethodNotFoundException.Create(@interface, "DeleteObjectReference()");

            invoker.Invoke(null, reference.Proxy);
        }

        public IEnumerable<Type> RegisteredGrainObserverTypes()
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
