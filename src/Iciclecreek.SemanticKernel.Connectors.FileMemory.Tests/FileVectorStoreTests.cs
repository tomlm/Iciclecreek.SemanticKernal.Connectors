using Microsoft.Extensions.VectorData;
using System.Runtime.CompilerServices;

namespace Iciclecreek.SemanticKernel.Connectors.FileMemory.Tests;

/// <summary>
/// Contains tests for the <see cref="FileVectorStore"/> class.
/// </summary>
public class FileVectorStoreTests
{
    //private static IEmbeddingGenerator s_EmbeddingGenerator = new EmbeddingClient("gpt-4.1-nano", new ApiKeyCredential(""))
    //                                                            .AsIEmbeddingGenerator();

    public FileVectorStoreOptions GetOptions()
    {
        return new FileVectorStoreOptions
        {
            EmbeddingGenerator = new MockEmbeddingGenerator(),
            Path = Path.Combine(Path.GetTempPath(), nameof(FileVectorStoreTests))
        };
    }

    private string GetTestName([CallerMemberName] string testName = null)
        => testName;

    [Fact]
    public async Task GetCollectionReturnsCollection()
    {
        var collectionName = GetTestName();
        // Arrange.
        using var sut = new FileVectorStore(GetOptions());

        await sut.EnsureCollectionDeletedAsync(collectionName);

        // Act.
        var actual = sut.GetCollection<string, SinglePropsModel<string>>(collectionName);

        // Assert.
        Assert.NotNull(actual);
        Assert.IsType<FileCollection<string, SinglePropsModel<string>>>(actual);
    }

    [Fact]
    public async Task GetCollectionReturnsCollectionWithNonStringKey()
    {
        var collectionName = GetTestName();
        // Arrange.
        using var sut = new FileVectorStore(GetOptions());

        await sut.EnsureCollectionDeletedAsync(collectionName);

        // Act.
        var actual = sut.GetCollection<int, SinglePropsModel<int>>(collectionName);

        // Assert.
        Assert.NotNull(actual);
        Assert.IsType<FileCollection<int, SinglePropsModel<int>>>(actual);
    }

    [Fact]
    public async Task GetCollectionDoesNotAllowADifferentDataTypeThanPreviouslyUsedAsync()
    {
        var collectionName = GetTestName();
        // Arrange.
        using var sut = new FileVectorStore(GetOptions());
        await sut.EnsureCollectionDeletedAsync(collectionName);

        var stringKeyCollection = sut.GetCollection<string, SinglePropsModel<string>>(collectionName);
        await stringKeyCollection.EnsureCollectionExistsAsync();

        // Act and assert.
        var exception = Assert.Throws<InvalidOperationException>(() => sut.GetCollection<string, SecondModel>(collectionName));
        Assert.Equal($"Collection '{collectionName}' already exists and with data type 'SinglePropsModel`1' so cannot be re-created with data type 'SecondModel'.", exception.Message);
    }

#pragma warning disable CA1812 // Classes are used as generic arguments
    private sealed class SinglePropsModel<TKey>
    {
        [VectorStoreKey]
        public required TKey Key { get; set; }

        [VectorStoreData]
        public string Data { get; set; } = string.Empty;

        [VectorStoreVector(4)]
        public ReadOnlyMemory<float>? Vector { get; set; }

        public string? NotAnnotated { get; set; }
    }

    private sealed class SecondModel
    {
        [VectorStoreKey]
        public required int Key { get; set; }

        [VectorStoreData]
        public string Data { get; set; } = string.Empty;
    }
#pragma warning restore CA1812
}