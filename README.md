# MediatR Performance Benchmarks

A repository containing benchmarks and findings as mentioned in my [reddit post](https://www.reddit.com/r/csharp/comments/rxxg5m/mediatr_performance_benchmarks/).

The goal of these benchmarks is to see the potential overhead that is paid when 
using the MediatR package. Specifically, I am interested in the memory allocations.

## Benchmarks

### Setup, Command & Handler
First the setup for the benchmarks. Here is am using MediatR 10.0.0 and .NET 6.
```c#
private IMediator _mediator;
private ExampleCommandHandler _handler;

[GlobalSetup]
public void Setup()
{
    var services = new ServiceCollection();
    services.AddMediatR(typeof(Benchy));

    var provider = services.BuildServiceProvider();
    _mediator = provider.GetRequiredService<IMediator>();
    _handler = new ExampleCommandHandler();
}
```

The command and handler are just simple as can be, not really doing anything.
```c#
public class ExampleCommand : IRequest
{
    public ExampleCommand(string arg1, int arg2)
    {
        Arg1 = arg1;
        Arg2 = arg2;
    }
    
    public string Arg1 { get; }
    
    public int Arg2 { get; }
}

public class ExampleCommandHandler : IRequestHandler<ExampleCommand>
{
    /// <inheritdoc />
    public Task<Unit> Handle(ExampleCommand request, CancellationToken cancellationToken)
    {
        return Unit.Task;
    }
}
```

### Calling Handler with MediatR
Starting off with the first benchmark, we are using MediatR as intended creating a new command
and sending it off using a private mediator object.
```c#
[Benchmark]
public async Task CallingHandler_WithMediator()
{
    var command = new ExampleCommand("Example Arg", 2);
    await _mediator.Send(command, CancellationToken.None);
}
```

### Calling Handler Directly
Here we are just calling an already created handler to test the performance overhead
that MediatR might add to calling a method directly.
```c#
[Benchmark]
public async Task CallingHandler_Directly()
{
    var command = new ExampleCommand("Example Arg", 2);
    await _handler.Handle(command, CancellationToken.None);
}
```

### Calling Handler Directly - Transient
Here here are trying the same as above, but now creating a new instance 
of the handler each call. To simulate a transient call.
```c#
[Benchmark]
public async Task CallingHandler_Directly_Transient()
{
    var handler = new ExampleCommandHandler();
    var command = new ExampleCommand("Example Arg", 2);
    await handler.Handle(command, CancellationToken.None);
}
```

### Results
```
// * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
AMD Ryzen 9 3900XT, 1 CPU, 24 logical and 12 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT


|                            Method |      Mean |     Error |    StdDev |  Gen 0 | Allocated |
|---------------------------------- |----------:|----------:|----------:|-------:|----------:|
|       CallingHandler_WithMediator | 818.05 ns | 16.147 ns | 36.446 ns | 0.1621 |   1,360 B |
|           CallingHandler_Directly |  16.19 ns |  0.180 ns |  0.168 ns | 0.0038 |      32 B |
| CallingHandler_Directly_Transient |  15.91 ns |  0.247 ns |  0.231 ns | 0.0038 |      32 B |

```

The results show that calling the handler with MediatR seems to incure a relatively 
high overhead. Drawing specific attention to the memory allocations.

## Memory Tests
In the next set of tests I created a simple loop that calls a MediatR handler repeatedly. The command 
and handlers are the same as in the previous benchmarks.

```c#
var services = new ServiceCollection();
services.AddMediatR(typeof(ExampleCommand));

var provider = services.BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();

for (var i = 0; i < 1_000_000;i++)
{
    var command = new ExampleCommand("Test String", i);
    await mediator.Send(command);
}
```

Running this in release configuration and in dotMemory yields the following results:
![img.png](img.png)

Over `1.67 GB` of memory allocated in 1 min of execution time. With over 2 seconds of GC time.
After running some similar tests on another repo that I am an author of that uses concepts similar 
to the MediatR package. I believe that the the majority memory allocations are coming from:

1. The command being initialized (no option to create commands as structs)
2. The handler being transient, creating a new instance on each call

Reference to Pull Request from other repo for potential fix idea: [Mapr PR 18](https://github.com/rena0157/mapr/pull/18)