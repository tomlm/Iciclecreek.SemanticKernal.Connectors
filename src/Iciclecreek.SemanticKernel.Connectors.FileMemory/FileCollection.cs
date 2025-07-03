using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Iciclecreek.SemanticKernel.Connectors.FileMemory
{
    public class FileCollection<TKey, TRecord> : InMemoryCollection<TKey, TRecord> where TKey : notnull where TRecord : class
    {
        private readonly FileVectorStoreOptions _options;
        private readonly string _collectionPath;
        private readonly string _name;
        private readonly string _metaFilePath;
        private readonly string _keyTypeName;
        private readonly string _recordTypeName;

        public FileCollection(FileVectorStoreOptions options, string collectionName) : base(collectionName, null)
        {
            _options = options;
            _name = collectionName;
            _collectionPath = Path.Combine(options.Path, collectionName);
            _metaFilePath = Path.Combine(_collectionPath, "collection.json");
            Directory.CreateDirectory(_collectionPath);
            _keyTypeName = typeof(TKey).Name;
            _recordTypeName = typeof(TRecord).Name;

            // If meta file exists, validate types
            if (File.Exists(_metaFilePath))
            {
                var metaJson = File.ReadAllText(_metaFilePath);
                var meta = JsonSerializer.Deserialize<CollectionMeta>(metaJson);
                if (meta == null || meta.KeyType != _keyTypeName || meta.RecordType != _recordTypeName)
                {
                    throw new InvalidOperationException($"Collection '{_name}' already exists and with data type '{meta.RecordType}' so cannot be re-created with data type '{_recordTypeName}'.");
                }
            }
            else
            {
                // Write meta file
                var meta = new CollectionMeta { KeyType = _keyTypeName, RecordType = _recordTypeName };
                File.WriteAllText(_metaFilePath, JsonSerializer.Serialize(meta));
            }

            // Load all records from file system into memory
            foreach (var file in Directory.EnumerateFiles(_collectionPath, "*.json"))
            {
                if (Path.GetFileName(file) == "collection.json") continue;
                var json = File.ReadAllText(file);
                var record = JsonSerializer.Deserialize<TRecord>(json);
                if (record != null)
                {
                    var keyProp = typeof(TRecord).GetProperty("Id") ?? typeof(TRecord).GetProperty("id");
                    if (keyProp == null) throw new ArgumentException("Record must have an 'Id' property");
                    var key = (TKey)Convert.ChangeType(keyProp.GetValue(record), typeof(TKey));
                    base.UpsertAsync(record).Wait();
                }
            }
        }

        public string Name => _name;

        public override async Task UpsertAsync(TRecord record, CancellationToken cancellationToken = default)
        {
            await base.UpsertAsync(record, cancellationToken);
            var keyProp = typeof(TRecord).GetProperty("Id") ?? typeof(TRecord).GetProperty("id");
            if (keyProp == null) throw new ArgumentException("Record must have an 'Id' property");
            var key = keyProp.GetValue(record)?.ToString();
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Record 'Id' cannot be null or empty");
            string filePath = Path.Combine(_collectionPath, key + ".json");
            string json = JsonSerializer.Serialize(record);
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }

        public override async Task UpsertAsync(IEnumerable<TRecord> records, CancellationToken cancellationToken = default)
        {
            foreach (var record in records)
                await UpsertAsync(record, cancellationToken);
        }

        public override async Task DeleteAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await base.DeleteAsync(key, cancellationToken);
            string filePath = Path.Combine(_collectionPath, key.ToString() + ".json");
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private class CollectionMeta
        {
            public string KeyType { get; set; }
            public string RecordType { get; set; }
        }
    }
}
