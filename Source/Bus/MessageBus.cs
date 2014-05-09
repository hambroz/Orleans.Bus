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

        readonly HashSet<CommandHandler> commandHandlers = new HashSet<CommandHandler>();
        readonly HashSet<QueryHandler> queryHandlers = new HashSet<QueryHandler>();

        readonly Dictionary<Type, CommandHandler> commands =
             new Dictionary<Type, CommandHandler>();

        readonly Dictionary<Type, QueryHandler> queries =
             new Dictionary<Type, QueryHandler>();

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
            RegisterHandlers(grain);
            RegisterCommands(grain);
            RegisterQueries(grain);
        }

        void RegisterHandlers(Type grain)
        {
            var handlers = FindHandlers(grain);

            foreach (var handler in handlers)
            {
                if (CommandHandler.Satisfies(handler))
                {
                    if (!commandHandlers.Add(CommandHandler.Create(grain, handler)))
                        throw new DuplicateHandlerMethodException("command", grain);

                    continue;
                }

                if (QueryHandler.Satisfies(handler))
                {
                    if (!queryHandlers.Add(QueryHandler.Create(grain, handler)))
                        throw new DuplicateHandlerMethodException("command", grain);

                    continue;
                }

                throw new NotSupportedException("Incorrect handler signature: " + handler);
            }
        }

        static IEnumerable<MethodInfo> FindHandlers(Type grain)
        {
            var result = new HashSet<MethodInfo>();
            FindHandlers(grain, result);
            return result;            
        }

        static void FindHandlers(Type grain, ISet<MethodInfo> result)
        {
            var methods = grain.GetMethods().Where(
                 method => method.HasAttribute<HandlerAttribute>());
            
            foreach (var method in methods)
                result.Add(method);

            foreach (var @interface in grain.GetInterfaces())
            {
                if (typeof(IGrain).IsAssignableFrom(@interface))
                    FindHandlers(@interface, result);
            }
        }

        void RegisterCommands(Type grain)
        {
            foreach (var attribute in grain.Attributes<HandlesAttribute>())
            {
                Debug.Assert(attribute.Command != null);

                var registered = commands.Find(attribute.Command);
                if (registered != null)
                    throw new DuplicateHandlesAttributeException(attribute.Command, registered.Grain, grain);

                var dispatcher = commandHandlers.SingleOrDefault(x => x.Grain == grain);
                if (dispatcher == null)
                    throw new HandlerNotRegisteredException(grain);

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
                    throw new DuplicateHandlesAttributeException(attribute.Query, registered.Grain, grain);

                var dispatcher = queryHandlers.SingleOrDefault(x => x.Grain == grain);
                if (dispatcher == null)
                    throw new HandlerNotRegisteredException(grain);

                queries.Add(attribute.Query, dispatcher);
            }
        }

        Task IMessageBus.Send(string destination, object command)
        {
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination id is null or empty", "destination");

            if (command == null)
                throw new ArgumentNullException("command");

            var handler = commands.Find(command.GetType());
            if (handler == null)
                throw new HandlerNotFoundException(command.GetType());

            var grain = factory.GetReference(handler.Grain, destination);
            return handler.Handle(grain, command).UnwrapExceptions();
        }

        async Task<TResult> IMessageBus.Query<TResult>(string destination, object query)
        {
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentException("Destination id is null or empty", "destination");

            if (query == null)
                throw new ArgumentNullException("query");

            var handler = queries.Find(query.GetType());
            if (handler == null)
                throw new HandlerNotFoundException(query.GetType());

            var reference = factory.GetReference(handler.Grain, destination);
            return (TResult)(await handler.Handle(reference, query).UnwrapExceptions());
        }

        [Serializable]
        internal class DuplicateHandlesAttributeException : ApplicationException
        {
            const string description = "The handler for {0} is already registered by {1}. Duplicate grain: {2}";

            public DuplicateHandlesAttributeException(Type message, Type registeredBy, Type duplicate)
                : base(string.Format(description, message, registeredBy, duplicate))
            {}

            protected DuplicateHandlesAttributeException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }        
        
        [Serializable]
        internal class DuplicateHandlerMethodException : ApplicationException
        {
            const string description = "Duplicate {0} handler method is specified by {1}";

            public DuplicateHandlerMethodException(string kind, Type grain)
                : base(string.Format(description, kind, grain))
            {}

            protected DuplicateHandlerMethodException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        [Serializable]
        internal class HandlerNotRegisteredException : ApplicationException
        {
            const string description = "{0} specifies [Handles/Answers] attributes but no handler could be found.\r\nCheck that you've put [Handler] attribute on a method";

            internal HandlerNotRegisteredException(Type grain)
                : base(string.Format(description, grain))
            {}

            protected HandlerNotRegisteredException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        [Serializable]
        internal class HandlerNotFoundException : ApplicationException
        {
            const string description = "Can't find handler for '{0}'.\r\nCheck that you've marked grain with [ExtendedPrimaryKey] and corresponding [Handles/Answers] attributes";

            internal HandlerNotFoundException(Type message)
                : base(string.Format(description, message))
            {}

            protected HandlerNotFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        abstract class Handler
        {
            public readonly Type Grain;

            protected Handler(Type grain)
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

        class CommandHandler : Handler
        {
            public static bool Satisfies(MethodInfo method)
            {
                return !method.IsGenericMethod &&
                        method.GetParameters().Length == 1 &&
                        method.GetParameters()[0].ParameterType == typeof(object) &&
                        method.ReturnType == typeof(Task);
            }

            public static CommandHandler Create(Type grain, MethodInfo method)
            {
                return new CommandHandler(grain, method);
            }

            public readonly Func<object, object, Task> Handle;

            CommandHandler(Type grain, MethodInfo method) : base(grain)
            {
                var target = Expression.Parameter(typeof(object), "target");
                var argument = Expression.Parameter(typeof(object), "command");

                var call = Expression.Call(Expression.Convert(target, Grain), method, new Expression[] { argument });
                var lambda = Expression.Lambda<Func<object, object, Task>>(call, target, argument);

                Handle = lambda.Compile();
            }
        }

        class QueryHandler : Handler
        {
            public static bool Satisfies(MethodInfo method)
            {
                return !method.IsGenericMethod &&
                        method.GetParameters().Length == 1 &&
                        method.GetParameters()[0].ParameterType == typeof(object) &&
                        method.ReturnType == typeof(Task<object>);
            }

            public readonly Func<object, object, Task<object>> Handle;

            public static QueryHandler Create(Type grain, MethodInfo method)
            {
                return new QueryHandler(grain, method);
            }

            QueryHandler(Type grain, MethodInfo method) : base(grain)
            {
                var target = Expression.Parameter(typeof(object), "target");
                var argument = Expression.Parameter(typeof(object), "query");

                var call = Expression.Call(Expression.Convert(target, Grain), method, new Expression[] { argument });
                var lambda = Expression.Lambda<Func<object, object, Task<object>>>(call, target, argument);

                Handle = lambda.Compile();
            }
        }
    }
}