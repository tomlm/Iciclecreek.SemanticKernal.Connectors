// Copyright (c) Microsoft. All rights reserved.

using FileMemory.ConformanceTests.Support;
using VectorData.ConformanceTests;
using VectorData.ConformanceTests.Support;
using Xunit;

#pragma warning disable CA2000 // Dispose objects before losing scope

namespace FileMemory.ConformanceTests;

public class FileMemoryEmbeddingTypeTests(FileMemoryEmbeddingTypeTests.Fixture fixture)
    : EmbeddingTypeTests<Guid>(fixture), IClassFixture<FileMemoryEmbeddingTypeTests.Fixture>
{
    public new class Fixture : EmbeddingTypeTests<Guid>.Fixture
    {
        public override TestStore TestStore => FileMemoryTestStore.Instance;

        public override bool RecreateCollection => true;
        public override bool AssertNoVectorsLoadedWithEmbeddingGeneration => false;
    }
}
