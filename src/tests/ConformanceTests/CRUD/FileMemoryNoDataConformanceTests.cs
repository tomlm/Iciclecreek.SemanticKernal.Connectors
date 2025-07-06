// Copyright (c) Microsoft. All rights reserved.

using FileMemory.ConformanceTests.Support;
using VectorData.ConformanceTests.CRUD;
using VectorData.ConformanceTests.Support;
using Xunit;

namespace FileMemory.ConformanceTests.CRUD;

public class FileMemoryNoDataConformanceTests(FileMemoryNoDataConformanceTests.Fixture fixture)
    : NoDataConformanceTests<string>(fixture), IClassFixture<FileMemoryNoDataConformanceTests.Fixture>
{
    public new class Fixture : NoDataConformanceTests<string>.Fixture
    {
        public override TestStore TestStore => FileMemoryTestStore.Instance;
    }
}
