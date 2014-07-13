![Orleans.Bus](Logo.Wide.png)

# Message bus for Orleans

## Main features

+ Superior developer-friendly API
+ Support for POCO actors
+ Command/Query/Event semantics enforcement
+ Selective subscriptions to discrete events
- Reactive Extensions (RX) support
+ Support for higher-order functions (handlers)
+ Convinient exception handling
+ Unit testing simplicity

### Prerequisites
- [Orleans SDK](https://orleans.codeplex.com/wikipage?title=Orleans%20Setup%20for%20Developers&referringTitle=Home "Link to Orleans SDK installation page")
- Familiarity with Orleans

### How to install

To install Orleans.Bus via NuGet, run this command in NuGet package manager console:

	PM> Install-Package Orleans.Bus

Then follow instructions in [README](https://github.com/yevhen/Orleans.Bus/blob/master/Build/Readme.txt) file.

## Reference

```cs
[Immutable, Serializable]
public class SetFoo                          // define Command messages
{
    public string Text;
}

[Immutable, Serializable]
public class GetFoo                         // define Query messages
{}

[Serializable]
public class FooChanged                     // define Event messages
{
    public string Text;
}

[Handles(typeof(DoFoo))]                    // specify Command handler
[Answers(typeof(GetFoo))]                   // specify Query handler
[Notifies(typeof(TextPublished))]           // specify Event notifier
[ExtendedPrimaryKey]                        // this is required attribute
public interface ITestGrain : IPocoGrain
{}

// inherit you grain from PocoGrain
// it will serve as a proxy to actual Poco
public class TestGrain : PocoGrain<Poco>, ITestGrain
{
    public TestGrain()
    {
        // define activator function
        Activate = grain =>                         
        {
            var poco = new Poco(Id(), Notify);      // create you grain, pass anything
            return poco.Activate();                 // you need to ctor and Activate it
        };

        // define cmd handling function
        Handle = (poco, m) => poco.Handle((dynamic)m);            
        
        // define qry handling function
        Answer = async (poco, m) => await poco.Answer((dynamic)m);
    }
}

// actual implementation which is free of
// any Orleans' infrastructure dependencies
public class Poco
{
    readonly string id;
    readonly Action<Event> notify;

    string fooText = "";
    
    public Poco(string id, Action<Event> notify)
    {
        this.id = id;
        this.notify = notify;
    }

    public Task<Poco> Activate()
    {
        return Task.FromResult(this);
    }

    public Task Handle(SetFoo cmd)
    {
        fooText = cmd.Text;
        notify(new FooChanged{Text = cmd.Text});
        return TaskDone.Done;
    }

    public Task<string> Answer(GetFoo query)
    {
        return Task.FromResult(fooText + "-" + id);
    }
}

// from client code, reference isntance of Bus
var bus = MessageBus.Instance;

// send command to grain
await bus.Send("id-123", new SetFoo{Text="foo"});

// send query to grain
var result = await bus.Query<string>("id-123", new GetFoo());
Console.WriteLine(result);

// subscribe to event notifications with strict typing
using (var proxy = await ObservableProxy.Create())
{
    await proxy.Attach<FooChanged>("id-123", (sender, @event) =>
    {
        Console.WriteLine("Received FooChanged from {0}: Text is {1},
                          sender, event.Text);
    });
}

// subscribe to event notifications in a generic fashion
using (var proxy = await GenericObservableProxy.Create())
{
    await proxy.Attach<FooChanged>("id-123", (sender, @event) =>
    {
        Console.WriteLine("Received {0} from {1}, sender, event.GetType());
    });
}

// RX subscribe to event notifications with strict typing
using (var proxy = await ReactiveObservableProxy.Create())
{
    var observable = await proxy.Attach<FooChanged>("id-123");
    observable.Subscribe(e => 
    {
        Console.WriteLine("Received FooChanged from {0}: Text is {1},
                          e.Source, e.Message.Text);    
    };
}

// RX subscribe to event notifications in a generic fashion
using (var proxy = await GenericReactiveObservableProxy.Create())
{
    var observable = await proxy.Attach<FooChanged>("id-123");
    observable.Subscribe(e => 
    {
        Console.WriteLine("Received {0} from {1}, e.Source, e.Message.GetType());  
    };
}
    
```

## Contributing

Pull requests are welcome!

## License

Apache 2 License
