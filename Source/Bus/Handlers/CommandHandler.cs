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

        public Task Handle(object grain, object command)
        {
            return (Task)handler.Invoke(grain, new[] { command });
        }
    }
}