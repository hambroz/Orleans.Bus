using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Allows to attach and detach to discrete event notifications
    /// To delete underlying runtime referece call <see cref="IDisposable.Dispose"/>
    /// </summary>
    /// <remarks> Instances of this type are not thread safe </remarks>
    public interface IObservableProxy: IDisposable
    {
        /// <summary>
        /// Attaches this observer proxy to receive event notifications of a particular type from the given source
        /// and deliver them to the given <paramref name="callback"/>
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <param name="callback">Notification callback</param>
        /// <returns>Promise</returns>
        Task Attach<TEvent>(string source, Action<string, TEvent> callback);

        /// <summary>
        /// Detaches this observer proxy from receiving event notifications of a particular type from the given source 
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <returns>Promise</returns>
        Task Detach<TEvent>(string source);
    }

    /// <summary>
    /// Allows to attach and detach to discrete event notifications, in a generic fashion
    /// To delete underlying runtime referece call <see cref="IDisposable.Dispose"/> method
    /// </summary>
    /// <remarks>
    /// <para>
    /// Useful for higher-order callback functions, when single callback method
    /// could receive events of different types
    /// </para>
    /// <para>Instances of this type are not thread safe</para>
    /// </remarks>
    public interface IGenericObservableProxy : IDisposable
    {
        /// <summary>
        /// Attaches this observer proxy to receive event notifications of a particular type from the given source
        /// and deliver them to the given loosely typed <paramref name="callback"/>.
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <param name="callback">Notification callback</param>
        /// <returns>Promise</returns>
        Task Attach<TEvent>(string source, Action<string, object> callback);

        /// <summary>
        /// Detaches this observer proxy from receiving event notifications of a particular type from the given source 
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="source">Id of the source grain</param>
        /// <returns>Promise</returns>
        Task Detach<TEvent>(string source);
    }

    /// <summary>
    /// Factory for <see cref="IGenericObservableProxy"/>
    /// </summary>
    public class GenericObservableProxy : IGenericObservableProxy, IObserve
    {
        /// <summary>
        /// Creates new <see cref="IGenericObservableProxy"/>
        /// </summary>
        /// <returns>New instance of <see cref="IGenericObservableProxy"/></returns>
        public static async Task<IGenericObservableProxy> Create()
        {
            var observable = new GenericObservableProxy();
            
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

        async Task IGenericObservableProxy.Attach<TEvent>(string source, Action<string, object> callback)
        {
            callbacks.Add(typeof(TEvent), callback);
            await SubscriptionManager.Instance.Subscribe<TEvent>(source, proxy);
        }

        async Task IGenericObservableProxy.Detach<TEvent>(string source)
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
    /// Factory for <see cref="IGenericObservableProxy"/>
    /// </summary>
    public class ObservableProxy : IObservableProxy
    {
        /// <summary>
        /// Creates new <see cref="IObservableProxy"/>
        /// </summary>
        /// <returns>New instance of <see cref="IObservableProxy"/></returns>
        public static async Task<IObservableProxy> Create()
        {
            return new ObservableProxy(await GenericObservableProxy.Create());
        }

        readonly IGenericObservableProxy proxy;

        ObservableProxy(IGenericObservableProxy proxy)
        {
            this.proxy = proxy;
        }

        void IDisposable.Dispose()
        {
            proxy.Dispose();
        }

        Task IObservableProxy.Attach<TEvent>(string source, Action<string, TEvent> callback)
        {
            return proxy.Attach<TEvent>(source, (s, e) => callback(s, (TEvent) e));
        }

        Task IObservableProxy.Detach<TEvent>(string source)
        {
            return proxy.Detach<TEvent>(source);
        }
    }
}