using System;
using System.Linq;
using System.Linq.Expressions;
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

        protected CommandHandler(Type grain, Type command)
        {
            Grain = grain;
            Command = command;
        }

        public static CommandHandler Create(Type grain, MethodInfo method)
        {
            var command = method.GetParameters()[0].ParameterType;

            var handler = typeof(CommandHandler<>).MakeGenericType(command);

            return (CommandHandler)Activator.CreateInstance(handler, new object[] { grain, method });
        }
    }

    class CommandHandler<TCommand> : CommandHandler
    {
        readonly Func<object, TCommand, Task> invoker;

        public CommandHandler(Type grain, MethodInfo method)
            : base(grain, typeof(TCommand))
        {
            invoker = Bind(grain, method);
        }

        public Task Handle(object grain, TCommand command)
        {
            return invoker(grain, command);
        }

        static Func<object, TCommand, Task> Bind(Type grain, MethodInfo method)
        {
            var target = Expression.Parameter(typeof(object), "target");
            var command = Expression.Parameter(typeof(TCommand), "command");

            var call = Expression.Call(Expression.Convert(target, grain), method, new Expression[]{command});
            var lambda = Expression.Lambda<Func<object, TCommand, Task>>(call, target, command);

            return lambda.Compile();
        }
    }
}