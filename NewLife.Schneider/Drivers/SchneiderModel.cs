using System.ComponentModel;

namespace NewLife.Schneider.Drivers;

/// <summary>施耐德PLC型号 / Schneider PLC models</summary>
public enum SchneiderModel
{
    /// <summary>M200。基础型PLC，适用于简单控制任务 / Basic PLC, suitable for simple control tasks</summary>
    [Description("M200 基础型PLC")]
    M200 = 0,

    /// <summary>M221。高级型PLC，适用于中等规模控制 / Advanced PLC, suitable for medium-scale control</summary>
    [Description("M221 高级型PLC")]
    M221 = 1,

    /// <summary>M241。逻辑控制器，适用于复杂逻辑控制 / Logic controller, suitable for complex logic control</summary>
    [Description("M241 逻辑控制器")]
    M241 = 2,

    /// <summary>M251。运动控制器，适用于运动控制场景 / Motion controller, suitable for motion control scenarios</summary>
    [Description("M251 运动控制器")]
    M251 = 3,

    /// <summary>M258。高级运动控制器，适用于高性能运动控制 / Advanced motion controller, suitable for high-performance motion control</summary>
    [Description("M258 高级运动控制器")]
    M258 = 4,

    /// <summary>M262。IoT网关PLC，集成物联网边缘网关功能 / IoT gateway PLC with integrated edge gateway functionality</summary>
    [Description("M262 IoT网关PLC")]
    M262 = 5,
}

/// <summary>施耐德PLC型号预设信息 / Schneider PLC model preset information</summary>
public class SchneiderModelInfo
{
    #region 属性
    /// <summary>型号 / Model</summary>
    public SchneiderModel Model { get; }

    /// <summary>型号名称 / Model name</summary>
    public String Name { get; }

    /// <summary>默认TCP端口 / Default TCP port</summary>
    public Int32 Port { get; }

    /// <summary>默认读取功能码。通常为3(读保持寄存器) / Default read function code. Usually 3 (Read Holding Registers)</summary>
    public Byte ReadCode { get; }

    /// <summary>默认写入功能码。通常为16(写多寄存器) / Default write function code. Usually 16 (Write Multiple Registers)</summary>
    public Byte WriteCode { get; }

    /// <summary>最大线圈数量 / Maximum number of coils</summary>
    public Int32 MaxCoils { get; }

    /// <summary>最大寄存器数量 / Maximum number of registers</summary>
    public Int32 MaxRegisters { get; }

    /// <summary>说明 / Description</summary>
    public String Description { get; }
    #endregion

    #region 构造
    /// <summary>构造型号预设信息 / Construct model preset information</summary>
    public SchneiderModelInfo(SchneiderModel model, String name, Int32 port, Byte readCode, Byte writeCode, Int32 maxCoils, Int32 maxRegisters, String description)
    {
        Model = model;
        Name = name;
        Port = port;
        ReadCode = readCode;
        WriteCode = writeCode;
        MaxCoils = maxCoils;
        MaxRegisters = maxRegisters;
        Description = description;
    }
    #endregion
}

/// <summary>施耐德PLC型号预设帮助类 / Schneider PLC model preset helper</summary>
/// <example>
/// <code>
/// // 获取 M200 型号预设
/// var info = SchneiderModelHelper.GetModelInfo(SchneiderModel.M200);
/// Console.WriteLine(info.Port); // 502
///
/// // 按名称获取
/// var m262 = SchneiderModelHelper.GetModelInfo("M262");
/// Console.WriteLine(m262.MaxRegisters); // 4096
///
/// // 列出所有型号
/// foreach (var m in SchneiderModelHelper.GetAllModels())
///     Console.WriteLine($"{m.Name}: {m.MaxRegisters} 寄存器");
/// </code>
/// </example>
public static class SchneiderModelHelper
{
    /// <summary>获取型号预设信息 / Get model preset information</summary>
    /// <param name="model">施耐德PLC型号 / Schneider PLC model</param>
    /// <returns>型号预设信息 / Model preset information</returns>
    public static SchneiderModelInfo GetModelInfo(SchneiderModel model) => model switch
    {
        SchneiderModel.M200 => new SchneiderModelInfo(
            model, "M200", 502, 3, 16, 256, 256,
            "基础型PLC，支持Modbus TCP，适用于简单控制任务"),

        SchneiderModel.M221 => new SchneiderModelInfo(
            model, "M221", 502, 3, 16, 512, 512,
            "高级型PLC，支持Modbus TCP，适用于中等规模控制"),

        SchneiderModel.M241 => new SchneiderModelInfo(
            model, "M241", 502, 3, 16, 1024, 1024,
            "逻辑控制器，支持Modbus TCP，适用于复杂逻辑控制"),

        SchneiderModel.M251 => new SchneiderModelInfo(
            model, "M251", 502, 3, 16, 2048, 2048,
            "运动控制器，支持Modbus TCP，适用于运动控制场景"),

        SchneiderModel.M258 => new SchneiderModelInfo(
            model, "M258", 502, 3, 16, 4096, 4096,
            "高级运动控制器，支持Modbus TCP，适用于高性能运动控制"),

        SchneiderModel.M262 => new SchneiderModelInfo(
            model, "M262", 502, 3, 16, 4096, 4096,
            "IoT网关PLC，集成物联网边缘网关功能，支持Modbus TCP和云连接"),

        _ => throw new ArgumentOutOfRangeException(nameof(model), $"不支持的施耐德PLC型号: {model}"),
    };

    /// <summary>根据型号名称获取预设信息 / Get model preset information by model name</summary>
    /// <param name="name">型号名称，如"M200"、"M221" / Model name, e.g. "M200", "M221"</param>
    /// <returns>型号预设信息 / Model preset information</returns>
    /// <exception cref="ArgumentOutOfRangeException">未识别的型号名称 / Unrecognized model name</exception>
    public static SchneiderModelInfo GetModelInfo(String name)
    {
        if (String.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        return name.ToUpperInvariant() switch
        {
            "M200" => GetModelInfo(SchneiderModel.M200),
            "M221" => GetModelInfo(SchneiderModel.M221),
            "M241" => GetModelInfo(SchneiderModel.M241),
            "M251" => GetModelInfo(SchneiderModel.M251),
            "M258" => GetModelInfo(SchneiderModel.M258),
            "M262" => GetModelInfo(SchneiderModel.M262),
            _ => throw new ArgumentOutOfRangeException(nameof(name), $"未识别的施耐德PLC型号: '{name}'"),
        };
    }

    /// <summary>获取所有支持的型号列表 / Get all supported model list</summary>
    /// <returns>型号预设信息数组 / Array of model preset information</returns>
    public static SchneiderModelInfo[] GetAllModels() =>
    [
        GetModelInfo(SchneiderModel.M200),
        GetModelInfo(SchneiderModel.M221),
        GetModelInfo(SchneiderModel.M241),
        GetModelInfo(SchneiderModel.M251),
        GetModelInfo(SchneiderModel.M258),
        GetModelInfo(SchneiderModel.M262),
    ];
}
