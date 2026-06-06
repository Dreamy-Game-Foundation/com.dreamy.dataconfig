using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace Dreamy.DataConfig
{
    public sealed class DataConfigService : IDataConfigService
    {
        private readonly IDataConfigSource source;
        private readonly Dictionary<Type, string> registrations = new();
        private readonly Dictionary<Type, IDataConfigTable> tables = new();

        public DataConfigService(IDataConfigSource source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public bool IsInitialized { get; private set; }

        public void Register<TTable>(string documentName)
            where TTable : class, IDataConfigTable
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException(
                    "Tables cannot be registered after initialization.");
            }

            if (string.IsNullOrWhiteSpace(documentName))
            {
                throw new ArgumentException(
                    "Document name cannot be empty.",
                    nameof(documentName));
            }

            Type tableType = typeof(TTable);
            if (!registrations.TryAdd(tableType, documentName))
            {
                throw new InvalidOperationException(
                    $"{tableType.Name} is already registered.");
            }
        }

        public async UniTask InitializeAsync(
            CancellationToken cancellationToken = default)
        {
            if (IsInitialized)
            {
                return;
            }

            await LoadRegisteredTablesAsync(cancellationToken);
            IsInitialized = true;
        }

        public TTable GetTable<TTable>()
            where TTable : class, IDataConfigTable
        {
            EnsureInitialized();

            if (TryGetTable(out TTable table))
            {
                return table;
            }

            throw new KeyNotFoundException(
                $"Data config table {typeof(TTable).Name} is not registered.");
        }

        public bool TryGetTable<TTable>(out TTable table)
            where TTable : class, IDataConfigTable
        {
            EnsureInitialized();

            if (tables.TryGetValue(typeof(TTable), out IDataConfigTable value))
            {
                table = (TTable)value;
                return true;
            }

            table = null;
            return false;
        }

        public async UniTask ReloadAsync(
            CancellationToken cancellationToken = default)
        {
            EnsureInitialized();
            await LoadRegisteredTablesAsync(cancellationToken);
        }

        private async UniTask LoadRegisteredTablesAsync(
            CancellationToken cancellationToken)
        {
            Dictionary<Type, IDataConfigTable> loadedTables = new();

            foreach (KeyValuePair<Type, string> registration in registrations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string json = await source.LoadJsonAsync(
                    registration.Value,
                    cancellationToken);

                IDataConfigTable table = Deserialize(
                    registration.Key,
                    registration.Value,
                    json);
                loadedTables.Add(registration.Key, table);
            }

            tables.Clear();
            foreach (KeyValuePair<Type, IDataConfigTable> entry in loadedTables)
            {
                tables.Add(entry.Key, entry.Value);
            }
        }

        private static IDataConfigTable Deserialize(
            Type tableType,
            string documentName,
            string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new DataConfigException(documentName, "JSON is empty.");
            }

            try
            {
                object value = JsonConvert.DeserializeObject(
                    json,
                    tableType,
                    DataConfigJson.Settings);

                if (value is not IDataConfigTable table)
                {
                    throw new DataConfigException(
                        documentName,
                        $"JSON did not produce {tableType.Name}.");
                }

                table.Initialize(documentName);
                return table;
            }
            catch (DataConfigException)
            {
                throw;
            }
            catch (JsonException exception)
            {
                string exceptionPath = exception switch
                {
                    JsonReaderException readerException => readerException.Path,
                    JsonSerializationException serializationException =>
                        serializationException.Path,
                    _ => null
                };
                string path = string.IsNullOrEmpty(exceptionPath)
                    ? "<root>"
                    : exceptionPath;
                throw new DataConfigException(
                    documentName,
                    $"Invalid JSON at '{path}': {exception.Message}",
                    exception);
            }
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "DataConfigService must be initialized before querying tables.");
            }
        }
    }
}
