using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.DataConfig
{
    public interface IRemoteConfigProvider
    {
        UniTask<string> FetchJsonAsync(
            string documentName,
            CancellationToken cancellationToken = default);
    }
}
