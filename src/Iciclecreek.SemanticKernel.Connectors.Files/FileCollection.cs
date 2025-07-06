using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Iciclecreek.SemanticKernel.Connectors.Files
{
    public class FileCollection<TKey, TRecord> : VectorStoreCollection<TKey, TRecord> where TKey : notnull where TRecord : class
    {
        private readonly FileVectorStoreOptions _options;
        private readonly string _collectionPath;
        private readonly string _name;
        private readonly string _metaFilePath;
        private readonly string _keyTypeName;
        private readonly string _recordTypeName;
        private readonly InMemoryCollection<TKey, TRecord> _inMemoryCollection;
        private readonly VectorStoreCollectionDefinition? _definition;
        private readonly PropertyInfo _keyProperty;
        private readonly Task _loadingTask;


        public FileCollection(FileVectorStoreOptions options, string collectionName, InMemoryCollection<TKey, TRecord> inMemoryCollection, VectorStoreCollectionDefinition? definition = null)
        {
            _options = options;
            _name = collectionName;
            _collectionPath = Path.Combine(options.Path, collectionName);
            _metaFilePath = Path.Combine(_collectionPath, "collection.json");
            Directory.CreateDirectory(_collectionPath);
            _keyTypeName = typeof(TKey).Name;
            _recordTypeName = typeof(TRecord).Name;
            _inMemoryCollection = inMemoryCollection;
            this._definition = definition;

            // Determine key property info from definition if available
            string keyPropertyName = definition?.Properties?.FirstOrDefault(p => p is VectorStoreKeyProperty)?.Name
                ?? typeof(TRecord).GetProperties().Where(p => p.GetCustomAttribute<VectorStoreKeyAttribute>() != null).Select(p => p.Name).FirstOrDefault()
                ?? "Id";
            _keyProperty = typeof(TRecord).GetProperty(keyPropertyName)
                ?? throw new ArgumentException($"Record must have an '{keyPropertyName}' or 'id' property");

            // If meta file exists, validate types
            if (File.Exists(_metaFilePath))
            {
                var metaJson = File.ReadAllText(_metaFilePath);
                var meta = JsonSerializer.Deserialize<CollectionMeta>(metaJson);
                if (meta == null || meta.KeyType != _keyTypeName || meta.RecordType != _recordTypeName)
                {
                    throw new InvalidOperationException($"Collection '{_name}' already exists and with data type '{meta.RecordType}' so cannot be re-created with data type '{_recordTypeName}'");
                }
            }
            else
            {
                // Write meta file
                var meta = new CollectionMeta { KeyType = _keyTypeName, RecordType = _recordTypeName };
                File.WriteAllText(_metaFilePath, JsonSerializer.Serialize(meta));
            }

            _loadingTask = Task.Run(async () =>
            {
                await _inMemoryCollection.EnsureCollectionExistsAsync();

                // Load all records from file system into memory
                foreach (var file in Directory.EnumerateFiles(_collectionPath, "*.json"))
                {
                    if (Path.GetFileName(file) == "collection.json") continue;
                    try
                    {

                        var json = await File.ReadAllTextAsync(file);
                        var record = JsonSerializer.Deserialize<TRecord>(json);
                        if (record != null)
                        {
                            var key = (TKey)Convert.ChangeType(_keyProperty.GetValue(record), typeof(TKey));
                            await _inMemoryCollection.UpsertAsync(record);
                        }
                    }
                    catch (FileNotFoundException err)
                    {
                        // Log or handle the error as needed
                        System.Diagnostics.Debug.WriteLine($"Error loading record from file {file}: {err.Message}");
                    }
                }
            });
        }

        public override string Name => _name;

        public override Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(_collectionPath);
            return _inMemoryCollection.EnsureCollectionExistsAsync(cancellationToken);
        }

        public override Task EnsureCollectionDeletedAsync(CancellationToken cancellationToken = default)
        {
            if (Directory.Exists(_collectionPath))
                Directory.Delete(_collectionPath, true);
            return _inMemoryCollection.EnsureCollectionDeletedAsync(cancellationToken);
        }

        public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Directory.Exists(_collectionPath));
        }

        public override async Task UpsertAsync(TRecord record, CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            await _inMemoryCollection.UpsertAsync(record, cancellationToken);
            var key = _keyProperty.GetValue(record)?.ToString();
            if (string.IsNullOrEmpty(key)) throw new ArgumentException($"Record key property '{_keyProperty.Name}' cannot be null or empty");
            string filePath = Path.Combine(_collectionPath, key + ".json");
            string json = JsonSerializer.Serialize(record);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }

        public override async Task UpsertAsync(IEnumerable<TRecord> records, CancellationToken cancellationToken = default)
        {
            if (records == null)
                throw new ArgumentNullException(nameof(records));
            
            foreach (var record in records)
                await UpsertAsync(record, cancellationToken);
        }

        public override async Task DeleteAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            await _inMemoryCollection.DeleteAsync(key, cancellationToken);
            string filePath = Path.Combine(_collectionPath, key.ToString() + ".json");
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        public override async Task<TRecord?> GetAsync(TKey key, RecordRetrievalOptions? options, CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            return await _inMemoryCollection.GetAsync(key, options, cancellationToken);
        }

        public override async IAsyncEnumerable<TRecord> GetAsync(Expression<Func<TRecord, bool>> predicate, int maxResults, FilteredRecordRetrievalOptions<TRecord>? options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            await foreach (var item in _inMemoryCollection.GetAsync(predicate, maxResults, options, cancellationToken))
                yield return item;
        }

        public override async IAsyncEnumerable<VectorSearchResult<TRecord>> SearchAsync<TInput>(TInput input, int maxResults, VectorSearchOptions<TRecord>? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await _loadingTask;

            await foreach (var item in _inMemoryCollection.SearchAsync(input, maxResults, options, cancellationToken))
                yield return item;
        }

        public override object? GetService(Type serviceType, object? serviceKey = null)
        {
            return _inMemoryCollection.GetService(serviceType, serviceKey);
        }

        private class CollectionMeta
        {
            public string KeyType { get; set; }
            public string RecordType { get; set; }
        }
    }
}
