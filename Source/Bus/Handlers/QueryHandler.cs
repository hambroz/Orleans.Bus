using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Orleans.Bus
{
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