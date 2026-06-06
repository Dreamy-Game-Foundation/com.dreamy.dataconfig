using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dreamy.DataConfig
{
    public interface IDataConfigService
    {
        bool IsInitialized { get; }

        void Register<TTable>(string documentName)
            where TTable : class, IDataConfigTable;

        UniTask InitializeAsync(CancellationToken cancellationToken = default);

        TTable GetTable<TTable>()
            where TTable : class, IDataConfigTable;

        bool TryGetTable<TTable>(out TTable table)
            where TTable : class, IDataConfigTable;

        UniTask ReloadAsync(CancellationToken cancellationToken = default);
    }
}
