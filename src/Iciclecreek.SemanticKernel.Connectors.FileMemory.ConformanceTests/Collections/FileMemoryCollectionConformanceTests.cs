// Copyright (c) Microsoft. All rights reserved.

using FileMemory.ConformanceTests.Support;
using VectorData.ConformanceTests.Collections;
using Xunit;

namespace FileMemory.ConformanceTests.Collections;

public class FileMemoryCollectionConformanceTests(FileMemoryFixture fixture)
    : CollectionConformanceTests<string>(fixture), IClassFixture<FileMemoryFixture>
{
}
