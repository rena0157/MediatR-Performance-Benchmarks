using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

BenchmarkRunner.Run<Benchy>();

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

[MemoryDiagnoser]
public class Benchy
{
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

    [Benchmark]
    public async Task CallingHandler_WithMediator()
    {
        var command = new ExampleCommand("Example Arg", 2);
        await _mediator.Send(command, CancellationToken.None);
    }

    [Benchmark]
    public async Task CallingHandler_Directly()
    {
        var command = new ExampleCommand("Example Arg", 2);
        await _handler.Handle(command, CancellationToken.None);
    }

    [Benchmark]
    public async Task CallingHandler_Directly_Transient()
    {
        var handler = new ExampleCommandHandler();
        var command = new ExampleCommand("Example Arg", 2);
        await handler.Handle(command, CancellationToken.None);
    }
}

public class ExampleCommandHandler : IRequestHandler<ExampleCommand>
{
    /// <inheritdoc />
    public Task<Unit> Handle(ExampleCommand request, CancellationToken cancellationToken)
    {
        return Unit.Task;
    }
}