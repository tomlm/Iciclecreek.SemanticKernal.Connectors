// Copyright (c) Microsoft. All rights reserved.

using FileMemory.ConformanceTests.Support;
using VectorData.ConformanceTests.CRUD;
using Xunit;

namespace FileMemory.ConformanceTests.CRUD;

public class FileMemoryRecordConformanceTests(FileMemorySimpleModelFixture fixture)
    : RecordConformanceTests<int>(fixture), IClassFixture<FileMemorySimpleModelFixture>
{
    // FileMemory always returns the vectors (IncludeVectors = false isn't respected)
    public override async Task GetAsync_WithoutVectors()
    {
        var expectedRecord = fixture.TestData[0];
        var received = await fixture.Collection.GetAsync(expectedRecord.Id, new() { IncludeVectors = false });

        expectedRecord.AssertEqual(received, includeVectors: true, fixture.TestStore.VectorsComparable);
    }
}
