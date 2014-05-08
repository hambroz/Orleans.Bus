using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Allows to attach and detach to discrete event notifications in reactive facshion (RX).
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
        /// Attaches this observer proxy to receive event notifications of a particular type from the given source 
        /// and in a loosely typed fashion. Useful for higher-order callback functions, when single callback method
        /// could receive events of different types
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <returns>Hot observable</returns>
        Task<IObservable<Notification>> AttachLoose<TEvent>(string source);

        /// <summary>
        /// Detaches this observer proxy from receiving event notifications of a particular type from the given source 
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <returns>Promise</returns>
        Task Detach<TEvent>(string source);
    }

    /// <summary>
    /// Default implementation of <see cref="IReactiveObservableProxy"/>
    /// </summary>
    public class ReactiveObservableProxy : IReactiveObservableProxy, IObserve
    {
        /// <summary>
        /// Factory method to create new instances of <see cref="IReactiveObservableProxy"/>
        /// </summary>
        /// <returns>New instance of <see cref="IReactiveObservableProxy"/></returns>
        public static async Task<IReactiveObservableProxy> Create()
        {
            var observable = new ReactiveObservableProxy();
            
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

        async Task<IObservable<Notification<TEvent>>> IReactiveObservableProxy.Attach<TEvent>(string source)
        {
            var subject = new Subject<Notification<TEvent>>();
            callbacks.Add(typeof(TEvent), (s, o) => 
                subject.OnNext(new Notification<TEvent>(s, (TEvent) o)));

            await SubscriptionManager.Instance.Subscribe<TEvent>(source, proxy);
            return subject;
        }

        async Task<IObservable<Notification>> IReactiveObservableProxy.AttachLoose<TEvent>(string source)
        {
            var subject = new Subject<Notification>();
            callbacks.Add(typeof(TEvent), (s, o) =>
                subject.OnNext(new Notification(s, o)));

            await SubscriptionManager.Instance.Subscribe<TEvent>(source, proxy);
            return subject;
        }

        async Task IReactiveObservableProxy.Detach<TEvent>(string source)
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
}