# Dreamy Data Config

Typed, read-only game configuration loaded primarily from JSON.

## Setup

Install UniTask and Newtonsoft JSON in the consuming project. Register each
table with its JSON document name before initialization:

```csharp
var service = new DataConfigService(new ResourcesJsonConfigSource());
service.Register<ItemTable>("items");
await service.InitializeAsync();

var item = service.GetTable<ItemTable>().GetById("item_001");
```

The default source reads
`Assets/Resources/DataConfig/<documentName>.json`.

## Data ownership

- DataConfig: read-only design data such as items, levels, and balance.
- Datasave: writable player progress and settings.

Use `InMemoryConfigSource` for tests. Use `CompositeConfigSource` when a
later source should override an earlier source.

## Validation

Run `Tools/Dreamy/Data Config/Validate All` to validate JSON files under
`Assets/Resources/DataConfig`.
