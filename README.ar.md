# NewLife.Schneider — مكتبة مشغل PLC شنايدر

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider** هي مكتبة مشغل اتصالات لـ PLC شنايدر إلكتريك (Schneider Electric) مبنية على بروتوكول Modbus TCP. وهي أحد مكونات المشغل في نظام NewLife IoT البيئي، حيث توفر وصولاً موحداً لأجهزة Schneider PLC لمنصات IoT العلوية مثل ZeroIoT و IoTEdge. يمكن للمطورين الاتصال بالأجهزة وقراءة/كتابة البيانات من خلال الواجهات الموحدة `IDriver` / `INode`.

> **🌐 لغات أخرى**: [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Français](README.fr.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [한국어](README.ko.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

## ✨ المميزات

- **جاهز للاستخدام** — مبني على تنفيذ Modbus TCP من NewLife.Modbus، قم بالتثبيت عبر NuGuide وابدأ فوراً
- **اكتشاف تلقائي** — مُعلَّم بـ `[Driver("SchneiderPLC")]`، يمكن لمنصات IoT مسح وتحميل المشغل تلقائياً
- **قراءة/كتابة بالعلامات** — اقرأ واكتب مباشرة باستخدام بناء جملة العنونة الأصلي لشنايدر (`"MW100"`، `"M0.1"`، `"MF100"`) دون التعامل مع تفاصيل بروتوكول Modbus
- **تحويل تلقائي للأنواع** — يحول تلقائياً بايتات Modbus ذات الترتيب الكبير (big-endian) إلى أنواع .NET المقابلة (UInt16/UInt32/Single/Boolean) عند القراءة
- **واجهة موحدة** — تتبع التجريدات المعيارية لـ NewLife.IoT، وتتكامل بسلاسة مع النظام البيئي IoT
- **متعدد الأطر** — يدعم netstandard2.1/2.0/net461/net45، متوافق مع أنظمة .NET Framework القديمة وتطبيقات .NET الحديثة

## 🚀 بداية سريعة

### التثبيت

```shell
dotnet add package NewLife.Schneider
```

### الاستخدام

#### قراءة/كتابة Modbus TCP الأساسية

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// تكوين المعلمات
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// فتح الاتصال
var node = await driver.OpenAsync(null, pm);

// قراءة سجل الاحتفاظ 100 (يعيد UInt16)
var value = await driver.ReadTag(node, "MW100");

// قراءة مقيدة النوع: تحويل تلقائي
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// كتابة سجل الاحتفاظ 100 (يدعم الأنواع الأصلية .NET)
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// قراءة مجمعة
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// إغلاق الاتصال
await driver.CloseAsync(node);
```

#### بناء جملة العنونة الخاصة بشنايدر

```csharp
// محلل العناوين (يمكن استخدامه دون الاتصال بـ PLC)
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 قراءة سجلات الاحتفاظ
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 قراءة الملفات

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → للقراءة فقط

// تحليل آمن
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### تكوين مسبق حسب الطراز

```csharp
// إنشاء معلمات مسبقة حسب الطراز
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// أو الاستعلام عن معلومات الطراز يدوياً
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} سجل");
```

## 📚 التوثيق

| المستند | الوصف |
|---------|------|
| [المتطلبات (中文)](Doc/需求文档.md) | الرؤية والأهداف الأساسية والمتطلبات الوظيفية |
| [قائمة الميزات (中文)](Doc/功能清单.md) | تتبع ثلاثي الأبعاد: التنفيذ / الاختبار / التوثيق |
| [الهندسة المعمارية (中文)](Doc/架构设计.md) | الطبقات والمكونات وسير العمل والقرارات الرئيسية |
| [تحليل المنافسين (中文)](Doc/竞品分析报告.md) | مصفوفة المقارنة وتحليل الفجوات |
| [جسر Modbus Plus (中文)](Doc/Modbus%20Plus%20桥接说明.md) | الوصول إلى أجهزة Modbus Plus عبر جسر وحدة Ethernet |

## 📦 التبعيات

| الحزمة | الوصف |
|--------|------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | التجريدات المعيارية IoT (واجهات IDriver/INode) |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | تنفيذ بروتوكول Modbus TCP/RTU/ASCII |

## 🔗 روابط

- **الكود المصدري**: https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**: https://www.nuget.org/packages/NewLife.Schneider
- **موقع NewLife**: https://newlifex.com
- **مجموعات QQ**: 1600800 / 1600838

## فريق تطوير NewLife

تأسس فريق NewLife (新生命) في عام 2002، وهو مكرس لتقديم استشارات تطبيقات الأجهزة/البرامج لصناعة IoT، وتخطيط هندسة الأنظمة، وخدمات التطوير. تم تبني أكثر من 80 مشروعاً مفتوح المصدر للفريق على نطاق واسع في قطاعات الطاقة والتعليم والإنترنت والاتصالات والنقل والخدمات اللوجستية والتحكم الصناعي والرعاية الصحية والتراث الثقافي، مع أكثر من 4 ملايين تنزيل على NuGet.

> مشاريع أخرى: [NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [Stardust](https://github.com/NewLifeX/Stardust) | [AntJob](https://github.com/NewLifeX/AntJob) | [المزيد](https://github.com/NewLifeX)
