// Copyright (c) Microsoft. All rights reserved.

using FileMemory.ConformanceTests.Support;
using VectorData.ConformanceTests.CRUD;
using VectorData.ConformanceTests.Support;
using Xunit;

namespace FileMemory.ConformanceTests.CRUD;

public class FileMemoryDynamicRecordConformanceTests(FileMemoryDynamicDataModelFixture fixture)
    : DynamicDataModelConformanceTests<int>(fixture), IClassFixture<FileMemoryDynamicDataModelFixture>
{
    // FileMemory always returns the vectors (IncludeVectors = false isn't respected)
    public override async Task GetAsync_WithoutVectors()
    {
        var expectedRecord = fixture.TestData[0];

        var received = await fixture.Collection.GetAsync(
            (int)expectedRecord[DynamicDataModelFixture<int>.KeyPropertyName]!,
            new() { IncludeVectors = false });

        AssertEquivalent(expectedRecord, received, includeVectors: true, fixture.TestStore.VectorsComparable);
    }
}
