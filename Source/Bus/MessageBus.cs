using System;
using System.Collections.Generic;
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

        readonly Dictionary<Type, CommandHandler> commands = 
             new Dictionary<Type, CommandHandler>();        
        
        readonly Dictionary<Type, QueryHandler> queries =
             new Dictionary<Type, QueryHandler>();

        readonly DynamicGrainFactory factory;

        MessageBus(DynamicGrainFactory factory)
        {
            this.factory = factory;
        }

        MessageBus Initialize()
        {
            foreach (var grain in factory.RegisteredGrainTypes())
                Register(grain);

            return this;
        }

        void Register(Type grain)
        {
            var handlers = grain.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Where(method => method.HasAttribute<HandlerAttribute>());

            foreach (var handler in handlers)
            {
                if (CommandHandler.Satisfies(handler))
                {
                    RegisterCommandHandler(grain, handler);
                    continue;
                }

                if (QueryHandler.Satisfies(handler))
                {
                    RegisterQueryHandler(grain, handler);
                    continue;
                }

                throw new NotSupportedException("Unsupported handler signature: " + handler);
            }
        }

        void RegisterCommandHandler(Type grain, MethodInfo method)
        {
            var handler = CommandHandler.Create(grain, method);
            commands.Add(handler.Command, handler);
        }

        void RegisterQueryHandler(Type grain, MethodInfo method)
        {
            var handler = QueryHandler.Create(grain, method);
            queries.Add(handler.Query, handler);
        }

        Task IMessageBus.Send(string destination, object command)
        {
            var handler = commands[command.GetType()];

            var grain = factory.GetReference(handler.Grain, destination);

            return handler.Handle(grain, command);
        }

        Task<TResult> IMessageBus.Query<TResult>(string destination, object query)
        {
            var handler = (QueryHandler<TResult>) queries[query.GetType()];

            var reference = factory.GetReference(handler.Grain, destination);

            return handler.Handle(reference, query);
        }

        [Serializable]
        internal class HandlerNotFoundException : ApplicationException
        {
            const string message = "Can't find handler for '{0}'.\r\nCheck that you've put [ExtendedPrimaryKey] on a grain and [Handler] on method";

            internal HandlerNotFoundException(Type messageType)
                : base(string.Format(message, messageType))
            {}

            protected HandlerNotFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {}
        }

        class CommandHandler
        {
            public static bool Satisfies(MethodInfo method)
            {
                return !method.IsGenericMethod &&
                        method.GetParameters().Length == 1 &&
                        method.ReturnType == typeof(Task);
            }

            public readonly Type Grain;
            public readonly Type Command;

            readonly Func<object, object, Task> invoker;

            CommandHandler(Type grain, Type command, MethodInfo method)
            {
                Grain = grain;
                Command = command;
                invoker = Bind(method);
            }

            Func<object, object, Task> Bind(MethodInfo method)
            {
                var target = Expression.Parameter(typeof(object), "target");
                var argument = Expression.Parameter(typeof(object), "command");

                var typeCast = Expression.Convert(target, Grain);
                var argumentCast = Expression.Convert(argument, Command);

                var call = Expression.Call(typeCast, method, new Expression[] { argumentCast });
                var lambda = Expression.Lambda<Func<object, object, Task>>(call, target, argument);

                return lambda.Compile();
            }

            public Task Handle(object grain, object command)
            {
                return invoker(grain, command);
            }

            public static CommandHandler Create(Type grain, MethodInfo method)
            {
                var command = method.GetParameters()[0].ParameterType;
                return new CommandHandler(grain, command, method);
            }
        }
    }

    abstract class QueryHandler
    {
        public static bool Satisfies(MethodInfo method)
        {
            return !method.IsGenericMethod &&
                   method.GetParameters().Length == 1 &&
                   typeof(Task).IsAssignableFrom(method.ReturnType) &&
                   method.ReturnType.IsConstructedGenericType;
        }

        public readonly Type Grain;
        public readonly Type Query;

        protected QueryHandler(Type grain, Type query)
        {
            Grain = grain;
            Query = query;
        }

        public static QueryHandler Create(Type grain, MethodInfo method)
        {
            var query = method.GetParameters()[0].ParameterType;
            var result = method.ReturnType.GetGenericArguments()[0];

            var handler = typeof(QueryHandler<>).MakeGenericType(result);
            return (QueryHandler)Activator.CreateInstance(handler, new object[] { grain, query, method });
        }
    }

    class QueryHandler<TResult> : QueryHandler
    {
        readonly Func<object, object, Task<TResult>> invoker;

        public QueryHandler(Type grain, Type query, MethodInfo method)
            : base(grain, query)
        {
            invoker = Bind(method);
        }

        public Task<TResult> Handle(object grain, object query)
        {
            return invoker(grain, query);
        }

        Func<object, object, Task<TResult>> Bind(MethodInfo method)
        {
            var target = Expression.Parameter(typeof(object), "target");
            var argument = Expression.Parameter(typeof(object), "query");

            var typeCast = Expression.Convert(target, Grain);
            var argumentCast = Expression.Convert(argument, Query);

            var call = Expression.Call(typeCast, method, new Expression[] { argumentCast });
            var lambda = Expression.Lambda<Func<object, object, Task<TResult>>>(call, target, argument);

            return lambda.Compile();
        }
    }
}