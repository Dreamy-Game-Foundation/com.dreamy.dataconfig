using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.DataConfig
{
    public interface IDataConfigService
    {
        bool IsInitialized { get; }

        void Register<TTable>(string documentName)
            where TTable : ConfigBase;

        int RegisterAllConfigs();

        UniTask InitializeAsync(CancellationToken cancellationToken = default);

        TTable GetTable<TTable>()
            where TTable : ConfigBase;

        bool TryGetTable<TTable>(out TTable table)
            where TTable : ConfigBase;

        UniTask ReloadAsync(CancellationToken cancellationToken = default);
    }
}
