using Microsoft.Extensions.AI;

namespace Iciclecreek.SemanticKernel.Connectors.FileMemory.Tests;

public class MockEmbeddingGenerator : IEmbeddingGenerator
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        return null;
    }
}
