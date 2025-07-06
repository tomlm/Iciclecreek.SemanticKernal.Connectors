// Copyright (c) Microsoft. All rights reserved.

using FileMemory.ConformanceTests.Support;
using VectorData.ConformanceTests.CRUD;
using VectorData.ConformanceTests.Support;
using Xunit;

namespace FileMemory.ConformanceTests.CRUD;

public class FileMemoryNoVectorConformanceTests(FileMemoryNoVectorConformanceTests.Fixture fixture)
    : NoVectorConformanceTests<string>(fixture), IClassFixture<FileMemoryNoVectorConformanceTests.Fixture>
{
    public new class Fixture : NoVectorConformanceTests<string>.Fixture
    {
        public override TestStore TestStore => FileMemoryTestStore.Instance;
    }
}
