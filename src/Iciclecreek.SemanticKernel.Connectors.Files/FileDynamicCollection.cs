using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Iciclecreek.SemanticKernel.Connectors.Files
{
    public class FileDynamicCollection : VectorStoreCollection<object, Dictionary<string, object?>>
    {
        private readonly string _collectionPath;
        private readonly InMemoryDynamicCollection _inMemoryCollection;
        private readonly string _name;
        private readonly string _metaFilePath;
        private readonly VectorStoreCollectionDefinition _definition;
        private readonly string _keyPropertyName;
        private readonly Task _loadingTask;

        public FileDynamicCollection(FileVectorStoreOptions options, string collectionName, InMemoryDynamicCollection inMemoryCollection, VectorStoreCollectionDefinition definition)
        {
            _collectionPath = Path.Combine(options.Path, collectionName);
            _inMemoryCollection = inMemoryCollection;
            _name = collectionName;
            _metaFilePath = Path.Combine(options.Path, $"{collectionName}.json");
            Directory.CreateDirectory(_collectionPath);
            _definition = definition;
            _keyPropertyName = definition?.Properties?.FirstOrDefault(p => p is VectorStoreKeyProperty)?.Name
                ?? "id";

            // If meta file exists, validate schema
            if (File.Exists(_metaFilePath))
            {
                var metaJson = File.ReadAllText(_metaFilePath);
                var meta = JsonSerializer.Deserialize<CollectionMeta>(metaJson);
                if (meta == null || meta.KeyProperty != _keyPropertyName)
                {
                    throw new InvalidOperationException($"Collection '{_name}' already exists with key property '{meta?.KeyProperty}' so cannot be re-created with key property '{_keyPropertyName}'");
                }
            }
            else
            {
                // Write meta file
                var meta = new CollectionMeta { KeyProperty = _keyPropertyName };
                File.WriteAllText(_metaFilePath, JsonSerializer.Serialize(meta));
            }

            Directory.CreateDirectory(_collectionPath);
            // Load all records from file system into memory
            var files = Directory.EnumerateFiles(_collectionPath, "*.json").ToList();

            _loadingTask = Task.Run(async () =>
            {
                await _inMemoryCollection.EnsureCollectionExistsAsync();
                await _inMemoryCollection.UpsertAsync(files.Select(file => JsonSerializer.Deserialize<Dictionary<string, object?>>(File.ReadAllText(file))));
            });
        }

        public override string Name => _name;

        public override async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            if (!File.Exists(_metaFilePath))
            {
                // Write meta file
                var meta = new CollectionMeta { KeyProperty = _keyPropertyName };
                File.WriteAllText(_metaFilePath, JsonSerializer.Serialize(meta));
            }

            if (!Directory.Exists(_collectionPath))
            {
                Directory.CreateDirectory(_collectionPath);
            }

            await _inMemoryCollection.EnsureCollectionExistsAsync(cancellationToken);
        }

        public override async Task EnsureCollectionDeletedAsync(CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            Directory.Delete(_collectionPath, true);

            File.Delete(_metaFilePath);

            await _inMemoryCollection.EnsureCollectionDeletedAsync(cancellationToken);
        }

        public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(File.Exists(_metaFilePath));
        }

        public override async Task UpsertAsync(Dictionary<string, object?> record, CancellationToken cancellationToken = default)
        {
            await _loadingTask; // Ensure loading is complete before upserting

            await _inMemoryCollection.UpsertAsync(record, cancellationToken);
            if (!record.ContainsKey(_keyPropertyName)) throw new ArgumentException($"Record must contain a '{_keyPropertyName}' key");
            var key = record[_keyPropertyName]?.ToString();
            if (string.IsNullOrEmpty(key)) throw new ArgumentException($"Record '{_keyPropertyName}' cannot be null or empty");
            string filePath = Path.Combine(_collectionPath, key + ".json");
            string json = JsonSerializer.Serialize(record);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }

        public override async Task UpsertAsync(IEnumerable<Dictionary<string, object?>> records, CancellationToken cancellationToken = default)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));

            await _loadingTask;

            foreach (var record in records)
                await UpsertAsync(record, cancellationToken);
        }

        public override async Task<Dictionary<string, object?>?> GetAsync(object key, RecordRetrievalOptions? options, CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            return await _inMemoryCollection.GetAsync(key, options, cancellationToken);
        }

        public override async IAsyncEnumerable<Dictionary<string, object?>> GetAsync(Expression<Func<Dictionary<string, object?>, bool>> predicate, int maxResults, FilteredRecordRetrievalOptions<Dictionary<string, object?>>? options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            await foreach (var item in _inMemoryCollection.GetAsync(predicate, maxResults, options, cancellationToken))
                yield return item;
        }

        public override async Task DeleteAsync(object key, CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            await _inMemoryCollection.DeleteAsync(key, cancellationToken);

            string filePath = Path.Combine(_collectionPath, key.ToString() + ".json");
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public override object? GetService(Type serviceType, object? serviceKey = null)
        {
            if (serviceType == typeof(VectorStoreCollectionDefinition))
                return _definition;
            return _inMemoryCollection.GetService(serviceType, serviceKey);
        }

        public override async IAsyncEnumerable<VectorSearchResult<Dictionary<string, object?>>> SearchAsync<TInput>(TInput input, int maxResults, VectorSearchOptions<Dictionary<string, object?>>? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            await foreach (var item in _inMemoryCollection.SearchAsync(input, maxResults, options, cancellationToken))
                yield return item;
        }

        private class CollectionMeta
        {
            public string KeyProperty { get; set; }
            public bool IsDynamic => true;
        }
    }
}
