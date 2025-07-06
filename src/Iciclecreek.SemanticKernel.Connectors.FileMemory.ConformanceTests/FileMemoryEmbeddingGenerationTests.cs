// Copyright (c) Microsoft. All rights reserved.

using FileMemory.ConformanceTests.Support;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using VectorData.ConformanceTests;
using VectorData.ConformanceTests.Support;
using Xunit;

namespace FileMemory.ConformanceTests;

public class FileMemoryEmbeddingGenerationTests(FileMemoryEmbeddingGenerationTests.StringVectorFixture stringVectorFixture, FileMemoryEmbeddingGenerationTests.RomOfFloatVectorFixture romOfFloatVectorFixture)
    : EmbeddingGenerationTests<int>(stringVectorFixture, romOfFloatVectorFixture), IClassFixture<FileMemoryEmbeddingGenerationTests.StringVectorFixture>, IClassFixture<FileMemoryEmbeddingGenerationTests.RomOfFloatVectorFixture>
{
    // FileMemory doesn't allowing accessing the same collection via different .NET types (it's unique in this).
    // The following dynamic tests attempt to access the fixture collection - which is created with Record - via
    // Dictionary<string, object?>.
    public override Task SearchAsync_with_property_generator_dynamic() => Task.CompletedTask;
    public override Task UpsertAsync_dynamic() => Task.CompletedTask;
    public override Task UpsertAsync_batch_dynamic() => Task.CompletedTask;

    // The same applies to the custom type test:
    public override Task SearchAsync_with_custom_input_type() => Task.CompletedTask;

    // The test relies on creating a new FileMemoryVectorStore configured with a store-default generator, but with FileMemory that store
    // doesn't share the seeded data with the fixture store (since each FileMemoryVectorStore has its own private data).
    // Test coverage is already largely sufficient via the property and collection tests.
    public override Task SearchAsync_with_store_generator() => Task.CompletedTask;

    public new class StringVectorFixture : EmbeddingGenerationTests<int>.StringVectorFixture
    {
        public override TestStore TestStore => FileMemoryTestStore.Instance;

        // Note that with FileMemory specifically, we can't create a vector store with an embedding generator, since it wouldn't share the seeded data with the fixture store.
        public override VectorStore CreateVectorStore(IEmbeddingGenerator? embeddingGenerator)
            => FileMemoryTestStore.Instance.DefaultVectorStore;

        public override Func<IServiceCollection, IServiceCollection>[] DependencyInjectionStoreRegistrationDelegates =>
        [
            // The FileMemory DI methods register a new vector store instance, which doesn't share the collection seeded by the
            // fixture and the test fails.
            //services => services.AddFileMemoryVectorStore()
        ];

        public override Func<IServiceCollection, IServiceCollection>[] DependencyInjectionCollectionRegistrationDelegates =>
        [
            // The FileMemory DI methods register a new vector store instance, which doesn't share the collection seeded by the
            // fixture and the test fails.
            // services => services.AddFileMemoryVectorStoreRecordCollection<int, RecordWithAttributes>(this.CollectionName)
        ];
    }

    public new class RomOfFloatVectorFixture : EmbeddingGenerationTests<int>.RomOfFloatVectorFixture
    {
        public override TestStore TestStore => FileMemoryTestStore.Instance;

        // Note that with FileMemory specifically, we can't create a vector store with an embedding generator, since it wouldn't share the seeded data with the fixture store.
        public override VectorStore CreateVectorStore(IEmbeddingGenerator? embeddingGenerator)
            => FileMemoryTestStore.Instance.DefaultVectorStore;

        public override Func<IServiceCollection, IServiceCollection>[] DependencyInjectionStoreRegistrationDelegates =>
        [
            // The FileMemory DI methods register a new vector store instance, which doesn't share the collection seeded by the
            // fixture and the test fails.
            // services => services.AddFileMemoryVectorStore()
        ];

        public override Func<IServiceCollection, IServiceCollection>[] DependencyInjectionCollectionRegistrationDelegates =>
        [
            // The FileMemory DI methods register a new vector store instance, which doesn't share the collection seeded by the
            // fixture and the test fails.
            // services => services.AddFileMemoryVectorStoreRecordCollection<int, RecordWithAttributes>(this.CollectionName)
        ];
    }
}
