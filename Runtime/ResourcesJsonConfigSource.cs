using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.DataConfig
{
    public sealed class ResourcesJsonConfigSource : IDataConfigSource
    {
        private const string DefaultRootPath = "DataConfig";

        private readonly string rootPath;

        public ResourcesJsonConfigSource(string rootPath = DefaultRootPath)
        {
            this.rootPath = rootPath?.Trim().Trim('/') ?? string.Empty;
        }

        public async UniTask<string> LoadJsonAsync(
            string documentName,
            CancellationToken cancellationToken = default)
        {
            string resourcePath = string.IsNullOrEmpty(rootPath)
                ? documentName
                : $"{rootPath}/{documentName}";

            ResourceRequest request = Resources.LoadAsync<TextAsset>(resourcePath);
            TextAsset asset = await request
                .ToUniTask(cancellationToken: cancellationToken) as TextAsset;

            if (!asset)
            {
                throw new DataConfigDocumentNotFoundException(
                    documentName,
                    $"TextAsset was not found at Resources/{resourcePath}.");
            }

            return asset.text;
        }
    }
}
