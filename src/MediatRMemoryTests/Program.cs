using MediatR;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddMediatR(typeof(ExampleCommand));

var provider = services.BuildServiceProvider();

var mediator = provider.GetRequiredService<IMediator>();

for (var i = 0; i < 1_000_000;i++)
{
    var command = new ExampleCommand("Test String", i);
    await mediator.Send(command);
}

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