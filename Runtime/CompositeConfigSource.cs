using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.DataConfig
{
    public sealed class CompositeConfigSource : IDataConfigSource
    {
        private readonly IReadOnlyList<IDataConfigSource> sources;

        public CompositeConfigSource(
            IReadOnlyList<IDataConfigSource> sources)
        {
            if (sources == null || sources.Count == 0)
            {
                throw new ArgumentException(
                    "At least one config source is required.",
                    nameof(sources));
            }

            this.sources = sources;
        }

        public async UniTask<string> LoadJsonAsync(
            string documentName,
            CancellationToken cancellationToken = default)
        {
            for (int index = 0; index < sources.Count; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    return await sources[index].LoadJsonAsync(
                        documentName,
                        cancellationToken);
                }
                catch (DataConfigDocumentNotFoundException)
                {
                    // Continue to the next fallback source.
                }
            }

            throw new DataConfigDocumentNotFoundException(
                documentName,
                "The document was not found in any configured source.");
        }
    }
}
