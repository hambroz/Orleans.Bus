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

        readonly Func<object, object, Task> invoker;

        CommandHandler(Type grain, Type command, MethodInfo method)
        {
            Grain   = grain;
            Command = command;
            invoker = Bind(method);
        }

        Func<object, object, Task> Bind(MethodInfo method)
        {
            var target = Expression.Parameter(typeof(object), "target");
            var argument = Expression.Parameter(typeof(object), "command");

            var typeCast = Expression.Convert(target, Grain);
            var argumentCast = Expression.Convert(argument, Command);
            
            var call = Expression.Call(typeCast, method, new Expression[] {argumentCast});
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