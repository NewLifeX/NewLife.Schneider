# Modbus Plus 桥接说明

> 版本：v1.0 | 日期：2026-07-22 | 状态：理论性指导，未经实测

## 1. 概述

Modbus Plus（MB+）是 Modicon（现 Schneider Electric）在 1980 年代开发的**专有高速网络协议**，与标准 Modbus TCP/RTU/ASCII **不兼容**。

| 特性 | Modbus Plus |
|------|-------------|
| 通信模型 | 令牌传递（Token-Passing），对等网络（Peer-to-Peer） |
| 物理层 | 专用双绞线，1 Mbps |
| 硬件接口 | 专用硬件卡（SA85、AT-984、PCI-MBUS+ 等） |
| 协议栈 | 施耐德专有，与 Modbus TCP/RTU 完全不同 |

**NewLife.Schneider 不原生支持 Modbus Plus**（详见[需求文档](需求文档.md) §4 功能边界）。但通过施耐德以太网模块的桥接功能，可以将 MB+ 网络上的设备映射到 Modbus TCP，从而使用现有 NewLife.Schneider 驱动进行数据读写。

## 2. 桥接原理

```text
┌──────────────────────┐     Modbus TCP      ┌──────────────────────┐
│  PC / Edge Gateway   │ ◄──────────────► │  施耐德以太网模块     │
│  NewLife.Schneider   │    (标准以太网)    │  140 NOE 771 01 等   │
│  零额外硬件成本       │                     └───────┬──────────────┘
└──────────────────────┘                              │
                                                       │ Modbus Plus 网络
                                                       ▼
                                              ┌──────────────────────┐
                                              │  MB+ 设备群          │
                                              │  Quantum / Premium   │
                                              │  984 / Momentum      │
                                              └──────────────────────┘
```

桥接的工作原理：

1. **以太网模块**（如 140 NOE 771 01）同时具备 Modbus TCP 服务和 MB+ 接口
2. 模块被配置为**网关模式**，将 Modbus TCP 请求转发到 MB+ 网络上的目标设备
3. 上位机使用标准 Modbus TCP 协议与以太网模块通信，模块透明地完成协议转换
4. 对 NewLife.Schneider 而言，访问 MB+ 设备与访问本地 Modbus TCP 设备**无任何区别**

## 3. 支持桥接的常见施耐德模块

以下施耐德模块同时具备以太网和 MB+ 接口，可配置为桥接网关：

| 模块型号 | 适用平台 | 说明 |
|----------|----------|------|
| **140 NOE 771 01** | Quantum | 以太网 + Modbus Plus 双接口模块 |
| **140 NOE 771 11** | Quantum | 增强版，支持更多连接 |
| **TSX ETY 110** | Premium | 以太网模块，支持 MB+ 桥接 |
| **TSX ETY 4103** | Premium | 高级以太网模块 |
| **BMX NOE 0100** | M340 | 以太网模块（需确认 MB+ 桥接支持） |

> 以上列表基于公开资料整理，实际支持情况请以具体模块的官方手册为准。

## 4. 典型配置流程（理论性指导）

> ⚠️ 以下配置步骤为理论性指导，因缺少实测环境，具体步骤可能因模块型号和固件版本而异。请以施耐德官方手册为准。

### 4.1 先决条件

- 一台带有 MB+ 端口的施耐德 PLC（如 Quantum 140 CPU 434 12）
- 一个支持桥接的以太网模块（如 140 NOE 771 01）
- 以太网模块已正确安装到 PLC 机架并接入局域网
- PC 与以太网模块处于同一网络

### 4.2 以太网模块配置（通过 Unity Pro / EcoStruxure Control Expert）

1. 在 Unity Pro/Control Expert 中打开项目
2. 在本地机架中添加以太网模块（如 140 NOE 771 01）
3. 配置以太网模块的 IP 地址、子网掩码和网关
4. 在模块参数中启用 **Modbus TCP 到 Modbus Plus 的桥接/网关功能**
5. 配置 MB+ 路由表：指定哪些 MB+ 节点地址可以通过此模块访问
6. 将配置下载到 PLC

### 4.3 上位机连接

完成桥接配置后，使用 NewLife.Schneider 连接 PLC：

```csharp
var driver = new SchneiderDriver();
var parameter = new SchneiderParameter
{
    Host = 1,                                     // 目标 MB+ 节点地址
    Server = "192.168.1.100:502",                 // 以太网模块的 IP:Port
    ReadCode = FunctionCodes.ReadHoldingRegister, // FC03
    WriteCode = FunctionCodes.WriteMultipleRegisters, // FC16
};
var node = driver.Open(null, parameter);

// 读取 MB+ 网络上的设备数据
var value = await driver.ReadTag(node, "MW100");
Console.WriteLine(value);
```

> **关键点**：`Host` 属性应设置为 MB+ 网络上的目标节点地址（Peer ID），而非 Modbus TCP 站号。以太网模块根据 `Host` 值将请求路由到正确的 MB+ 设备。

## 5. 注意事项与限制

### 5.1 性能

- MB+ 网络本身为 1 Mbps，桥接后性能受限于 MB+ 网络带宽
- 每个 Modbus TCP 请求需经过协议转换，延迟会比直接访问 Modbus TCP 设备略高
- 避免高频率轮询（建议 > 100ms 间隔）

### 5.2 寻址

- MB+ 节点的地址范围通常为 1~64，具体取决于网络配置
- 施耐德原生地址（`%MW`、`%M` 等）在桥接后的映射行为取决于以太网模块的实现
- 建议先通过手动测试验证地址映射是否正确

### 5.3 兼容性

- **未经全部模块型号验证**，不同固件版本的行为可能有差异
- 部分旧型号以太网模块可能不支持桥接功能，请查阅具体模块的数据手册
- 老旧 MB+ 设备（如 984 系列）的部分专有功能可能无法通过桥接访问

### 5.4 安全性

- MB+ 网络本身没有认证机制
- 桥接后 MB+ 网络暴露在以太网上，建议通过 VLAN 或防火墙隔离

## 6. 故障排查

| 现象 | 可能原因 | 排查方法 |
|------|----------|----------|
| 连接超时 | IP 地址/端口配置错误 | 使用 `ping` 测试以太网模块连通性 |
| 请求无响应 | Host 地址不正确 | 检查 `Host` 是否等于目标 MB+ 节点地址 |
| 读取到异常值 | 功能码不匹配 | 尝试更换读功能码（FC03/FC04） |
| 写操作失败 | MB+ 设备不支持写操作 | 确认目标设备地址类型为可写（%MW 等） |

## 7. 相关资源

- [Schneider Electric - Modbus Plus 网络安装指南](https://www.se.com)
- [140 NOE 771 01 产品手册](https://www.se.com)
- [NewLife.Schneider 需求文档](需求文档.md)
- [NewLife.Schneider GitHub 仓库](https://github.com/NewLifeX/NewLife.Schneider)

---

> **免责声明**：本文档为理论性指导，未经实际 Modbus Plus 环境验证。具体配置步骤请以施耐德官方手册为准。
