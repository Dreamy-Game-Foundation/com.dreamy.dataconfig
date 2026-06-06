using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dreamy.DataConfig.Tests
{
    public sealed class DataConfigServiceTests
    {
        private const string ValidJson =
            "{\"schemaVersion\":1,\"rows\":[{\"id\":\"item_1\",\"price\":10}]}";
        private const string DuplicateJson =
            "{\"schemaVersion\":1,\"rows\":[{\"id\":\"item_1\"},{\"id\":\"item_1\"}]}";

        [Test]
        public async Task InitializeAsync_ValidJson_IndexesRows()
        {
            DataConfigService service = CreateService(ValidJson);
            service.Register<ItemTable>("items");

            await service.InitializeAsync();

            Assert.That(service.GetTable<ItemTable>().GetById("item_1").Price,
                Is.EqualTo(10));
        }

        [Test]
        public void InitializeAsync_DuplicateId_Throws()
        {
            DataConfigService service = CreateService(DuplicateJson);
            service.Register<ItemTable>("items");

            Assert.ThrowsAsync<DataConfigException>(
                async () => await service.InitializeAsync());
        }

        private static DataConfigService CreateService(string json)
        {
            Dictionary<string, string> documents = new()
            {
                ["items"] = json
            };
            return new DataConfigService(new InMemoryConfigSource(documents));
        }

        private sealed class ItemTable : DataConfigTable<ItemRow>
        {
        }

        private sealed class ItemRow : DataConfigRow
        {
            public int Price { get; set; }
        }
    }
}
