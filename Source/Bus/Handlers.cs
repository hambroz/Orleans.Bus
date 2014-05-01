using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleans.Bus
{
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

        readonly MethodInfo handler;

        public CommandHandler(Type grain, MethodInfo handler)
        {
            Grain = grain;
            Command = handler.GetParameters()[0].ParameterType;

            this.handler = handler;
        }

        public Task Dispatch(object grain, object command)
        {
            return (Task)handler.Invoke(grain, new[] { command });
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

        public static QueryHandler Create(Type grain, MethodInfo handler)
        {
            var queryType = handler.GetParameters()[0].ParameterType;
            var resultType = handler.ReturnType.GetGenericArguments()[0];

            var queryHandler = typeof(QueryHandler<>).MakeGenericType(resultType);
            return (QueryHandler)Activator.CreateInstance(queryHandler, new object[] { grain, queryType, handler });
        }
    }

    class QueryHandler<TResult> : QueryHandler
    {
        readonly MethodInfo handler;

        public QueryHandler(Type grain, Type query, MethodInfo handler)
            : base(grain, query)
        {
            this.handler = handler;
        }

        public Task<TResult> Dispatch(object grain, object query)
        {
            return (Task<TResult>)handler.Invoke(grain, new[] { query });
        }
    }
}
