//using System;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.VectorData;
//using Microsoft.SemanticKernel;
//using Iciclecreek.SemanticKernel.Connectors.FileMemory;
//using Xunit;

//namespace Iciclecreek.SemanticKernel.Connectors.FileMemory.Tests;


///// <summary>
///// Contains tests for the <see cref="FileSystemServiceCollectionExtensions"/> class.
///// </summary>
//public class FileSystemServiceCollectionExtensionsTests
//{
//    private readonly IServiceCollection _serviceCollection;

//    public FileSystemServiceCollectionExtensionsTests()
//    {
//        this._serviceCollection = new ServiceCollection();
//    }

//    [Fact]
//    public void AddVectorStoreRegistersClass()
//    {
//        // Act.
//        this._serviceCollection.AddFileSystemVectorStore();

//        // Assert.
//        var serviceProvider = this._serviceCollection.BuildServiceProvider();
//        var vectorStore = serviceProvider.GetRequiredService<VectorStore>();
//        Assert.NotNull(vectorStore);
//        Assert.IsType<FileSystemVectorStore>(vectorStore);
//    }

//    [Fact]
//    public void AddVectorStoreRecordCollectionRegistersClass()
//    {
//        // Act.
//        this._serviceCollection.AddFileSystemVectorStoreRecordCollection<string, TestRecord>("testcollection");

//        // Assert.
//        this.AssertVectorStoreRecordCollectionCreated();
//    }

//    private void AssertVectorStoreRecordCollectionCreated()
//    {
//        var serviceProvider = this._serviceCollection.BuildServiceProvider();

//        var collection = serviceProvider.GetRequiredService<VectorStoreCollection<string, TestRecord>>();
//        Assert.NotNull(collection);
//        Assert.IsType<FileSystemCollection<string, TestRecord>>(collection);

//        var vectorizedSearch = serviceProvider.GetRequiredService<IVectorSearchable<TestRecord>>();
//        Assert.NotNull(vectorizedSearch);
//        Assert.IsType<FileSystemCollection<string, TestRecord>>(vectorizedSearch);
//    }

//#pragma warning disable CA1812 // Avoid uninstantiated internal classes
//    private sealed class TestRecord
//#pragma warning restore CA1812 // Avoid uninstantiated internal classes
//    {
//        [VectorStoreKey]
//        public string Id { get; set; } = string.Empty;

//        [VectorStoreVector(4)]
//        public ReadOnlyMemory<float> Vector { get; set; }
//    }
//}