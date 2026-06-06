using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.DataConfig
{
    public sealed class InMemoryConfigSource : IDataConfigSource
    {
        private readonly IReadOnlyDictionary<string, string> documents;

        public InMemoryConfigSource(IReadOnlyDictionary<string, string> documents)
        {
            this.documents = documents
                ?? throw new ArgumentNullException(nameof(documents));
        }

        public UniTask<string> LoadJsonAsync(
            string documentName,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!documents.TryGetValue(documentName, out string json))
            {
                throw new DataConfigDocumentNotFoundException(
                    documentName,
                    "The document does not exist in the in-memory source.");
            }

            return UniTask.FromResult(json);
        }
    }
}
