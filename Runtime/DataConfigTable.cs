using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dreamy.DataConfig
{
    [Serializable]
    public class DataConfigTable<TRow> : IDataConfigTable
        where TRow : DataConfigRow
    {
        [JsonProperty("schemaVersion")]
        private int schemaVersion = 1;

        [JsonProperty("rows", Required = Required.Always)]
        private List<TRow> rows = new();

        [JsonIgnore]
        private IReadOnlyDictionary<string, TRow> rowsById;

        [JsonIgnore]
        public int SchemaVersion => schemaVersion;

        public IReadOnlyList<TRow> GetAll()
        {
            return rows;
        }

        public TRow GetById(string id)
        {
            if (TryGetById(id, out TRow row))
            {
                return row;
            }

            throw new KeyNotFoundException(
                $"No row with id '{id}' exists in {GetType().Name}.");
        }

        public bool TryGetById(string id, out TRow row)
        {
            if (rowsById == null)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} has not been initialized.");
            }

            return rowsById.TryGetValue(id, out row);
        }

        public virtual void Initialize(string documentName)
        {
            Dictionary<string, TRow> index = new(StringComparer.Ordinal);

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                TRow row = rows[rowIndex];
                if (row == null)
                {
                    throw new DataConfigException(
                        documentName,
                        $"Row at index {rowIndex} is null.");
                }

                if (string.IsNullOrWhiteSpace(row.Id))
                {
                    throw new DataConfigException(
                        documentName,
                        $"Row at index {rowIndex} has an empty id.");
                }

                if (!index.TryAdd(row.Id, row))
                {
                    throw new DataConfigException(
                        documentName,
                        $"Duplicate row id '{row.Id}' at index {rowIndex}.");
                }
            }

            rowsById = index;
        }
    }
}
