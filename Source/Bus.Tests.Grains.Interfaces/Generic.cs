using System;
using System.Linq;

namespace Orleans.Bus
{
    public abstract class Command
    {}

    public abstract class Query
    {}    
    
    public abstract class Query<TResult> : Query
    {}

    public abstract class Event
    {}
}
