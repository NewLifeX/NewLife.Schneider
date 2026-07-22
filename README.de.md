# NewLife.Schneider — Schneider PLC-Treiberbibliothek

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider** ist eine auf dem Modbus TCP-Protokoll basierende Kommunikationstreiberbibliothek für Schneider Electric SPS. Sie ist eine der Treiberkomponenten im NewLife IoT-Ökosystem und bietet standardisierten Zugriff auf Schneider SPS-Geräte für übergeordnete IoT-Plattformen wie ZeroIoT und IoTEdge. Entwickler können über die einheitlichen `IDriver` / `INode`-Schnittstellen auf Geräte zugreifen und Daten lesen/schreiben.

> **🌐 Andere Sprachen**: [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Français](README.fr.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md) | [العربية](README.ar.md)

## ✨ Highlights

- **Sofort Einsatzbereit** — Basierend auf der Modbus TCP-Implementierung von NewLife.Modbus, per NuGet installieren und sofort loslegen
- **Auto-Erkennung** — Mit `[Driver("SchneiderPLC")]` annotiert, IoT-Plattformen können den Treiber automatisch scannen und laden
- **Markenbasierte L/S** — Direktes Lesen und Schreiben mit der nativen Schneider-Adressierungssyntax (`"MW100"`, `"M0.1"`, `"MF100"`) ohne Modbus-Protokolldetails
- **Automatische Typkonvertierung** — Konvertiert automatisch Modbus Big-Endian-Bytes in .NET-Typen (UInt16/UInt32/Single/Boolean) beim Lesen
- **Einheitliche Schnittstelle** — Folgt den NewLife.IoT-Standardabstraktionen, nahtlose Integration in das IoT-Ökosystem
- **Multi-Framework** — Unterstützt netstandard2.1/2.0/net461/net45, kompatibel mit Legacy-.NET Framework und modernen .NET-Anwendungen

## 🚀 Schnellstart

### Installation

```shell
dotnet add package NewLife.Schneider
```

### Verwendung

#### Basis Modbus TCP L/S

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// Parameter konfigurieren
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// Verbindung öffnen
var node = await driver.OpenAsync(null, pm);

// Halteregister 100 lesen (gibt UInt16 zurück)
var value = await driver.ReadTag(node, "MW100");

// Stark typisiertes Lesen: automatische Konvertierung
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// Halteregister 100 schreiben (unterstützt .NET-native Typen)
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// Stapelverarbeitung lesen
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// Verbindung schließen
await driver.CloseAsync(node);
```

#### Schneider-spezifische Adressierungssyntax

```csharp
// Adressparser (kann ohne Verbindung zu einer SPS verwendet werden)
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 Halteregister lesen
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 Spulen lesen

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → Schreibgeschützt

// Sichere Analyse
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### Voreingestellte Konfiguration nach Modell

```csharp
// Voreingestellte Parameter nach Modell erstellen
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// Oder Modellinformationen manuell abfragen
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} Register");
```

## 📚 Dokumentation

| Dokument | Beschreibung |
|----------|-------------|
| [Anforderungen (中文)](Doc/需求文档.md) | Vision, Kernziele und funktionale Anforderungen |
| [Funktionsliste (中文)](Doc/功能清单.md) | 3D-Tracking: Implementierung / Test / Dokumentation |
| [Architektur (中文)](Doc/架构设计.md) | Schichten, Komponenten, Arbeitsabläufe und wichtige Entscheidungen |
| [Wettbewerbsanalyse (中文)](Doc/竞品分析报告.md) | Vergleichsmatrix und Lückenanalyse |
| [Modbus Plus Brücke (中文)](Doc/Modbus%20Plus%20桥接说明.md) | Zugriff auf Modbus Plus-Geräte über Ethernet-Modul-Brücke |

## 📦 Abhängigkeiten

| Paket | Beschreibung |
|-------|-------------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | IoT-Standardabstraktionen (IDriver/INode-Schnittstellen) |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | Modbus TCP/RTU/ASCII-Protokollimplementierung |

## 🔗 Links

- **Quellcode**: https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**: https://www.nuget.org/packages/NewLife.Schneider
- **NewLife-Website**: https://newlifex.com
- **QQ-Gruppen**: 1600800 / 1600838

## NewLife Entwicklungsteam

Das 2002 gegründete NewLife-Team (新生命) ist auf die Beratung zu Hardware-/Software-Anwendungen für die IoT-Branche, Systemarchitekturplanung und Entwicklungsdienstleistungen spezialisiert. Die über 80 Open-Source-Projekte des Teams wurden in den Bereichen Energie, Bildung, Internet, Telekommunikation, Transport, Logistik, industrielle Steuerung, Gesundheitswesen und Kulturerbe weit verbreitet eingesetzt, mit über 4 Millionen NuGet-Downloads.

> Weitere Projekte: [NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [Stardust](https://github.com/NewLifeX/Stardust) | [AntJob](https://github.com/NewLifeX/AntJob) | [Mehr](https://github.com/NewLifeX)
