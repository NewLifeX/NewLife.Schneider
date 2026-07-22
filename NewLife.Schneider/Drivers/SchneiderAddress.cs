using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NewLife.Schneider.Drivers;

/// <summary>施耐德地址类型 / Schneider address type</summary>
public enum SchneiderAddressType
{
    /// <summary>线圈。对应Modbus FC01/FC05，如%M、%Q / Coil. Maps to Modbus FC01/FC05, e.g. %M, %Q</summary>
    Coil = 0,

    /// <summary>离散输入。对应Modbus FC02，如%I / Discrete input. Maps to Modbus FC02, e.g. %I</summary>
    DiscreteInput = 1,

    /// <summary>输入寄存器。对应Modbus FC04，如%IW、%SW / Input register. Maps to Modbus FC04, e.g. %IW, %SW</summary>
    InputRegister = 2,

    /// <summary>保持寄存器。对应Modbus FC03/FC16，如%MW、%MD、%MF、%QW / Holding register. Maps to Modbus FC03/FC16, e.g. %MW, %MD, %MF, %QW</summary>
    HoldingRegister = 3,
}

/// <summary>施耐德PLC地址解析器 / Schneider PLC address parser</summary>
/// <remarks>
/// 将施耐德 PLC 原生寻址语法解析为 Modbus 地址和功能码。
/// 支持格式：
///   %MWn    — 保持寄存器字(16位)，n为偏移
///   %MDn    — 保持寄存器双字(32位)，n为偏移
///   %MFn    — 保持寄存器浮点(32位)，n为偏移
///   %Mx.y   — 线圈位，x为字节偏移，y为位索引(0~7)
///   %Ix.y   — 离散输入位，x为字节偏移，y为位索引(0~7)
///   %IWn    — 输入寄存器字(16位)，n为偏移
///   %Qx.y   — 输出线圈位，x为字节偏移，y为位索引(0~7)
///   %QWn    — 输出寄存器字(16位)，n为偏移
///   %SWn    — 系统字(只读)，n为偏移
/// </remarks>
/// <example>
/// <code>
/// // 解析保持寄存器地址
/// var addr = SchneiderAddress.Parse("MW100");
/// Console.WriteLine(addr.Type);           // HoldingRegister
/// Console.WriteLine(addr.Offset);         // 100
/// Console.WriteLine(addr.GetReadCode());  // 3 (FC03)
///
/// // 解析线圈位地址
/// var coil = SchneiderAddress.Parse("M0.1");
/// Console.WriteLine(coil.GetModbusAddress()); // 1 = 0*8 + 1
///
/// // 尝试解析（安全版本）
/// if (SchneiderAddress.TryParse("IW50", out var iw))
///     Console.WriteLine(iw.GetReadCode()); // 4 (FC04)
/// </code>
/// </example>
[DisplayName("施耐德地址")]
public class SchneiderAddress
{
    #region 属性
    /// <summary>地址类型 / Address type</summary>
    public SchneiderAddressType Type { get; }

    /// <summary>Modbus寄存器/线圈偏移地址 / Modbus register/coil offset address</summary>
    public Int32 Offset { get; }

    /// <summary>是否位操作。线圈和离散输入为位操作 / Whether this is a bit operation. Coils and discrete inputs are bit operations</summary>
    public Boolean IsBit { get; }

    /// <summary>位地址中的位索引(0~7)。仅IsBit=true时有效 / Bit index in bit address (0~7). Only valid when IsBit=true</summary>
    public Int32 BitIndex { get; }

    /// <summary>数据类型长度。1=位/字, 2=双字/浮点 / Data type length. 1=bit/word, 2=double word/float</summary>
    public Int32 Length { get; }

    /// <summary>原始标签字符串 / Raw tag string</summary>
    public String Raw { get; }
    #endregion

    #region 构造
    private SchneiderAddress(SchneiderAddressType type, Int32 offset, Boolean isBit, Int32 bitIndex, Int32 length, String raw)
    {
        Type = type;
        Offset = offset;
        IsBit = isBit;
        BitIndex = bitIndex;
        Length = length;
        Raw = raw;
    }
    #endregion

