using System.ComponentModel;
using NewLife.IoT;
using NewLife.IoT.Drivers;
using NewLife.IoT.ThingModels;
using NewLife.Log;

namespace NewLife.Schneider.Drivers;

/// <summary>施耐德PLC驱动</summary>
/// <remarks>
/// 基于 Modbus TCP 的施耐德 PLC 专用驱动，支持标签式读写和自动类型转换。
/// 提供 ReadTag/WriteTag 高层接口，内部自动解析施耐德原生寻址语法并映射到 Modbus 功能码。
/// </remarks>
[Driver("SchneiderPLC")]
[DisplayName("施耐德PLC")]
public class SchneiderDriver : ModbusTcpDriver, ILogFeature, ITracerFeature
{
    #region 构造
    /// <summary>建立连接，打开驱动</summary>
    /// <param name="device">逻辑设备</param>
    /// <param name="parameter">驱动参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>节点对象</returns>
    public override async Task<INode> OpenAsync(IDevice device, IDriverParameter parameter, CancellationToken cancellationToken)
    {
        var modbusNode = await base.OpenAsync(device, parameter, cancellationToken);
        if (modbusNode is ModbusNode node && Modbus != null)
        {
            Modbus.Open();
        }

        return modbusNode;
    }
    #endregion

    #region 型号预设
    /// <summary>根据施耐德PLC型号创建预设参数</summary>
    /// <remarks>
    /// 根据型号自动填充默认端口、功能码和寄存器限制。支持 M200/M221/M241/M251/M258/M262。
    /// </remarks>
    /// <param name="model">施耐德PLC型号</param>
    /// <param name="host">站号，默认1</param>
    /// <param name="server">服务端地址，默认 "127.0.0.1:502"</param>
    /// <returns>预设参数的 SchneiderParameter</returns>
    /// <example>
    /// <code>
    /// var driver = new SchneiderDriver();
    /// var pm = driver.CreateParameter(SchneiderModel.M221);
    /// pm.Server = "192.168.1.100:502";
    /// var node = driver.Open(null, pm);
    /// </code>
    /// </example>
    public SchneiderParameter CreateParameter(SchneiderModel model, Byte host = 1, String server = "127.0.0.1:502")
    {
        var info = SchneiderModelHelper.GetModelInfo(model);
        return new SchneiderParameter
        {
            Host = host,
            Server = server,
            ReadCode = (NewLife.IoT.Protocols.FunctionCodes)info.ReadCode,
            WriteCode = (NewLife.IoT.Protocols.FunctionCodes)info.WriteCode,
        };
    }
    #endregion

    #region 标签式读写
    /// <summary>读取单个施耐德标签，返回自动转换类型后的值</summary>
    /// <remarks>
    /// 支持施耐德原生寻址语法，如 "MW100"、"M0.1"、"I0.0"、"QW50"、"MD200"、"MF100"。
    /// 内部自动解析标签地址并映射到对应的 Modbus 功能码和寄存器地址。
    /// 返回类型根据地址类型自动确定：
    /// <list type="bullet">
    ///   <item><term>%MW、%IW、%QW、%SW</term><description>UInt16（16 位无符号整数）</description></item>
    ///   <item><term>%MD</term><description>UInt32（32 位无符号整数）</description></item>
    ///   <item><term>%MF</term><description>Single（32 位浮点数）</description></item>
    ///   <item><term>%M、%I、%Q</term><description>Boolean（位状态）</description></item>
    /// </list>
    /// </remarks>
    /// <param name="node">节点对象</param>
    /// <param name="tag">施耐德标签地址，如 "MW100"、"M0.1"</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>读取到的数据值，类型由地址自动确定</returns>
    /// <example>
    /// <code>
    /// var driver = new SchneiderDriver();
    /// var node = driver.Open(null, parameter);
    /// var value = await driver.ReadTag(node, "MW100");
    /// Console.WriteLine(value); // UInt16
    ///
    /// var temp = await driver.ReadTag(node, "MF100");
    /// Console.WriteLine(temp); // Single (Float)
    /// </code>
    /// </example>
    public async Task<Object> ReadTag(INode node, String tag, CancellationToken cancellationToken = default)
    {
        var addr = SchneiderAddress.Parse(tag);
        var point = CreatePoint(tag, addr);
        var result = await ReadAsync(node, new[] { point }, cancellationToken);
        if (!result.IsSuccess) return null;

        var value = result.GetValue(tag);
        return ConvertToTypedValue(value, tag, addr);
    }

