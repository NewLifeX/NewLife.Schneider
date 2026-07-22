# NewLife.Schneider — シュナイダーPLCドライバライブラリ

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider** は、Modbus TCP プロトコルに基づくシュナイダーエレクトリック（Schneider Electric）PLC 通信ドライバライブラリです。NewLife IoT エコシステムのドライバコンポーネントの1つであり、ZeroIoT や IoTEdge などの上位 IoT プラットフォームに標準化されたシュナイダー PLC デバイスアクセスを提供します。開発者は統一された `IDriver` / `INode` インターフェースを通じてデバイス接続とデータ読み書きを行えます。

> **🌐 他の言語**: [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Français](README.fr.md) | [Deutsch](README.de.md) | [한국어](README.ko.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md) | [العربية](README.ar.md)

## ✨ ハイライト

- **すぐに使える** — NewLife.Modbus の Modbus TCP 実装に基づき、NuGet でインストールしてすぐに開始可能
- **自動検出** — `[Driver("SchneiderPLC")]` 属性で注釈され、IoT プラットフォームが自動的にドライバをスキャンしてロード
- **タグベースの読み書き** — シュナイダー固有のアドレス構文（`"MW100"`、`"M0.1"`、`"MF100"`）を使用して直接読み書き、Modbus プロトコルの詳細を意識する必要なし
- **自動型変換** — 読み取り時に Modbus ビッグエンディアンバイトを対応する .NET 型（UInt16/UInt32/Single/Boolean）に自動変換
- **統一インターフェース** — NewLife.IoT 標準抽象化に準拠し、IoT エコシステムとシームレスに統合
- **マルチフレームワーク** — netstandard2.1/2.0/net461/net45 をサポート、レガシー .NET Framework と最新 .NET アプリケーションの両方と互換性あり

## 🚀 クイックスタート

### インストール

```shell
dotnet add package NewLife.Schneider
```

### 使い方

#### 基本 Modbus TCP 読み書き

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// パラメータ設定
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// 接続を開く
var node = await driver.OpenAsync(null, pm);

// 保持レジスタ 100 を読み取り (UInt16 を返す)
var value = await driver.ReadTag(node, "MW100");

// 強い型付け読み取り：自動型変換
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// 保持レジスタ 100 に書き込み（.NET ネイティブ型をサポート）
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// バッチ読み取り
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// 接続を閉じる
await driver.CloseAsync(node);
```

#### シュナイダー固有のアドレス構文

```csharp
// アドレスパーサー（PLC に接続せずに使用可能）
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 保持レジスタ読み取り
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 コイル読み取り

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → 読み取り専用

// 安全な解析
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### モデル別プリセット設定

```csharp
// モデル別にプリセットパラメータを作成
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// または手動でモデル情報を照会
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} レジスタ");
```

## 📚 ドキュメント

| ドキュメント | 説明 |
|-------------|------|
| [要件定義 (中文)](Doc/需求文档.md) | ビジョン、核心目標、機能要件 |
| [機能一覧 (中文)](Doc/功能清单.md) | 3次元追跡：実装 / テスト / ドキュメント |
| [アーキテクチャ (中文)](Doc/架构设计.md) | 階層、コンポーネント、ワークフロー、主要な決定事項 |
| [競合分析 (中文)](Doc/竞品分析报告.md) | 比較マトリックスとギャップ分析 |
| [Modbus Plus ブリッジ (中文)](Doc/Modbus%20Plus%20桥接说明.md) | イーサネットモジュールブリッジ経由の Modbus Plus デバイスアクセス |

## 📦 依存関係

| パッケージ | 説明 |
|-----------|------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | IoT 標準抽象化（IDriver/INode インターフェース） |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | Modbus TCP/RTU/ASCII プロトコル実装 |

## 🔗 リンク

- **ソースコード**: https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**: https://www.nuget.org/packages/NewLife.Schneider
- **NewLife ウェブサイト**: https://newlifex.com
- **QQ グループ**: 1600800 / 1600838

## NewLife 開発チーム

2002 年に設立された NewLife チーム（新生命）は、IoT 業界向けのハードウェア/ソフトウェアアプリケーションコンサルティング、システムアーキテクチャ計画、開発サービスを提供しています。チームが主導する 80 以上のオープンソースプロジェクトは、電力、教育、インターネット、電気通信、交通、物流、産業制御、医療、文化遺産などの業界で広く採用され、NuGet ダウンロード数は 400 万を超えています。

> 他のプロジェクト: [NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [Stardust](https://github.com/NewLifeX/Stardust) | [AntJob](https://github.com/NewLifeX/AntJob) | [もっと見る](https://github.com/NewLifeX)
