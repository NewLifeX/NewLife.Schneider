# NewLife.Schneider — Biblioteca de Controladores para PLC Schneider

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider** es una biblioteca de comunicación para PLC Schneider Electric basada en el protocolo Modbus TCP. Es uno de los componentes controladores del ecosistema NewLife IoT, que proporciona acceso estandarizado a dispositivos Schneider PLC para plataformas IoT de capa superior como ZeroIoT e IoTEdge. Los desarrolladores pueden conectarse a dispositivos y leer/escribir datos a través de las interfaces unificadas `IDriver` / `INode`.

> **🌐 Otros Idiomas**: [English](README.md) | [简体中文](README.zh-CN.md) | [Français](README.fr.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md) | [العربية](README.ar.md)

## ✨ Destacados

- **Listo para Usar** — Construido sobre la implementación Modbus TCP de NewLife.Modbus, instálelo vía NuGet y empiece inmediatamente
- **Auto-Detección** — Anotado con `[Driver("SchneiderPLC")]`, las plataformas IoT pueden escanear y cargar el controlador automáticamente
- **Lectura/Escritura por Etiquetas** — Lea y escriba directamente usando la sintaxis de direccionamiento nativa de Schneider (`"MW100"`, `"M0.1"`, `"MF100"`) sin ocuparse de los detalles del protocolo Modbus
- **Conversión Automática de Tipos** — Convierte automáticamente bytes big-endian de Modbus a tipos .NET (UInt16/UInt32/Single/Boolean) al leer
- **Interfaz Unificada** — Sigue las abstracciones estándar de NewLife.IoT, se integra sin problemas con el ecosistema IoT
- **Multi-Plataforma** — Soporta netstandard2.1/2.0/net461/net45, compatible con sistemas .NET Framework heredados y aplicaciones .NET modernas

## 🚀 Inicio Rápido

### Instalación

```shell
dotnet add package NewLife.Schneider
```

### Uso

#### Lectura/Escritura Modbus TCP Básica

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// Configurar parámetros
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// Abrir conexión
var node = await driver.OpenAsync(null, pm);

// Leer registro de retención 100 (devuelve UInt16)
var value = await driver.ReadTag(node, "MW100");

// Lectura fuertemente tipada: conversión automática
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// Escribir registro de retención 100 (soporta tipos nativos .NET)
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// Lectura por lotes
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// Cerrar conexión
await driver.CloseAsync(node);
```

#### Sintaxis de Direccionamiento Específica de Schneider

```csharp
// Analizador de direcciones (se puede usar sin conectar a un PLC)
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 Leer Registros de Retención
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 Leer Bobinas

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → Solo lectura

// Análisis seguro
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### Configuración Predefinida por Modelo

```csharp
// Crear parámetros predefinidos por modelo
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// O consultar información del modelo manualmente
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} registros");
```

## 📚 Documentación

| Documento | Descripción |
|-----------|-------------|
| [Requisitos (中文)](Doc/需求文档.md) | Visión, objetivos principales y requisitos funcionales |
| [Lista de Funciones (中文)](Doc/功能清单.md) | Seguimiento 3D: implementación / prueba / documentación |
| [Arquitectura (中文)](Doc/架构设计.md) | Capas, componentes, flujos de trabajo y decisiones clave |
| [Análisis Competitivo (中文)](Doc/竞品分析报告.md) | Matriz de comparación y análisis de brechas |
| [Puente Modbus Plus (中文)](Doc/Modbus%20Plus%20桥接说明.md) | Acceso a dispositivos Modbus Plus mediante puente de módulo Ethernet |

## 📦 Dependencias

| Paquete | Descripción |
|---------|-------------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | Abstracciones estándar de IoT (interfaces IDriver/INode) |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | Implementación del protocolo Modbus TCP/RTU/ASCII |

## 🔗 Enlaces

- **Código Fuente**: https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**: https://www.nuget.org/packages/NewLife.Schneider
- **Sitio Web NewLife**: https://newlifex.com
- **Grupos QQ**: 1600800 / 1600838

## Equipo de Desarrollo NewLife

Fundado en 2002, el equipo NewLife (新生命) está dedicado a proporcionar consultoría de aplicaciones de hardware/software para la industria IoT, planificación de arquitectura de sistemas y servicios de desarrollo. Más de 80 proyectos de código abierto del equipo han sido ampliamente adoptados en las industrias de energía, educación, internet, telecomunicaciones, transporte, logística, control industrial, salud y patrimonio cultural, con más de 4 millones de descargas en NuGet.

> Otros proyectos: [NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [Stardust](https://github.com/NewLifeX/Stardust) | [AntJob](https://github.com/NewLifeX/AntJob) | [Más](https://github.com/NewLifeX)
