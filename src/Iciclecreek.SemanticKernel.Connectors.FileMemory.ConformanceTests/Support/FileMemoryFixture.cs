// Copyright (c) Microsoft. All rights reserved.

using VectorData.ConformanceTests.Support;

namespace FileMemory.ConformanceTests.Support;

public class FileMemoryFixture : VectorStoreFixture
{
    public override TestStore TestStore => FileMemoryTestStore.Instance;
}
