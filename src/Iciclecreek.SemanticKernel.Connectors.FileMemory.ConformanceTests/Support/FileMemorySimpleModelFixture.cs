// Copyright (c) Microsoft. All rights reserved.

using VectorData.ConformanceTests.Support;

namespace FileMemory.ConformanceTests.Support;

public class FileMemorySimpleModelFixture : SimpleModelFixture<int>
{
    public override TestStore TestStore => FileMemoryTestStore.Instance;
}
