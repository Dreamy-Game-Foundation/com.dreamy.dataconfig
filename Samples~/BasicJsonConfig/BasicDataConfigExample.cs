using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Dreamy.DataConfig.Samples
{
    public sealed class BasicDataConfigExample : MonoBehaviour
    {
        private async UniTaskVoid Start()
        {
            DataConfigService service =
                new(new ResourcesJsonConfigSource());
            service.Register<ItemTable>("items");

            await service.InitializeAsync(
                this.GetCancellationTokenOnDestroy());

            ItemConfig item = service
                .GetTable<ItemTable>()
                .GetById("item_001");
            Debug.Log($"Loaded item: {item.Name}, price: {item.Price}");
        }
    }
}
