using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Fasterflect;
using Orleans.IoC;

namespace Orleans.Bus
{
    public interface IBus
    {
        Task Send(long id, object command);
        Task<TResult> Query<TResult>(long id, object query);
    }

    public class Bus : IBus
    {
        public static IBus Instance = new Bus(GrainRuntime.Instance).Initialize();

        readonly Dictionary<Type, CommandHandler> commands = 
             new Dictionary<Type, CommandHandler>();        
        
        readonly Dictionary<Type, QueryHandler> queries =
             new Dictionary<Type, QueryHandler>();
        
        readonly IGrainRuntime runtime;

        public Bus(IGrainRuntime runtime)
        {
            this.runtime = runtime;
        }

        IBus Initialize()
        {
            foreach (var grain in runtime.RegisteredGrainTypes())
            {
                var handlers = Find(grain);
                Register(grain, handlers);
            }

            return this;
        }

        static IEnumerable<MethodInfo> Find(Type grain)
        {
            return grain.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                        .Where(method => method.HasAttribute<HandlerAttribute>());
        }

        void Register(Type grain, IEnumerable<MethodInfo> handlers)
        {
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

        void RegisterCommandHandler(Type grain, MethodInfo handler)
        {
            var commandHandler = new CommandHandler(grain, handler);
            commands.Add(commandHandler.Command, commandHandler);
        }

        void RegisterQueryHandler(Type grain, MethodInfo handler)
        {
            var queryHandler = QueryHandler.Create(grain, handler);
            queries.Add(queryHandler.Query, queryHandler);
        }

        public Task Send(long id, object command)
        {
            var handler = commands[command.GetType()];

            var grain = runtime.Reference(handler.Grain, id);

            return handler.Dispatch(grain, command);
        }

        public Task<TResult> Query<TResult>(long id, object query)
        {
            var handler = (QueryHandler<TResult>) queries[query.GetType()];

            var grain = runtime.Reference(handler.Grain, id);

            return handler.Dispatch(grain, query);
        }
    }
}