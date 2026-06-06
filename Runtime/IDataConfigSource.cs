using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.DataConfig
{
    public interface IDataConfigSource
    {
        UniTask<string> LoadJsonAsync(
            string documentName,
            CancellationToken cancellationToken = default);
    }
}