    /// <summary>读取单个施耐德标签并返回指定 .NET 类型的值</summary>
    /// <typeparam name="T">目标 .NET 类型（如 UInt16、UInt32、Single、Boolean）</typeparam>
    /// <param name="node">节点对象</param>
    /// <param name="tag">施耐德标签地址</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>转换后的类型化值；转换失败或读取失败时返回 default(T)</returns>
    /// <example>
    /// <code>
    /// var driver = new SchneiderDriver();
    /// var node = driver.Open(null, parameter);
    /// var value = await driver.ReadTagValue&lt;UInt16&gt;(node, "MW100");
    /// var temp = await driver.ReadTagValue&lt;Single&gt;(node, "MF100");
    /// var flag = await driver.ReadTagValue&lt;Boolean&gt;(node, "M0.1");
    /// </code>
    /// </example>
    public async Task<T> ReadTagValue<T>(INode node, String tag, CancellationToken cancellationToken = default)
    {
        var value = await ReadTag(node, tag, cancellationToken);
        if (value == null) return default;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T), null);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>批量读取多个施耐德标签</summary>
    /// <param name="node">节点对象</param>
    /// <param name="tags">施耐德标签地址数组，如 ["MW100", "M0.1", "MD200"]</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>标签名到数据值的字典。值已自动转换为对应 .NET 类型</returns>
    public async Task<IDictionary<String, Object>> ReadTags(INode node, String[] tags, CancellationToken cancellationToken = default)
    {
        var pointList = new List<(String tag, SchneiderAddress addr, IPoint point)>();
        foreach (var tag in tags)
        {
            var addr = SchneiderAddress.Parse(tag);
            pointList.Add((tag, addr, CreatePoint(tag, addr)));
        }

        var result = await ReadAsync(node, pointList.Select(p => p.point).ToArray(), cancellationToken);
        if (!result.IsSuccess || result.Points == null) return new Dictionary<String, Object>();

        var dic = new Dictionary<String, Object>();
        for (var i = 0; i < result.Points.Length; i++)
        {
            if (i < result.Values.Length)
            {
                var tag = result.Points[i].Name;
                var item = pointList.FirstOrDefault(p => p.tag == tag);
                dic[tag] = ConvertToTypedValue(result.Values[i], tag, item.addr);
            }
        }
        return dic;
    }

    /// <summary>写入单个施耐德标签</summary>
    /// <remarks>
    /// 支持施耐德原生寻址语法，内部自动解析标签地址并映射到对应的 Modbus 功能码和寄存器地址。
    /// value 参数支持 .NET 原生类型（Boolean、UInt16、UInt32、Single 等），驱动自动转换为 Modbus 协议格式。
    /// </remarks>
    /// <param name="node">节点对象</param>
    /// <param name="tag">施耐德标签地址，如 "MW100"、"M0.1"</param>
    /// <param name="value">要写入的数据值，支持 Boolean/UInt16/UInt32/Single 等 .NET 类型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>写入结果</returns>
    public async Task<Object> WriteTag(INode node, String tag, Object value, CancellationToken cancellationToken = default)
    {
        var addr = SchneiderAddress.Parse(tag);
        var point = CreatePoint(tag, addr);
        var request = new WriteRequest(point, value);
        return await WriteAsync(node, new[] { request }, cancellationToken);
    }

    /// <summary>根据施耐德地址创建点位对象</summary>
    /// <param name="tag">原始标签</param>
    /// <param name="addr">解析后的施耐德地址</param>
    /// <returns>点位对象</returns>
    private static IPoint CreatePoint(String tag, SchneiderAddress addr)
    {
        var modbusAddr = addr.GetModbusAddress();
        var type = addr.Type switch
        {
            SchneiderAddressType.Coil => "Boolean",
            SchneiderAddressType.DiscreteInput => "Boolean",
            SchneiderAddressType.InputRegister => addr.Length > 1 ? "UInt32" : "UInt16",
            SchneiderAddressType.HoldingRegister => addr.Length switch
            {
                1 => "UInt16",
                2 => GetTagPrefix(tag) == "MF" ? "Float" : "UInt32",
                _ => "UInt16",
            },
            _ => "UInt16",
        };

        return new SchneiderPoint
        {
            Name = tag,
            Address = modbusAddr.ToString(),
            Type = type,
            Length = addr.Length * 2, // 每个寄存器2字节
        };
    }

    /// <summary>从施耐德标签地址中提取前缀（如 MW、MD、MF、M 等）</summary>
    /// <param name="tag">原始标签字符串，如 "MW100"、"MF100" 或 "%M0.1"</param>
    /// <returns>大写前缀，如 "MW"、"MF"、"M"</returns>
    /// <example>
    /// <code>
    /// var prefix = SchneiderDriver.GetTagPrefix("MF100"); // "MF"
    /// var prefix2 = SchneiderDriver.GetTagPrefix("%MW200"); // "MW"
    /// </code>
    /// </example>
    public static String GetTagPrefix(String tag)
    {
        if (String.IsNullOrEmpty(tag)) return String.Empty;

        var input = tag.Trim();
        if (input.StartsWith("%")) input = input[1..];

        // 匹配前缀字母部分
        var i = 0;
        while (i < input.Length && Char.IsLetter(input[i])) i++;

        return input[..i].ToUpperInvariant();
    }
    #endregion

    #region 类型转换
    /// <summary>将 Modbus 读取的原始字节（大端序）转换为对应 .NET 类型</summary>
    /// <remarks>
    /// 根据标签地址类型自动判断目标 .NET 类型：
    /// <list type="bullet">
    ///   <item><term>%MW、%IW、%QW、%SW</term><description>→ UInt16</description></item>
    ///   <item><term>%MD</term><description>→ UInt32</description></item>
    ///   <item><term>%MF</term><description>→ Single（浮点数）</description></item>
    ///   <item><term>%M、%I、%Q（位地址）</term><description>→ Boolean</description></item>
    /// </list>
    /// </remarks>
    /// <param name="value">Modbus 读取的原始值（Byte[] 或 Int32）</param>
    /// <param name="tag">原始标签字符串，用于确定数据类型</param>
    /// <param name="addr">解析后的施耐德地址</param>
    /// <returns>转换后的 .NET 类型值</returns>
    /// <example>
    /// <code>
    /// var addr = SchneiderAddress.Parse("MF100");
    /// var raw = new Byte[] { 0x40, 0x49, 0x0F, 0xDA }; // 大端序 3.14159
    /// var value = SchneiderDriver.ConvertToTypedValue(raw, "MF100", addr);
    /// Console.WriteLine(value); // 3.14159f
    /// </code>
    /// </example>
    public static Object ConvertToTypedValue(Object value, String tag, SchneiderAddress addr)
    {
        // 线圈/离散输入：ModbusDriver 返回 Int32(0/1)，转为 Boolean
        if (addr.IsBit)
        {
            if (value is Int32 intVal) return intVal != 0;
            if (value is Byte[] buf && buf.Length > 0) return buf[0] != 0;
            return value;
        }

        // 寄存器类型：ModbusDriver 返回 Byte[]（大端序），需转换
        if (value is Byte[] data && data.Length > 0)
        {
            var prefix = GetTagPrefix(tag);

            // 浮点数 %MF
            if (prefix == "MF" && data.Length >= 4)
            {
                // Modbus 大端序 → .NET 小端序
                var bytes = new Byte[4];
                bytes[0] = data[3];
                bytes[1] = data[2];
                bytes[2] = data[1];
                bytes[3] = data[0];
                return BitConverter.ToSingle(bytes, 0);
            }

            // 32 位整数 %MD
            if (data.Length >= 4)
            {
                return (UInt32)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3]);
            }

            // 16 位整数 %MW、%IW、%QW、%SW
            if (data.Length >= 2)
            {
                return (UInt16)((data[0] << 8) | data[1]);
            }
        }

        return value;
    }
    #endregion
}