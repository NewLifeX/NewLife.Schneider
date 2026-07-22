# NewLife.Schneider — 施耐德 PLC 驱动库

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider** 是基于 Modbus TCP 协议的施耐德（Schneider Electric）PLC 通信驱动库，属于新生命 IoT 生态的驱动组件之一。为上层 IoT 平台（如 ZeroIoT、IoTEdge）提供标准化的施耐德 PLC 设备接入能力，开发者通过统一的 `IDriver` / `INode` 接口即可完成设备连接与数据读写。

> **🌐 其他语言**: [English](README.md) | [Español](README.es.md) | [Français](README.fr.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md) | [العربية](README.ar.md)

## ✨ 亮点

- **开箱即用** — 基于 NewLife.Modbus 的 Modbus TCP 实现，NuGet 安装即用
- **驱动自动发现** — 通过 `[Driver("SchneiderPLC")]` 特性标注，IoT 平台可自动扫描加载
- **标签式读写** — 使用施耐德原生寻址语法（`"MW100"`、`"M0.1"`、`"MF100"`）直接读写，无需关注 Modbus 协议细节
- **自动类型转换** — 读取时自动将 Modbus 大端序字节转换为对应 .NET 类型（UInt16/UInt32/Single/Boolean）
- **统一接口** — 遵循 NewLife.IoT 标准抽象，与 IoT 生态无缝集成
- **多框架支持** — 支持 netstandard2.1/2.0/net461/net45，兼容 .NET Framework 老旧系统与 .NET 现代应用

## 🚀 快速开始

### 安装

```shell
dotnet add package NewLife.Schneider
```

### 使用

#### 基础 Modbus TCP 读写

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// 配置参数
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// 打开连接
var node = await driver.OpenAsync(null, pm);

// 读取保持寄存器 100 (返回 UInt16)
var value = await driver.ReadTag(node, "MW100");

// 强类型读取：自动转换为目标类型
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// 写入保持寄存器 100（支持 .NET 原生类型）
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// 批量读取
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// 关闭连接
await driver.CloseAsync(node);
```

#### 施耐德专用寻址语法

```csharp
// 地址解析器（无需连接PLC即可使用）
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 读保持寄存器
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 读线圈

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → 只读

// 安全解析
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### 型号预设一键配置

```csharp
// 按型号创建预设参数
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// 或手动查型号信息
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} 寄存器");
```

## 📚 文档

| 文档 | 说明 |
|------|------|
| [需求文档](Doc/需求文档.md) | 愿景、核心目标与功能需求 |
| [功能清单](Doc/功能清单.md) | 功能实现/测试/注释三维状态追踪 |
| [架构设计](Doc/架构设计.md) | 分层、组件、流程与关键决策 |
| [竞品分析](Doc/竞品分析报告.md) | 竞品对比矩阵与差距分析 |
| [Modbus Plus 桥接说明](Doc/Modbus%20Plus%20桥接说明.md) | 通过以太网模块桥接访问 Modbus Plus 设备 |

## 📦 依赖

| 包名 | 说明 |
|------|------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | IoT 标准抽象（IDriver/INode 等接口） |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | Modbus TCP/RTU/ASCII 协议实现 |

## 🔗 链接

- **源码**：https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**：https://www.nuget.org/packages/NewLife.Schneider
- **新生命官网**：https://newlifex.com
- **QQ群**：1600800 / 1600838

## 新生命开发团队

新生命团队（NewLife）成立于 2002 年，致力于提供物联网行业软硬件应用方案咨询、系统架构规划与开发服务。团队主导的 80 多个开源项目已被广泛应用于电力、高校、互联网、电信、交通、物流、工控、医疗、文博等行业，NuGet 累计下载量 400 余万次。

> 团队其他项目：[NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [星尘 Stardust](https://github.com/NewLifeX/Stardust) | [蚂蚁调度 AntJob](https://github.com/NewLifeX/AntJob) | [更多](https://github.com/NewLifeX)
