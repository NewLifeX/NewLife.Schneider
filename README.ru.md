# NewLife.Schneider — Библиотека драйверов для PLC Schneider

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider** — это библиотека драйверов связи для программируемых логических контроллеров (PLC) Schneider Electric, основанная на протоколе Modbus TCP. Это один из компонентов драйверов экосистемы NewLife IoT, предоставляющий стандартизированный доступ к устройствам Schneider PLC для вышестоящих IoT-платформ, таких как ZeroIoT и IoTEdge. Разработчики могут подключаться к устройствам и читать/записывать данные через унифицированные интерфейсы `IDriver` / `INode`.

> **🌐 Другие языки**: [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Français](README.fr.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Português (BR)](README.pt-BR.md) | [العربية](README.ar.md)

## ✨ Ключевые особенности

- **Готов к использованию** — Построен на реализации Modbus TCP от NewLife.Modbus, установите через NuGet и начинайте сразу
- **Автообнаружение** — Аннотирован атрибутом `[Driver("SchneiderPLC")]`, IoT-платформы могут автоматически сканировать и загружать драйвер
- **Чтение/запись по тегам** — Читайте и записывайте напрямую, используя собственный синтаксис адресации Schneider (`"MW100"`, `"M0.1"`, `"MF100"`), без необходимости вникать в детали протокола Modbus
- **Автоматическое преобразование типов** — Автоматически преобразует big-endian байты Modbus в соответствующие типы .NET (UInt16/UInt32/Single/Boolean) при чтении
- **Единый интерфейс** — Следует стандартным абстракциям NewLife.IoT, бесшовно интегрируется с экосистемой IoT
- **Мультиплатформенность** — Поддерживает netstandard2.1/2.0/net461/net45, совместим как с устаревшими системами .NET Framework, так и с современными .NET-приложениями

## 🚀 Быстрый старт

### Установка

```shell
dotnet add package NewLife.Schneider
```

### Использование

#### Базовая работа с Modbus TCP

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// Настройка параметров
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// Открытие соединения
var node = await driver.OpenAsync(null, pm);

// Чтение регистра хранения 100 (возвращает UInt16)
var value = await driver.ReadTag(node, "MW100");

// Строго типизированное чтение: автоматическое преобразование
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// Запись регистра хранения 100 (поддерживает родные типы .NET)
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// Пакетное чтение
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// Закрытие соединения
await driver.CloseAsync(node);
```

#### Специфический синтаксис адресации Schneider

```csharp
// Парсер адресов (можно использовать без подключения к PLC)
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 Чтение регистров хранения
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 Чтение катушек

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → Только чтение

// Безопасный парсинг
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### Предустановленная конфигурация по модели

```csharp
// Создание предустановленных параметров по модели
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// Или ручной запрос информации о модели
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} регистров");
```

## 📚 Документация

| Документ | Описание |
|----------|-------------|
| [Требования (中文)](Doc/需求文档.md) | Видение, основные цели и функциональные требования |
| [Список функций (中文)](Doc/功能清单.md) | 3D-отслеживание: реализация / тест / документация |
| [Архитектура (中文)](Doc/架构设计.md) | Уровни, компоненты, рабочие процессы и ключевые решения |
| [Конкурентный анализ (中文)](Doc/竞品分析报告.md) | Матрица сравнения и анализ пробелов |
| [Мост Modbus Plus (中文)](Doc/Modbus%20Plus%20桥接说明.md) | Доступ к устройствам Modbus Plus через мост Ethernet-модуля |

## 📦 Зависимости

| Пакет | Описание |
|-------|-------------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | Стандартные абстракции IoT (интерфейсы IDriver/INode) |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | Реализация протокола Modbus TCP/RTU/ASCII |

## 🔗 Ссылки

- **Исходный код**: https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**: https://www.nuget.org/packages/NewLife.Schneider
- **Веб-сайт NewLife**: https://newlifex.com
- **Группы QQ**: 1600800 / 1600838

## Команда разработчиков NewLife

Основанная в 2002 году, команда NewLife (新生命) занимается предоставлением консультационных услуг по аппаратно-программным приложениям для IoT-индустрии, планированию системной архитектуры и разработке. Более 80 проектов с открытым исходным кодом, созданных командой, широко используются в энергетике, образовании, интернете, телекоммуникациях, транспорте, логистике, промышленной автоматизации, здравоохранении и культурном наследии, с более чем 4 миллионами загрузок на NuGet.

> Другие проекты: [NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [Stardust](https://github.com/NewLifeX/Stardust) | [AntJob](https://github.com/NewLifeX/AntJob) | [Ещё](https://github.com/NewLifeX)
