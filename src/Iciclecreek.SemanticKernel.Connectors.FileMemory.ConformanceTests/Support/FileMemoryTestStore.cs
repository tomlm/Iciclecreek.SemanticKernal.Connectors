// Copyright (c) Microsoft. All rights reserved.

using Iciclecreek.SemanticKernel.Connectors.FileMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI.Embeddings;
using System.ClientModel;
using VectorData.ConformanceTests.Support;

namespace FileMemory.ConformanceTests.Support;

internal sealed class FileMemoryTestStore : TestStore
{
    public static FileMemoryTestStore Instance { get; } = new();

    public FileVectorStore GetVectorStore(FileVectorStoreOptions options)
        => new FileVectorStore(options);

    private FileMemoryTestStore()
    {
    }

    protected override Task StartAsync()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<FileMemoryTestStore>()
            .Build();

        this.DefaultVectorStore = new FileVectorStore(new FileVectorStoreOptions()
        {
            EmbeddingGenerator = new EmbeddingClient("gpt-4.1-nano", new ApiKeyCredential(config["OpenAIKey"]))
                        .AsIEmbeddingGenerator()

        });

        return Task.CompletedTask;
    }
}
