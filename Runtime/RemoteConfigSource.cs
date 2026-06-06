using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.DataConfig
{
    public sealed class RemoteConfigSource : IDataConfigSource
    {
        private readonly IRemoteConfigProvider provider;
        private readonly bool fallbackOnError;

        public RemoteConfigSource(
            IRemoteConfigProvider provider,
            bool fallbackOnError = true)
        {
            this.provider = provider
                ?? throw new ArgumentNullException(nameof(provider));
            this.fallbackOnError = fallbackOnError;
        }

        public async UniTask<string> LoadJsonAsync(
            string documentName,
            CancellationToken cancellationToken = default)
        {
            string json;
            try
            {
                json = await provider.FetchJsonAsync(
                    documentName,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception) when (fallbackOnError)
            {
                throw new DataConfigDocumentNotFoundException(
                    documentName,
                    $"Remote config failed: {exception.Message}");
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new DataConfigDocumentNotFoundException(
                    documentName,
                    "Remote config did not return a value.");
            }

            return json;
        }
    }
}
