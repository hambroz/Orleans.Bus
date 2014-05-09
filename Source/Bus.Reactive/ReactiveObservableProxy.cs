using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Allows to attach and detach to discrete event notifications as reactive observable
    /// To delete underlying runtime referece call <see cref="IDisposable.Dispose"/> method
    /// </summary>
    /// <remarks>Instances of this type are not thread safe</remarks>
    public interface IReactiveObservableProxy : IDisposable
    {
        /// <summary>
        /// Attaches this observer proxy to receive event notifications of a particular type from the given source
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <returns>Hot observable</returns>
        Task<IObservable<Notification<TEvent>>> Attach<TEvent>(string source);

        /// <summary>
        /// Detaches this observer proxy from receiving event notifications of a particular type from the given source 
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <returns>Promise</returns>
        Task Detach<TEvent>(string source);
    }

    /// <summary>
    /// Allows to attach and detach to discrete event notifications as reactive observable, in a generic fashion.
    /// To delete underlying runtime referece call <see cref="IDisposable.Dispose"/> method
    /// </summary>
    /// <remarks>
    /// <para>
    /// Useful for higher-order callback functions, when single callback method
    /// could receive events of different types
    /// </para>
    /// <para>Instances of this type are not thread safe</para>
    /// </remarks>
    public interface IGenericReactiveObservableProxy : IDisposable
    {
        /// <summary>
        /// Attaches this observer proxy to receive event notifications of a particular type from the given source 
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <returns>Hot observable</returns>
        Task<IObservable<Notification>> Attach<TEvent>(string source);

        /// <summary>
        /// Detaches this observer proxy from receiving event notifications of a particular type from the given source 
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <returns>Promise</returns>
        Task Detach<TEvent>(string source);
    }

    /// <summary>
    /// Factory for <see cref="IGenericReactiveObservableProxy"/>
    /// </summary>
    public class GenericReactiveObservableProxy : IGenericReactiveObservableProxy, IObserve
    {
        /// <summary>
        /// Creates new <see cref="IGenericReactiveObservableProxy"/>
        /// </summary>
        /// <returns>New instance of <see cref="IGenericReactiveObservableProxy"/></returns>
        public static async Task<IGenericReactiveObservableProxy> Create()
        {
            var observable = new GenericReactiveObservableProxy();
            
            var proxy = await SubscriptionManager.Instance.CreateProxy(observable);
            observable.Initialize(proxy);

            return observable;
        }

        readonly IDictionary<Type, Action<string, object>> callbacks =
             new Dictionary<Type, Action<string, object>>();

        IObserve proxy;

        void Initialize(IObserve proxy)
        {
            this.proxy = proxy;
        }

        void IDisposable.Dispose()
        {        
            SubscriptionManager.Instance.DeleteProxy(proxy);
        }

        async Task<IObservable<Notification>> IGenericReactiveObservableProxy.Attach<TEvent>(string source)
        {
            var subject = new Subject<Notification>();
            callbacks.Add(typeof(TEvent), (s, o) =>
                subject.OnNext(new Notification(s, o)));

            await SubscriptionManager.Instance.Subscribe<TEvent>(source, proxy);
            return subject;
        }

        async Task IGenericReactiveObservableProxy.Detach<TEvent>(string source)
        {
            callbacks.Remove(typeof(TEvent));
            await SubscriptionManager.Instance.Unsubscribe<TEvent>(source, proxy);
        }

        void IObserve.On(string source, object e)
        {
            var callback = callbacks[e.GetType()];
            callback(source, e);
        }
    }

    /// <summary>
    /// Factory for <see cref="IReactiveObservableProxy"/>
    /// </summary>
    class ReactiveObservableProxy : IReactiveObservableProxy
    {
        /// <summary>
        /// Creates new <see cref="IReactiveObservableProxy"/>
        /// </summary>
        /// <returns>New instance of <see cref="IReactiveObservableProxy"/></returns>
        public static async Task<IReactiveObservableProxy> Create()
        {
            return new ReactiveObservableProxy(await GenericReactiveObservableProxy.Create());
        }

        readonly IGenericReactiveObservableProxy generic;

        ReactiveObservableProxy(IGenericReactiveObservableProxy generic)
        {
            this.generic = generic;
        }

        void IDisposable.Dispose()
        {
            generic.Dispose();
        }

        async Task<IObservable<Notification<TEvent>>> IReactiveObservableProxy.Attach<TEvent>(string source)
        {
            return new Observable<TEvent>(await generic.Attach<TEvent>(source));
        }

        Task IReactiveObservableProxy.Detach<TEvent>(string source)
        {
            return generic.Detach<TEvent>(source);
        }

        class Observable<TEvent> : IObservable<Notification<TEvent>>
        {
            readonly IObservable<Notification> generic;

            public Observable(IObservable<Notification> generic)
            {
                this.generic = generic;
            }

            public IDisposable Subscribe(IObserver<Notification<TEvent>> observer)
            {
                return generic.Subscribe(new Observer<TEvent>(observer));
            }
        }

        class Observer<TEvent> : IObserver<Notification>
        {
            readonly IObserver<Notification<TEvent>> observer;

            public Observer(IObserver<Notification<TEvent>> observer)
            {
                this.observer = observer;
            }

            public void OnNext(Notification value)
            {
                observer.OnNext(new Notification<TEvent>(value.Source, (TEvent) value.Message));
            }

            public void OnError(Exception error)
            {
                observer.OnError(error);
            }

            public void OnCompleted()
            {
                observer.OnCompleted();
            }
        }
    }
}