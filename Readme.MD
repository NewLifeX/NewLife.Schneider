# NewLife.Schneider — Schneider PLC Driver Library

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider** is a Schneider Electric PLC communication driver library based on the Modbus TCP protocol. It is one of the driver components in the NewLife IoT ecosystem, providing standardized Schneider PLC device access for upper-layer IoT platforms such as ZeroIoT and IoTEdge. Developers can connect to devices and read/write data through the unified `IDriver` / `INode` interfaces.

> **🌐 Other Languages**: [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Français](README.fr.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md) | [العربية](README.ar.md)

## ✨ Highlights

- **Ready to Use** — Built on NewLife.Modbus Modbus TCP implementation, install via NuGet and start immediately
- **Auto-Discovery** — Annotated with `[Driver("SchneiderPLC")]`, IoT platforms can automatically scan and load the driver
- **Tag-Based R/W** — Read and write directly using Schneider native addressing syntax (`"MW100"`, `"M0.1"`, `"MF100"`) without dealing with Modbus protocol details
- **Auto Type Conversion** — Automatically converts Modbus big-endian bytes to .NET types (UInt16/UInt32/Single/Boolean) on read
- **Unified Interface** — Follows NewLife.IoT standard abstractions, seamlessly integrates with the IoT ecosystem
- **Multi-Framework** — Supports netstandard2.1/2.0/net461/net45, compatible with both legacy .NET Framework and modern .NET applications

## 🚀 Quick Start

### Installation

```shell
dotnet add package NewLife.Schneider
```

### Usage

#### Basic Modbus TCP R/W

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// Configure parameters
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// Open connection
var node = await driver.OpenAsync(null, pm);

// Read holding register 100 (returns UInt16)
var value = await driver.ReadTag(node, "MW100");

// Strongly-typed read: auto-converts to target type
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// Write holding register 100 (supports .NET native types)
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// Batch read
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// Close connection
await driver.CloseAsync(node);
```

#### Schneider-Specific Addressing Syntax

```csharp
// Address parser (can be used without connecting to a PLC)
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 Read Holding Registers
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 Read Coils

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → Read-only

// Safe parsing
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### Preset Configuration by Model

```csharp
// Create preset parameters by model
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// Or query model information manually
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} registers");
```

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [Requirements (中文)](Doc/需求文档.md) | Vision, core objectives, and functional requirements |
| [Feature Checklist (中文)](Doc/功能清单.md) | 3D tracking: implementation / test / documentation |
| [Architecture (中文)](Doc/架构设计.md) | Layering, components, workflows, and key decisions |
| [Competitive Analysis (中文)](Doc/竞品分析报告.md) | Comparison matrix and gap analysis |
| [Modbus Plus Bridge (中文)](Doc/Modbus%20Plus%20桥接说明.md) | Accessing Modbus Plus devices via Ethernet module bridging |

## 📦 Dependencies

| Package | Description |
|---------|-------------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | IoT standard abstractions (IDriver/INode interfaces) |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | Modbus TCP/RTU/ASCII protocol implementation |

## 🔗 Links

- **Source Code**: https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**: https://www.nuget.org/packages/NewLife.Schneider
- **NewLife Website**: https://newlifex.com
- **QQ Groups**: 1600800 / 1600838

## NewLife Development Team

Founded in 2002, the NewLife team (新生命) is dedicated to providing IoT industry hardware/software application consulting, system architecture planning, and development services. The team's 80+ open-source projects have been widely adopted across power, education, internet, telecommunications, transportation, logistics, industrial control, healthcare, and cultural heritage industries, with over 4 million NuGet downloads.

> Other projects: [NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [Stardust](https://github.com/NewLifeX/Stardust) | [AntJob](https://github.com/NewLifeX/AntJob) | [More](https://github.com/NewLifeX)
