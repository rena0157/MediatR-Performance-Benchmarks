using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Mapr;
using Mapr.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Hello, World!");
BenchmarkRunner.Run<Bench>();

[MemoryDiagnoser]
public class Bench
{
    private readonly IMap<string, int> _map;
    private readonly IMapper _mapper;

    public Bench()
    {
        var services = new ServiceCollection();

        services.AddMapr(config =>
        {
            config.Scan<ExampleMap>();
        });

        var provider = services.BuildServiceProvider();
        
        _map = new ExampleMap();
        _mapper = provider.GetRequiredService<IMapper>();
    }

    [Benchmark]
    public void CallingMapper()
    {
        _mapper.Map<string, int>("Test String");
    }

    [Benchmark]
    public void CallingMap()
    {
        _map.Map("Test String");
    }

    [Benchmark]
    public void Calling_SingletonMap()
    {
        _mapper.Map<int, string>(23);
    }
}

public class ExampleMap : IMap<string, int>
{
    public int Map(string source)
    {
        return 0;
    }
}

[Map(Lifetime = MapLifetime.Singleton)]
public class ExampleSingletonMap : IMap<int, string>
{
    /// <inheritdoc />
    public string Map(int source)
    {
        return string.Empty;
    }
}