using System.Collections.Generic;

namespace Dreamy.DataConfig
{
    public static class DataConfigSources
    {
        public static IDataConfigSource CreateDefault(
            IRemoteConfigProvider remoteProvider = null,
            string resourcesRootPath = "DataConfig")
        {
            ResourcesJsonConfigSource localSource =
                new(resourcesRootPath);

            if (remoteProvider == null)
            {
                return localSource;
            }

            return new CompositeConfigSource(
                new List<IDataConfigSource>
                {
                    new RemoteConfigSource(remoteProvider),
                    localSource
                });
        }
    }
}
