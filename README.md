# Dreamy Data Config

Package thuộc Dreamy Game Studio. Hướng dẫn dưới đây mô tả cấu trúc, cách cài vào project và tích hợp ở root/scene.

## Cài package

Dùng Unity 6000.0 trở lên. Sandbox đã tham chiếu package bằng `file:../LocalPackages/com.dreamy.dataconfig`. Project khác dùng Package Manager > + > Install package from disk và chọn package.json, hoặc Git URL của repository nội bộ. Cài cả dependency Dreamy/Git vào manifest của game; version dependency không tự cấu hình registry riêng.

Dependency trực tiếp theo package.json:

- `com.unity.nuget.newtonsoft-json` (3.2.1)

## Cấu trúc và asmdef

| Assembly | Reference | Phạm vi |
| --- | --- | --- |
| `Dreamy.DataConfig.Editor` | Dreamy.DataConfig.Runtime, Unity.Newtonsoft.Json | Chỉ Editor |
| `Dreamy.DataConfig.Runtime` | UniTask, Unity.Newtonsoft.Json | Runtime |

Trong asmdef của game, thêm assembly chứa API trực tiếp sử dụng. Code bootstrap reference thêm Core/DataConfig/Datasave/Economy theo nhu cầu; code async reference UniTask. Code gọi type sample reference assembly sample. Giữ Editor reference trong asmdef Editor-only.

## Cấu trúc và dữ liệu

Runtime chứa ConfigBase, DataConfigTable, DataConfigService và các nguồn JSON. Editor cung cấp trình sửa/validate. DataConfig giữ dữ liệu thiết kế chỉ đọc; tiến trình người chơi nằm trong Datasave.

JSON mặc định ở Assets/Resources/DataConfig/<documentName>.json. Mỗi document chỉ có một bản theo đường dẫn Resources. Config kế thừa ConfigBase; table có thể kế thừa DataConfigTable<T>. Dùng DataConfigAttribute để đặt tên document hoặc Register<T>(documentName) ở root.

## Khởi tạo ở GameInstaller

```csharp
using Dreamy.DataConfig;
using Dreamy.Core;

var dataConfig = new DataConfigService(new ResourcesJsonConfigSource());
// Đăng ký config của game/feature trước khi initialize.
ShopInstaller.RegisterConfig(dataConfig); // Chỉ khi đã cài Dreamy Shop.
await dataConfig.InitializeAsync(cancellationToken);
ServiceLocator.Register<IDataConfigService>(dataConfig);
```

Đoạn này chạy trong method async UniTask; cancellationToken thuộc root. Dùng Dreamy.Shop cho ShopInstaller nếu lấy ví dụ trên. Với nhiều feature, gọi tất cả RegisterConfig trước cùng một InitializeAsync, rồi cài các service feature sau đó. Không tạo lại DataConfig trong từng panel.

DataConfigSources.CreateDefault(remoteProvider) hỗ trợ nguồn remote và fallback JSON local. CompositeConfigSource dùng để ghép nguồn; nguồn sau có thể ghi đè nguồn trước. InMemoryConfigSource phù hợp cho fixture.

Asmdef Runtime hiện reference UniTask, nên project phải cài UniTask dù manifest package chưa khai báo nó. Code bootstrap dùng Core cần reference Dreamy.Core.Runtime.

## Công cụ Editor

Tools/Dreamy/Data Config/Create Missing JSON tạo file thiếu, không ghi đè file có sẵn. Open Editor cho phép xem Text/Table, sửa và validate JSON. Chạy Validate All sau khi sửa catalog. Root unregister IDataConfigService khi kết thúc lifecycle sở hữu.
## Sample

Manifest hiện không khai báo sample để import qua Package Manager.

## Addressables

Package này không có panel cần đăng ký vào Addressables Group. Việc đặt address của prefab/asset thuộc game hoặc package UI/Assets; không dùng Addressables thay bước đăng ký service/config/save.
