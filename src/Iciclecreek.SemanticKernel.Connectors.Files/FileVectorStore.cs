using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Iciclecreek.SemanticKernel.Connectors.Files
{
    /// <summary>
    /// This is a vector store which stores each collection in a separate folder and each record in a file, wrapping InMemoryVectorStore.
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

        public override async Task<bool> CollectionExistsAsync(string name, CancellationToken cancellationToken = default)
        {
            // Check both file system and in-memory
            string path = Path.Combine(_options.Path, name);
            bool existsOnDisk = Directory.Exists(path);
            bool existsInMemory = await _inMemoryStore.CollectionExistsAsync(name, cancellationToken);
            return existsOnDisk || existsInMemory;
        }

        public override async Task EnsureCollectionDeletedAsync(string name, CancellationToken cancellationToken = default)
        {
            string path = Path.Combine(_options.Path, name);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
            path = $"{path}.json";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            await _inMemoryStore.EnsureCollectionDeletedAsync(name, cancellationToken);
        }

        public override VectorStoreCollection<TKey, TRecord> GetCollection<TKey, TRecord>(string collectionName, VectorStoreCollectionDefinition? definition = null)
        {
            var inMemoryCollection = (InMemoryCollection<TKey, TRecord>)_inMemoryStore.GetCollection<TKey, TRecord>(collectionName, definition);
            var collection = new FileCollection<TKey, TRecord>(_options, collectionName, inMemoryCollection, definition);

            return collection;
        }

        public override VectorStoreCollection<object, Dictionary<string, object?>> GetDynamicCollection(string name, VectorStoreCollectionDefinition definition)
        {
            var inMemoryCollection = (InMemoryDynamicCollection)_inMemoryStore.GetDynamicCollection(name, definition);
            return new FileDynamicCollection(_options, name, inMemoryCollection, definition);
        }

        public override object? GetService(Type serviceType, object? serviceKey = null)
        {
            return _inMemoryStore.GetService(serviceType, serviceKey);
        }

        public override async IAsyncEnumerable<string> ListCollectionNamesAsync(CancellationToken cancellationToken = default)
        {
            // List from disk
            foreach (var dir in Directory.EnumerateDirectories(_options.Path))
            {
                yield return Path.GetFileName(dir);
            }
        }
    }
}
