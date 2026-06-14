# Dreamy Data Config

Typed, read-only game configuration loaded primarily from JSON.

Use `Tools/Dreamy/Data Config/Create Missing JSON` to scan concrete public
`ConfigBase` types and create missing files under
`Assets/Resources/DataConfig`. Existing JSON files are never overwritten.

## Setup

Install UniTask and Newtonsoft JSON in the consuming project. Each config
inherits `ConfigBase`. Use `DataConfigAttribute` when the JSON document name
should not be inferred from the class name:

```csharp
[DataConfig("items")]
public sealed class ItemConfig : DataConfigTable<ItemRow>
{
}

var source = DataConfigSources.CreateDefault(remoteProvider);
var service = new DataConfigService(source);
service.RegisterAllConfigs();
await service.InitializeAsync();

var item = service.GetTable<ItemConfig>().GetById("item_001");
```

The default source reads
`Assets/Resources/DataConfig/<documentName>.json`.

`remoteProvider` is optional and implements `IRemoteConfigProvider`. When it
returns JSON, the remote value wins. When it returns empty or throws, the
service falls back to the local Resources JSON.

Register the initialized `IDataConfigService` once in the project's
`GameInstaller`.

## Data ownership

- DataConfig: read-only design data such as items, levels, and balance.
- Datasave: writable player progress and settings.

Use `InMemoryConfigSource` for tests. Use `CompositeConfigSource` when a
later source should override an earlier source.

## Validation

Open `Tools/Dreamy/Data Config/Open Editor` to:

- Find JSON files across the project.
- Save favorite files for quick access.
- Edit raw JSON in Text view.
- Edit object properties or row data in Table view.
- Validate and save changes.

Run `Tools/Dreamy/Data Config/Validate All` for batch validation.
