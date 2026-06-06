using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Dreamy.DataConfig
{
    public static class DataConfigJson
    {
        private static readonly JsonSerializerSettings SettingsInstance = CreateSettings();

        public static JsonSerializerSettings Settings => SettingsInstance;

        private static JsonSerializerSettings CreateSettings()
        {
            JsonSerializerSettings settings = new()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                MissingMemberHandling = MissingMemberHandling.Error,
                NullValueHandling = NullValueHandling.Include,
                ObjectCreationHandling = ObjectCreationHandling.Replace
            };

            settings.Converters.Add(new StringEnumConverter());
            return settings;
        }
    }
}
