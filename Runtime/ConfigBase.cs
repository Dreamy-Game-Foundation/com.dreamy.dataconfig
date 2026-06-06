using System;
using Newtonsoft.Json;

namespace Dreamy.DataConfig
{
    [Serializable]
    public abstract class ConfigBase : IDataConfigTable
    {
        private const string ConfigSuffix = "Config";

        [JsonProperty("schemaVersion")]
        private int schemaVersion = 1;

        [JsonIgnore]
        public string DocumentName => ResolveDocumentName(GetType());

        [JsonIgnore]
        public int SchemaVersion => schemaVersion;

        public virtual void Initialize(string documentName)
        {
        }

        internal static string ResolveDocumentName(Type configType)
        {
            DataConfigAttribute attribute =
                Attribute.GetCustomAttribute(
                    configType,
                    typeof(DataConfigAttribute)) as DataConfigAttribute;

            if (attribute != null)
            {
                return attribute.DocumentName;
            }

            string typeName = configType.Name;
            if (typeName.EndsWith(ConfigSuffix, StringComparison.Ordinal))
            {
                typeName = typeName.Substring(
                    0,
                    typeName.Length - ConfigSuffix.Length);
            }

            return char.ToLowerInvariant(typeName[0]) + typeName.Substring(1);
        }
    }
}
