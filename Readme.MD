# NewLife.Schneider — 施耐德 PLC 驱动库

![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)
![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)
![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)
![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)

**NewLife.Schneider** 是基于 Modbus TCP 协议的施耐德（Schneider Electric）PLC 通信驱动库，属于新生命 IoT 生态的驱动组件之一。为上层 IoT 平台（如 ZeroIoT、IoTEdge）提供标准化的施耐德 PLC 设备接入能力，开发者通过统一的 `IDriver` / `INode` 接口即可完成设备连接与数据读写。

## ✨ 亮点

- **开箱即用** — 基于 NewLife.Modbus 的 Modbus TCP 实现，NuGet 安装即用
- **驱动自动发现** — 通过 `[Driver("SchneiderPLC")]` 特性标注，IoT 平台可自动扫描加载
- **统一接口** — 遵循 NewLife.IoT 标准抽象，与 IoT 生态无缝集成
- **多框架支持** — 支持 netstandard2.1/2.0/net461/net45，兼容 .NET Framework 老旧系统与 .NET 现代应用

## 🚀 快速开始

### 安装

```shell
dotnet add package NewLife.Schneider
```

### 使用

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;

// 创建驱动
var driver = new SchneiderDriver();

// 配置参数
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
    ReadCode = FunctionCodes.ReadCoil,
    WriteCode = FunctionCodes.WriteCoil
};

// 打开连接
var node = driver.Open(null, pm);

// 读取数据
var point = new Point
{
    Name = "test",
    Address = "100",
    Type = "UInt16",
    Length = 2
};
var dic = driver.Read(node, new[] { point });

// 写入数据
var data = BitConverter.GetBytes(12345);
driver.Write(node, point, data);

// 关闭连接
driver.Close(node);
```

## 📚 文档

| 文档 | 说明 |
|------|------|
| [需求文档](Doc/需求文档.md) | 愿景、核心目标与功能需求 |
| [功能清单](Doc/功能清单.md) | 功能实现/测试/注释三维状态追踪 |
| [架构设计](Doc/架构设计.md) | 分层、组件、流程与关键决策 |

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
