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

            var handler = typeof(QueryHandler<,>).MakeGenericType(query, result);
            return (QueryHandler)Activator.CreateInstance(handler, new object[] { grain, method });
        }
    }

    class QueryHandler<TQuery, TResult> : QueryHandler
    {
        readonly Func<object, TQuery, Task<TResult>> invoker;

        public QueryHandler(Type grain, MethodInfo method)
            : base(grain, typeof(TQuery))
        {
            invoker = Bind(grain, method);
        }

        public Task<TResult> Handle(object grain, TQuery query)
        {
            return invoker(grain, query);
        }

        static Func<object, TQuery, Task<TResult>> Bind(Type grain, MethodInfo method)
        {
            var target = Expression.Parameter(typeof(object), "target");
            var query = Expression.Parameter(typeof(TQuery), "query");

            var call = Expression.Call(Expression.Convert(target, grain), method, new Expression[] { query });
            var lambda = Expression.Lambda<Func<object, TQuery, Task<TResult>>>(call, target, query);

            return lambda.Compile();
        }
    }
}