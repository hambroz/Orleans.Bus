using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Orleans.Bus
{
    /// <summary>
    /// Central communication hub for message exchanges
    /// between grains and between clients and grains.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Sends command message to the specified grain
        /// </summary>
        /// <param name="destination">Id of the destination grain</param>
        /// <param name="command">Command message to send</param>
        /// <returns>Promise</returns>
        Task Send(string destination, object command);

        /// <summary>
        /// Sends query message to the specified grain
        /// </summary>
        /// <typeparam name="TResult">Type of the result</typeparam>
        /// <param name="destination">Id of the destination grain</param>
        /// <param name="query">Query message to send</param>
        /// <returns>Promise</returns>
        Task<TResult> Query<TResult>(string destination, object query);
    }

    /// <summary>
    /// Default implementation of <see cref="IMessageBus"/>
    /// </summary>
    public class MessageBus : IMessageBus
    {
        /// <summary>
        /// Globally available default instance of <see cref="IMessageBus"/>
        /// </summary>
        public static readonly IMessageBus Instance = 
           new MessageBus(DynamicGrainFactory.Instance).Initialize();

        readonly HashSet<CommandDispatcher> commandDispatchers = new HashSet<CommandDispatcher>();
        readonly HashSet<QueryDispatcher> queryDispatchers = new HashSet<QueryDispatcher>();

        readonly Dictionary<Type, CommandDispatcher> commands =
             new Dictionary<Type, CommandDispatcher>();

        readonly Dictionary<Type, QueryDispatcher> queries =
             new Dictionary<Type, QueryDispatcher>();

        readonly DynamicGrainFactory factory;

        internal MessageBus(DynamicGrainFactory factory)
        {
            this.factory = factory;
        }

        MessageBus Initialize()
        {
            Register(factory.RegisteredGrainTypes());
            return this;
        }

        internal void Register(IEnumerable<Type> grains)
        {
            foreach (var grain in grains)
                Register(grain);
        }

        void Register(Type grain)
        {
            RegisterDispatchers(grain);
            RegisterCommands(grain);
            RegisterQueries(grain);
        }

        void RegisterDispatchers(Type grain)
        {
            var methods = grain.GetPublicInstanceMethods()
                               .Where(method => method.HasAttribute<DispatcherAttribute>());

            foreach (var method in methods)
            {
                if (CommandDispatcher.Satisfies(method))
                {
                    if (!commandDispatchers.Add(CommandDispatcher.Create(grain, method)))
                        throw new DuplicateDispatcherException("command", grain);

                    continue;
                }

                if (QueryDispatcher.Satisfies(method))
                {
                    if (!queryDispatchers.Add(QueryDispatcher.Create(grain, method)))
                        throw new DuplicateDispatcherException("command", grain);

                    continue;
                }

                throw new NotSupportedException("Incorrect dispatcher signature: " + method);
            }
        }

        void RegisterCommands(Type grain)
        {
            foreach (var attribute in grain.Attributes<HandlesAttribute>())
            {
                Debug.Assert(attribute.Command != null);

                var registered = commands.Find(attribute.Command);
                if (registered != null)
                    throw new DuplicateHandlerException(attribute.Command, registered.Grain, grain);

                var dispatcher = commandDispatchers.SingleOrDefault(x => x.Grain == grain);
                if (dispatcher == null)
                    throw new DispatcherNotRegisteredException(grain);

                commands.Add(attribute.Command, dispatcher);
            }
        }

        void RegisterQueries(Type grain)
        {
            foreach (var attribute in grain.Attributes<AnswersAttribute>())
            {
                Debug.Assert(attribute.Query != null);

                var registered = queries.Find(attribute.Query);
                if (registered != null)
                    throw new DuplicateHandlerException(attribute.Query, registered.Grain, grain);

                var dispatcher = queryDispatchers.SingleOrDefault(x => x.Grain == grain);
                if (dispatcher == null)
                    throw new DispatcherNotRegisteredException(grain);

                queries.Add(attribute.Query, dispatcher);
            }
        }

        Task IMessageBus.Send(string destination, object command)
        {
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination id is null or empty", "destination");

            if (command == null)
                throw new ArgumentNullException("command");

            var dispatcher = commands.Find(command.GetType());
            if (dispatcher == null)
                throw new DispatcherNotFoundException(command.GetType());

            var grain = factory.GetReference(dispatcher.Grain, destination);
            return dispatcher.Dispatch(grain, command);
        }

        async Task<TResult> IMessageBus.Query<TResult>(string destination, object query)
        {
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination id is null or empty", "destination");

            if (query == null)
                throw new ArgumentNullException("query");

            var dispatcher = queries.Find(query.GetType());
            if (dispatcher == null)
                throw new DispatcherNotFoundException(query.GetType());

            var reference = factory.GetReference(dispatcher.Grain, destination);
            return (TResult)(await dispatcher.Dispatch(reference, query));
        }

        [Serializable]
        internal class DuplicateHandlerException : ApplicationException
        {
            const string description = "The handler for {0} is already registered by {1}. Duplicate grain: {2}";

            public DuplicateHandlerException(Type message, Type registeredBy, Type duplicate)
                : base(string.Format(description, message, registeredBy, duplicate))
            {}

            protected DuplicateHandlerException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }        
        
        [Serializable]
        internal class DuplicateDispatcherException : ApplicationException
        {
            const string description = "Duplicate {0} dispatcher is specified by {1}";

            public DuplicateDispatcherException(string kind, Type grain)
                : base(string.Format(description, kind, grain))
            {}

            protected DuplicateDispatcherException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        [Serializable]
        internal class DispatcherNotRegisteredException : ApplicationException
        {
            const string description = "{0} specifies [Handles/Answers] attributes but no dispatcher could be found.\r\nCheck that you've put [Dispatcher] attribute on a method";

            internal DispatcherNotRegisteredException(Type grain)
                : base(string.Format(description, grain))
            {}

            protected DispatcherNotRegisteredException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        [Serializable]
        internal class DispatcherNotFoundException : ApplicationException
        {
            const string description = "Can't find dispatcher for '{0}'.\r\nCheck that you've marked grain with [ExtendedPrimaryKey] and corresponding [Handles/Answers] attributes";

            internal DispatcherNotFoundException(Type message)
                : base(string.Format(description, message))
            {}

            protected DispatcherNotFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        abstract class Dispatcher
        {
            public readonly Type Grain;

            protected Dispatcher(Type grain)
            {
                Grain = grain;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                
                if (ReferenceEquals(this, obj))
                    return true;

                return obj.GetType() == GetType() && Equals(obj);
            }

            public override int GetHashCode()
            {
                return Grain.GetHashCode();
            }
        }

        class CommandDispatcher : Dispatcher
        {
            public static bool Satisfies(MethodInfo method)
            {
                return !method.IsGenericMethod &&
                        method.GetParameters().Length == 1 &&
                        method.GetParameters()[0].ParameterType == typeof(object) &&
                        method.ReturnType == typeof(Task);
            }

            public static CommandDispatcher Create(Type grain, MethodInfo method)
            {
                return new CommandDispatcher(grain, method);
            }

            readonly Func<object, object, Task> dispatch;

            CommandDispatcher(Type grain, MethodInfo method) : base(grain)
            {
                var target = Expression.Parameter(typeof(object), "target");
                var argument = Expression.Parameter(typeof(object), "command");

                var call = Expression.Call(Expression.Convert(target, Grain), method, new Expression[] { argument });
                var lambda = Expression.Lambda<Func<object, object, Task>>(call, target, argument);

                dispatch = lambda.Compile();
            }

            public Task Dispatch(object grain, object command)
            {
                return dispatch(grain, command);
            }
        }

        class QueryDispatcher : Dispatcher
        {
            public static bool Satisfies(MethodInfo method)
            {
                return !method.IsGenericMethod &&
                        method.GetParameters().Length == 1 &&
                        method.GetParameters()[0].ParameterType == typeof(object) &&
                        method.ReturnType == typeof(Task<object>);
            }

            readonly Func<object, object, Task<object>> dispatch;

            public static QueryDispatcher Create(Type grain, MethodInfo method)
            {
                return new QueryDispatcher(grain, method);
            }

            QueryDispatcher(Type grain, MethodInfo method) : base(grain)
            {
                var target = Expression.Parameter(typeof(object), "target");
                var argument = Expression.Parameter(typeof(object), "query");

                var call = Expression.Call(Expression.Convert(target, Grain), method, new Expression[] { argument });
                var lambda = Expression.Lambda<Func<object, object, Task<object>>>(call, target, argument);

                dispatch = lambda.Compile();
            }

            public Task<object> Dispatch(object grain, object query)
            {
                return dispatch(grain, query);
            }
        }
    }
}