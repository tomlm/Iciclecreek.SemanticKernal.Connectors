using Microsoft.Extensions.VectorData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Iciclecreek.SemanticKernel.Connectors.FileMemory
{
    public class FileDynamicCollection : VectorStoreCollection<object, Dictionary<string, object?>>
    {
        private readonly string _collectionPath;
        private readonly string _name;

        public FileDynamicCollection(string collectionPath)
        {
            _collectionPath = collectionPath;
            _name = Path.GetFileName(collectionPath);
            Directory.CreateDirectory(_collectionPath);
        }

        public override string Name => _name;

        public override Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public override Task EnsureCollectionDeletedAsync(CancellationToken cancellationToken = default)
        {
            if (Directory.Exists(_collectionPath))
                Directory.Delete(_collectionPath, true);
            return Task.CompletedTask;
        }

        public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Directory.Exists(_collectionPath));

        public override Task UpsertAsync(Dictionary<string, object?> record, CancellationToken cancellationToken = default)
        {
            if (!record.ContainsKey("id")) throw new ArgumentException("Record must contain an 'id' key");
            var key = record["id"]?.ToString();
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Record 'id' cannot be null or empty");
            string filePath = Path.Combine(_collectionPath, key + ".json");
            string json = JsonSerializer.Serialize(record);
            File.WriteAllText(filePath, json);
            return Task.CompletedTask;
        }

        public override Task UpsertAsync(IEnumerable<Dictionary<string, object?>> records, CancellationToken cancellationToken = default)
        {
            foreach (var record in records)
                UpsertAsync(record, cancellationToken).Wait();
            return Task.CompletedTask;
        }

        public override Task<Dictionary<string, object?>?> GetAsync(object key, RecordRetrievalOptions? options, CancellationToken cancellationToken = default)
        {
            string filePath = Path.Combine(_collectionPath, key.ToString() + ".json");
            if (!File.Exists(filePath))
                return Task.FromResult<Dictionary<string, object?>?>(null);
            string json = File.ReadAllText(filePath);
            return Task.FromResult(JsonSerializer.Deserialize<Dictionary<string, object?>>(json));
        }

        public override async IAsyncEnumerable<Dictionary<string, object?>> GetAsync(Expression<Func<Dictionary<string, object?>, bool>> predicate, int maxResults, FilteredRecordRetrievalOptions<Dictionary<string, object?>>? options, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            int count = 0;
            foreach (var file in Directory.EnumerateFiles(_collectionPath, "*.json"))
            {
                if (cancellationToken.IsCancellationRequested) yield break;
                var json = await File.ReadAllTextAsync(file, cancellationToken);
                var record = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
                if (predicate.Compile().Invoke(record))
                {
                    yield return record;
                    count++;
                    if (count >= maxResults) yield break;
                }
            }
        }

        public override Task DeleteAsync(object key, CancellationToken cancellationToken = default)
        {
            string filePath = Path.Combine(_collectionPath, key.ToString() + ".json");
            if (File.Exists(filePath))
                File.Delete(filePath);
            return Task.CompletedTask;
        }

        public override object GetService(Type serviceType, object serviceKey = null) => null;

        public override async IAsyncEnumerable<VectorSearchResult<Dictionary<string, object?>>> SearchAsync<TInput>(TInput input, int maxResults, VectorSearchOptions<Dictionary<string, object?>>? options = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Not implemented: would require vector search logic
            await Task.Yield();
            yield break;
        }
    }
}
