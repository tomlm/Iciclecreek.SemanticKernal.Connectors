using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Iciclecreek.SemanticKernel.Connectors.FileMemory
{

    /// <summary>
    /// This is a vector store which stores each collection in a seperate folder and each record in a file 
    /// </summary>
    public class FileVectorStore : VectorStore
    {
        private readonly InMemoryVectorStore _inMemoryStore = new InMemoryVectorStore();
        private readonly FileVectorStoreOptions _options;

        public FileVectorStore(FileVectorStoreOptions options)
        {
            _options = options;
            Directory.CreateDirectory(_options.Path);
        }

        public override Task<bool> CollectionExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            string path = Path.Combine(_options.Path, name);
            return Task.FromResult(Directory.Exists(path));
        }

        public override async Task EnsureCollectionDeletedAsync(string name, CancellationToken cancellationToken = default)
        {
            string path = Path.Combine(_options.Path, name);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
            await _inMemoryStore.EnsureCollectionDeletedAsync(name, cancellationToken);
        }

        public override VectorStoreCollection<TKey, TRecord> GetCollection<TKey, TRecord>(string collectionName, VectorStoreCollectionDefinition? definition = null)
        {
            string path = Path.Combine(_options.Path, collectionName);
            Directory.CreateDirectory(path);

            _inMemoryStore.GetCollection<TKey, TRecord>(collectionName, definition);
            return new FileCollection<TKey, TRecord>(_options, collectionName);
        }

        public override VectorStoreCollection<object, Dictionary<string, object?>> GetDynamicCollection(string name, VectorStoreCollectionDefinition definition)
        {
            string path = Path.Combine(_options.Path, name);
            Directory.CreateDirectory(path);
            return new FileDynamicCollection(path);
        }

        public override object? GetService(Type serviceType, object? serviceKey = null)
        {
            return null;
        }

        public override async IAsyncEnumerable<string> ListCollectionNamesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var dir in Directory.EnumerateDirectories(_options.Path))
            {
                yield return Path.GetFileName(dir);
            }
        }
    }
}
