# NewLife.Schneider — Biblioteca de Driver para CLP Schneider

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider** é uma biblioteca de driver de comunicação para CLPs Schneider Electric baseada no protocolo Modbus TCP. É um dos componentes de driver do ecossistema NewLife IoT, fornecendo acesso padronizado a dispositivos Schneider PLC para plataformas IoT de camada superior, como ZeroIoT e IoTEdge. Desenvolvedores podem conectar-se a dispositivos e ler/escrever dados através das interfaces unificadas `IDriver` / `INode`.

> **🌐 Outros Idiomas**: [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Français](README.fr.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Русский](README.ru.md) | [العربية](README.ar.md)

## ✨ Destaques

- **Pronto para Usar** — Construído sobre a implementação Modbus TCP do NewLife.Modbus, instale via NuGet e comece imediatamente
- **Autodescoberta** — Anotado com `[Driver("SchneiderPLC")]`, plataformas IoT podem escanear e carregar o driver automaticamente
- **Leitura/Escrita por Tags** — Leia e escreva diretamente usando a sintaxe de endereçamento nativa Schneider (`"MW100"`, `"M0.1"`, `"MF100"`) sem lidar com detalhes do protocolo Modbus
- **Conversão Automática de Tipos** — Converte automaticamente bytes big-endian Modbus para tipos .NET (UInt16/UInt32/Single/Boolean) na leitura
- **Interface Unificada** — Segue as abstrações padrão NewLife.IoT, integra-se perfeitamente ao ecossistema IoT
- **Multi-Framework** — Suporta netstandard2.1/2.0/net461/net45, compatível com sistemas .NET Framework legados e aplicações .NET modernas

## 🚀 Início Rápido

### Instalação

```shell
dotnet add package NewLife.Schneider
```

### Uso

#### Leitura/Escrita Modbus TCP Básica

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// Configurar parâmetros
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// Abrir conexão
var node = await driver.OpenAsync(null, pm);

// Ler registrador de retenção 100 (retorna UInt16)
var value = await driver.ReadTag(node, "MW100");

// Leitura fortemente tipada: conversão automática
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// Escrever registrador de retenção 100 (suporta tipos nativos .NET)
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// Leitura em lote
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// Fechar conexão
await driver.CloseAsync(node);
```

#### Sintaxe de Endereçamento Específica Schneider

```csharp
// Analisador de endereços (pode ser usado sem conectar a um CLP)
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 Ler Registradores de Retenção
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 Ler Bobinas

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → Somente leitura

// Análise segura
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### Configuração Predefinida por Modelo

```csharp
// Criar parâmetros predefinidos por modelo
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// Ou consultar informações do modelo manualmente
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} registradores");
```

## 📚 Documentação

| Documento | Descrição |
|-----------|-------------|
| [Requisitos (中文)](Doc/需求文档.md) | Visão, objetivos principais e requisitos funcionais |
| [Lista de Funcionalidades (中文)](Doc/功能清单.md) | Acompanhamento 3D: implementação / teste / documentação |
| [Arquitetura (中文)](Doc/架构设计.md) | Camadas, componentes, fluxos de trabalho e decisões principais |
| [Análise Concorrencial (中文)](Doc/竞品分析报告.md) | Matriz de comparação e análise de lacunas |
| [Ponte Modbus Plus (中文)](Doc/Modbus%20Plus%20桥接说明.md) | Acesso a dispositivos Modbus Plus via ponte de módulo Ethernet |

## 📦 Dependências

| Pacote | Descrição |
|--------|-------------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | Abstrações padrão IoT (interfaces IDriver/INode) |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | Implementação do protocolo Modbus TCP/RTU/ASCII |

## 🔗 Links

- **Código Fonte**: https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**: https://www.nuget.org/packages/NewLife.Schneider
- **Site NewLife**: https://newlifex.com
- **Grupos QQ**: 1600800 / 1600838

## Equipe de Desenvolvimento NewLife

Fundada em 2002, a equipe NewLife (新生命) é dedicada a fornecer consultoria de aplicações de hardware/software para a indústria IoT, planejamento de arquitetura de sistemas e serviços de desenvolvimento. Mais de 80 projetos open-source da equipe foram amplamente adotados nos setores de energia, educação, internet, telecomunicações, transporte, logística, controle industrial, saúde e patrimônio cultural, com mais de 4 milhões de downloads no NuGet.

> Outros projetos: [NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [Stardust](https://github.com/NewLifeX/Stardust) | [AntJob](https://github.com/NewLifeX/AntJob) | [Mais](https://github.com/NewLifeX)
