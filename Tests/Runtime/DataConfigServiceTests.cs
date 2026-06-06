using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace Dreamy.DataConfig.Tests
{
    public sealed class DataConfigServiceTests
    {
        private const string ValidJson =
            "{\"schemaVersion\":1,\"rows\":[{\"id\":\"item_1\",\"price\":10}]}";
        private const string DuplicateJson =
            "{\"schemaVersion\":1,\"rows\":[{\"id\":\"item_1\"},{\"id\":\"item_1\"}]}";
        private const string LocalJson =
            "{\"schemaVersion\":1,\"rows\":[{\"id\":\"item_1\",\"price\":10}]}";
        private const string RemoteJson =
            "{\"schemaVersion\":1,\"rows\":[{\"id\":\"item_1\",\"price\":20}]}";

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

        [Test]
        public async Task CompositeSource_RemoteExists_UsesRemote()
        {
            IDataConfigSource source = new CompositeConfigSource(
                new IDataConfigSource[]
                {
                    new InMemoryConfigSource(
                        new Dictionary<string, string>
                        {
                            ["items"] = RemoteJson
                        }),
                    new InMemoryConfigSource(
                        new Dictionary<string, string>
                        {
                            ["items"] = LocalJson
                        })
                });
            DataConfigService service = new(source);
            service.Register<ItemTable>("items");

            await service.InitializeAsync();

            Assert.That(
                service.GetTable<ItemTable>().GetById("item_1").Price,
                Is.EqualTo(20));
        }

        [Test]
        public async Task RemoteSource_MissingValue_FallsBackToLocal()
        {
            DataConfigService service = new(
                new CompositeConfigSource(
                    new IDataConfigSource[]
                    {
                        new RemoteConfigSource(new EmptyRemoteProvider()),
                        new InMemoryConfigSource(
                            new Dictionary<string, string>
                            {
                                ["items"] = LocalJson
                            })
                    }));
            service.Register<ItemTable>("items");

            await service.InitializeAsync();

            Assert.That(
                service.GetTable<ItemTable>().GetById("item_1").Price,
                Is.EqualTo(10));
        }

        [Test]
        public void ConfigBase_Attribute_ResolvesDocumentName()
        {
            Assert.That(
                new ItemTable().DocumentName,
                Is.EqualTo("items"));
        }

        private static DataConfigService CreateService(string json)
        {
            Dictionary<string, string> documents = new()
            {
                ["items"] = json
            };
            return new DataConfigService(new InMemoryConfigSource(documents));
        }

        [DataConfig("items")]
        private sealed class ItemTable : DataConfigTable<ItemRow>
        {
        }

        private sealed class ItemRow : DataConfigRow
        {
            public int Price { get; set; }
        }

        private sealed class EmptyRemoteProvider : IRemoteConfigProvider
        {
            public UniTask<string> FetchJsonAsync(
                string documentName,
                CancellationToken cancellationToken = default)
            {
                return UniTask.FromResult<string>(null);
            }
        }
    }
}