    #region 方法
    /// <summary>解析施耐德地址标签 / Parse Schneider address tag</summary>
    /// <param name="tag">施耐德地址标签，如 "MW100"、"M0.1"、"I0.0"、"QW50" / Schneider address tag</param>
    /// <returns>解析后的SchneiderAddress对象 / Parsed SchneiderAddress object</returns>
    /// <exception cref="ArgumentNullException">tag为null时抛出 / Thrown when tag is null</exception>
    /// <exception cref="FormatException">地址格式不正确时抛出 / Thrown when address format is invalid</exception>
    /// <example>
    /// <code>
    /// var addr = SchneiderAddress.Parse("%MW100");
    /// // addr.Type = HoldingRegister, addr.Offset = 100
    /// </code>
    /// </example>
    public static SchneiderAddress Parse(String tag)
    {
        if (String.IsNullOrEmpty(tag)) throw new ArgumentNullException(nameof(tag));

        // 去掉前导的%符号（兼容带%和不带%的格式）
        var input = tag.Trim();
        if (input.StartsWith("%")) input = input[1..];

        // %Mx.y 位格式或 %MW/MD/MF 字格式
        // %Ix.y 位格式或 %IW 字格式
        // %Qx.y 位格式或 %QW 字格式
        var match = Regex.Match(input, @"^(M|I|Q|IW|QW|SW|MW|MD|MF)(\d+)(?:\.(\d))?$", RegexOptions.IgnoreCase);
        if (!match.Success)
            throw new FormatException($"无法解析施耐德地址: '{tag}'，预期格式如 MW100、M0.1、I0.0、QW50");

        var prefix = match.Groups[1].Value.ToUpperInvariant();
        var offset = Int32.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

        // 处理位索引
        var bitIndex = 0;
        var hasBit = match.Groups[3].Success;
        if (hasBit) bitIndex = Int32.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

        return prefix switch
        {
            "M" => new SchneiderAddress(SchneiderAddressType.Coil, offset, true, bitIndex, 1, tag),
            "Q" => new SchneiderAddress(SchneiderAddressType.Coil, offset, true, bitIndex, 1, tag),
            "I" => new SchneiderAddress(SchneiderAddressType.DiscreteInput, offset, true, bitIndex, 1, tag),
            "MW" => new SchneiderAddress(SchneiderAddressType.HoldingRegister, offset, false, 0, 1, tag),
            "MD" => new SchneiderAddress(SchneiderAddressType.HoldingRegister, offset, false, 0, 2, tag),
            "MF" => new SchneiderAddress(SchneiderAddressType.HoldingRegister, offset, false, 0, 2, tag),
            "QW" => new SchneiderAddress(SchneiderAddressType.HoldingRegister, offset, false, 0, 1, tag),
            "IW" => new SchneiderAddress(SchneiderAddressType.InputRegister, offset, false, 0, 1, tag),
            "SW" => new SchneiderAddress(SchneiderAddressType.InputRegister, offset, false, 0, 1, tag),
            _ => throw new FormatException($"不支持的施耐德地址前缀: '{prefix}'"),
        };
    }

    /// <summary>尝试解析施耐德地址标签，不抛出异常 / Try to parse Schneider address tag without throwing exceptions</summary>
    /// <param name="tag">施耐德地址标签 / Schneider address tag</param>
    /// <param name="address">解析成功时返回SchneiderAddress对象 / Parsed SchneiderAddress object on success</param>
    /// <returns>是否解析成功 / Whether parsing succeeded</returns>
    public static Boolean TryParse(String tag, out SchneiderAddress address)
    {
        try
        {
            address = Parse(tag);
            return true;
        }
        catch
        {
            address = null;
            return false;
        }
    }

    /// <summary>获取推荐的读功能码 / Get recommended read function code</summary>
    /// <returns>Modbus功能码 / Modbus function code</returns>
    public Byte GetReadCode() => Type switch
    {
        SchneiderAddressType.Coil => SchneiderFunctionCodes.ReadCoil,
        SchneiderAddressType.DiscreteInput => SchneiderFunctionCodes.ReadDiscreteInput,
        SchneiderAddressType.InputRegister => SchneiderFunctionCodes.ReadInputRegister,
        SchneiderAddressType.HoldingRegister => SchneiderFunctionCodes.ReadHoldingRegister,
        _ => SchneiderFunctionCodes.ReadHoldingRegister,
    };

    /// <summary>获取推荐的写功能码 / Get recommended write function code</summary>
    /// <returns>Modbus功能码。只读类型返回0 / Modbus function code, returns 0 for read-only types</returns>
    public Byte GetWriteCode() => Type switch
    {
        SchneiderAddressType.Coil => IsBit ? SchneiderFunctionCodes.WriteCoil : SchneiderFunctionCodes.WriteMultipleCoils,
        SchneiderAddressType.DiscreteInput => 0, // 离散输入只读
        SchneiderAddressType.InputRegister => 0, // 输入寄存器只读
        SchneiderAddressType.HoldingRegister => Length > 1 ? SchneiderFunctionCodes.WriteMultipleRegisters : SchneiderFunctionCodes.WriteSingleRegister,
        _ => SchneiderFunctionCodes.WriteSingleRegister,
    };

    /// <summary>获取Modbus起始地址 / Get Modbus start address</summary>
    /// <remarks>对于位类型地址（%Mx.y），Modbus 地址为 x*8 + y / For bit-type addresses (%Mx.y), Modbus address is x*8 + y</remarks>
    public Int32 GetModbusAddress() => Type switch
    {
        SchneiderAddressType.Coil or SchneiderAddressType.DiscreteInput => Offset * 8 + BitIndex,
        _ => Offset,
    };

    /// <summary>返回施耐德格式的地址字符串 / Returns the address string in Schneider format</summary>
    public override String ToString() => $"%{Raw}";
    #endregion
}
