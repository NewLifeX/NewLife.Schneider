# NewLife.Schneider — Bibliothèque de Pilote pour API Schneider

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider** est une bibliothèque de communication pour automates programmables (API) Schneider Electric basée sur le protocole Modbus TCP. C'est l'un des composants pilotes de l'écosystème NewLife IoT, fournissant un accès standardisé aux équipements Schneider PLC pour les plateformes IoT de niveau supérieur telles que ZeroIoT et IoTEdge. Les développeurs peuvent se connecter aux appareils et lire/écrire des données via les interfaces unifiées `IDriver` / `INode`.

> **🌐 Autres Langues**: [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md) | [العربية](README.ar.md)

## ✨ Points Forts

- **Prêt à l'Emploi** — Construit sur l'implémentation Modbus TCP de NewLife.Modbus, installez via NuGet et commencez immédiatement
- **Auto-Découverte** — Annoté avec `[Driver("SchneiderPLC")]`, les plateformes IoT peuvent scanner et charger automatiquement le pilote
- **Lecture/Écriture par Étiquettes** — Lisez et écrivez directement en utilisant la syntaxe d'adressage native Schneider (`"MW100"`, `"M0.1"`, `"MF100"`) sans vous soucier des détails du protocole Modbus
- **Conversion Automatique de Types** — Convertit automatiquement les octets big-endian Modbus en types .NET (UInt16/UInt32/Single/Boolean) à la lecture
- **Interface Unifiée** — Suit les abstractions standard NewLife.IoT, s'intègre parfaitement avec l'écosystème IoT
- **Multi-Cadres** — Prend en charge netstandard2.1/2.0/net461/net45, compatible avec les anciens systèmes .NET Framework et les applications .NET modernes

## 🚀 Démarrage Rapide

### Installation

```shell
dotnet add package NewLife.Schneider
```

### Utilisation

#### Lecture/Écriture Modbus TCP de Base

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// Configurer les paramètres
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// Ouvrir la connexion
var node = await driver.OpenAsync(null, pm);

// Lire le registre de maintien 100 (retourne UInt16)
var value = await driver.ReadTag(node, "MW100");

// Lecture fortement typée : conversion automatique
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// Écrire le registre de maintien 100 (prend en charge les types natifs .NET)
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// Lecture par lots
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// Fermer la connexion
await driver.CloseAsync(node);
```

#### Syntaxe d'Adressage Spécifique Schneider

```csharp
// Analyseur d'adresses (peut être utilisé sans connexion à un API)
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 Lire Registres de Maintien
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 Lire Bobines

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → Lecture seule

// Analyse sécurisée
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### Configuration Prédéfinie par Modèle

```csharp
// Créer des paramètres prédéfinis par modèle
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// Ou consulter les informations du modèle manuellement
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} registres");
```

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [Exigences (中文)](Doc/需求文档.md) | Vision, objectifs principaux et exigences fonctionnelles |
| [Liste des Fonctionnalités (中文)](Doc/功能清单.md) | Suivi 3D : implémentation / test / documentation |
| [Architecture (中文)](Doc/架构设计.md) | Couches, composants, flux de travail et décisions clés |
| [Analyse Concurrentielle (中文)](Doc/竞品分析报告.md) | Matrice de comparaison et analyse des écarts |
| [Pont Modbus Plus (中文)](Doc/Modbus%20Plus%20桥接说明.md) | Accès aux appareils Modbus Plus via pont de module Ethernet |

## 📦 Dépendances

| Paquet | Description |
|--------|-------------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | Abstractions standard IoT (interfaces IDriver/INode) |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | Implémentation du protocole Modbus TCP/RTU/ASCII |

## 🔗 Liens

- **Code Source**: https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**: https://www.nuget.org/packages/NewLife.Schneider
- **Site Web NewLife**: https://newlifex.com
- **Groupes QQ**: 1600800 / 1600838

## Équipe de Développement NewLife

Fondée en 2002, l'équipe NewLife (新生命) est dédiée à fournir des services de conseil en applications matérielles/logicielles pour l'industrie IoT, de planification d'architecture système et de développement. Les plus de 80 projets open-source de l'équipe ont été largement adoptés dans les secteurs de l'énergie, de l'éducation, d'Internet, des télécommunications, des transports, de la logistique, du contrôle industriel, de la santé et du patrimoine culturel, avec plus de 4 millions de téléchargements NuGet.

> Autres projets : [NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [Stardust](https://github.com/NewLifeX/Stardust) | [AntJob](https://github.com/NewLifeX/AntJob) | [Plus](https://github.com/NewLifeX)
