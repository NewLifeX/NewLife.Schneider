# NewLife.Schneider — 슈나이더 PLC 드라이버 라이브러리

[![GitHub top language](https://img.shields.io/github/languages/top/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![GitHub License](https://img.shields.io/github/license/newlifex/NewLife.Schneider?logo=github)](https://github.com/NewLifeX/NewLife.Schneider)
[![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget](https://img.shields.io/nuget/v/NewLife.Schneider?logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.Schneider?label=dev%20nuget&logo=nuget)](https://www.nuget.org/packages/NewLife.Schneider)

**NewLife.Schneider**는 Modbus TCP 프로토콜 기반의 슈나이더 일렉트릭(Schneider Electric) PLC 통신 드라이버 라이브러리입니다. NewLife IoT 생태계의 드라이버 구성 요소 중 하나로, ZeroIoT 및 IoTEdge 같은 상위 IoT 플랫폼에 표준화된 슈나이더 PLC 장치 액세스를 제공합니다. 개발자는 통합된 `IDriver` / `INode` 인터페이스를 통해 장치 연결 및 데이터 읽기/쓰기를 수행할 수 있습니다.

> **🌐 다른 언어**: [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Français](README.fr.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md) | [العربية](README.ar.md)

## ✨ 주요 특징

- **바로 사용 가능** — NewLife.Modbus의 Modbus TCP 구현 기반, NuGet으로 설치하고 즉시 시작
- **자동 발견** — `[Driver("SchneiderPLC")]` 특성으로 주석 처리, IoT 플랫폼이 드라이버를 자동으로 스캔 및 로드
- **태그 기반 읽기/쓰기** — 슈나이더 네이티브 주소 구문(`"MW100"`, `"M0.1"`, `"MF100"`)을 사용하여 직접 읽기/쓰기, Modbus 프로토콜 세부 정보 불필요
- **자동 타입 변환** — 읽기 시 Modbus 빅엔디안 바이트를 해당 .NET 타입(UInt16/UInt32/Single/Boolean)으로 자동 변환
- **통합 인터페이스** — NewLife.IoT 표준 추상화를 따르며 IoT 생태계와 완벽하게 통합
- **멀티 프레임워크** — netstandard2.1/2.0/net461/net45 지원, 레거시 .NET Framework 및 최신 .NET 애플리케이션과 호환

## 🚀 빠른 시작

### 설치

```shell
dotnet add package NewLife.Schneider
```

### 사용법

#### 기본 Modbus TCP 읽기/쓰기

```csharp
using NewLife.Schneider.Drivers;
using NewLife.IoT.Drivers;

var driver = new SchneiderDriver();

// 매개변수 설정
var pm = new SchneiderParameter
{
    Host = 1,
    Server = "192.168.1.100:502",
};

// 연결 열기
var node = await driver.OpenAsync(null, pm);

// 유지 레지스터 100 읽기 (UInt16 반환)
var value = await driver.ReadTag(node, "MW100");

// 강력한 타입 읽기: 자동 변환
var temp = await driver.ReadTagValue<Single>(node, "MF100");  // %MF → Single
var flag = await driver.ReadTagValue<Boolean>(node, "M0.1");   // %M → Boolean

// 유지 레지스터 100 쓰기 (.NET 네이티브 타입 지원)
await driver.WriteTag(node, "MW100", (UInt16)12345);
await driver.WriteTag(node, "MD200", (UInt32)100000);
await driver.WriteTag(node, "MF100", 3.14159f);

// 배치 읽기
var dic = await driver.ReadTags(node, ["MW100", "MD200", "M0.1"]);

// 연결 닫기
await driver.CloseAsync(node);
```

#### 슈나이더 전용 주소 구문

```csharp
// 주소 파서 (PLC에 연결하지 않고 사용 가능)
var addr = SchneiderAddress.Parse("MW100");
Console.WriteLine(addr.GetReadCode());  // 3 → FC03 유지 레지스터 읽기
Console.WriteLine(addr.GetModbusAddress()); // 100

var coil = SchneiderAddress.Parse("M0.1");
Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
Console.WriteLine(coil.GetReadCode());  // 1 → FC01 코일 읽기

var di = SchneiderAddress.Parse("I2.3");
Console.WriteLine(di.GetWriteCode());   // 0 → 읽기 전용

// 안전한 파싱
if (SchneiderAddress.TryParse("QW50", out var qw))
    Console.WriteLine($"{qw.Type}:{qw.Offset}"); // HoldingRegister:50
```

#### 모델별 사전 설정 구성

```csharp
// 모델별 사전 설정 매개변수 생성
var pm = driver.CreateParameter(SchneiderModel.M221);
pm.Server = "192.168.1.100:502";

// 또는 수동으로 모델 정보 조회
var info = SchneiderModelHelper.GetModelInfo("M262");
Console.WriteLine($"{info.Name}: {info.MaxRegisters} 레지스터");
```

## 📚 문서

| 문서 | 설명 |
|------|------|
| [요구사항 (中文)](Doc/需求文档.md) | 비전, 핵심 목표 및 기능 요구사항 |
| [기능 목록 (中文)](Doc/功能清单.md) | 3차원 추적: 구현 / 테스트 / 문서 |
| [아키텍처 (中文)](Doc/架构设计.md) | 계층, 구성 요소, 워크플로우 및 주요 결정 사항 |
| [경쟁사 분석 (中文)](Doc/竞品分析报告.md) | 비교 매트릭스 및 격차 분석 |
| [Modbus Plus 브리지 (中文)](Doc/Modbus%20Plus%20桥接说明.md) | 이더넷 모듈 브리징을 통한 Modbus Plus 장치 액세스 |

## 📦 종속성

| 패키지 | 설명 |
|--------|------|
| [NewLife.IoT](https://www.nuget.org/packages/NewLife.IoT) | IoT 표준 추상화 (IDriver/INode 인터페이스) |
| [NewLife.Modbus](https://www.nuget.org/packages/NewLife.Modbus) | Modbus TCP/RTU/ASCII 프로토콜 구현 |

## 🔗 링크

- **소스 코드**: https://github.com/NewLifeX/NewLife.Schneider
- **NuGet**: https://www.nuget.org/packages/NewLife.Schneider
- **NewLife 웹사이트**: https://newlifex.com
- **QQ 그룹**: 1600800 / 1600838

## NewLife 개발 팀

2002년에 설립된 NewLife 팀(新生命)은 IoT 산업 하드웨어/소프트웨어 애플리케이션 컨설팅, 시스템 아키텍처 계획 및 개발 서비스를 제공합니다. 팀이 주도하는 80개 이상의 오픈소스 프로젝트는 전력, 교육, 인터넷, 통신, 교통, 물류, 산업 제어, 의료 및 문화 유산 분야에서 널리 채택되었으며, NuGet 다운로드 수는 400만 회를 넘었습니다.

> 다른 프로젝트: [NewLife.Core](https://github.com/NewLifeX/X) | [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | [Stardust](https://github.com/NewLifeX/Stardust) | [AntJob](https://github.com/NewLifeX/AntJob) | [더 보기](https://github.com/NewLifeX)
