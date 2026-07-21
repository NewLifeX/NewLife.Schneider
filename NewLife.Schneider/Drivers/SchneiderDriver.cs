using System.ComponentModel;
using NewLife.IoT;
using NewLife.IoT.Drivers;
using NewLife.IoT.ThingModels;
using NewLife.Log;

namespace NewLife.Schneider.Drivers;

/// <summary>
/// 施耐德PLC驱动
/// </summary>
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

    #region 标签式读写
    /// <summary>读取单个施耐德标签</summary>
    /// <remarks>
    /// 支持施耐德原生寻址语法，如 "MW100"、"M0.1"、"I0.0"、"QW50"、"MD200"、"MF100"。
    /// 内部自动解析标签地址并映射到对应的 Modbus 功能码和寄存器地址。
    /// </remarks>
    /// <param name="node">节点对象</param>
    /// <param name="tag">施耐德标签地址，如 "MW100"、"M0.1"</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>读取到的数据值</returns>
    public async Task<Object> ReadTag(INode node, String tag, CancellationToken cancellationToken = default)
    {
        var addr = SchneiderAddress.Parse(tag);
        var point = CreatePoint(tag, addr);
        var result = await ReadAsync(node, new[] { point }, cancellationToken);
        if (!result.IsSuccess) return null;

        return result.GetValue(tag);
    }

    /// <summary>批量读取多个施耐德标签</summary>
    /// <param name="node">节点对象</param>
    /// <param name="tags">施耐德标签地址数组，如 ["MW100", "M0.1", "MD200"]</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>标签名到数据值的字典。仅包含读取成功的点位</returns>
    public async Task<IDictionary<String, Object>> ReadTags(INode node, String[] tags, CancellationToken cancellationToken = default)
    {
        var points = tags.Select(tag =>
        {
            var addr = SchneiderAddress.Parse(tag);
            return CreatePoint(tag, addr);
        }).ToArray();

        var result = await ReadAsync(node, points, cancellationToken);
        if (!result.IsSuccess || result.Points == null) return new Dictionary<String, Object>();

        var dic = new Dictionary<String, Object>();
        for (var i = 0; i < result.Points.Length; i++)
        {
            if (i < result.Values.Length)
                dic[result.Points[i].Name] = result.Values[i];
        }
        return dic;
    }

    /// <summary>写入单个施耐德标签</summary>
    /// <remarks>
    /// 支持施耐德原生寻址语法，内部自动解析标签地址并映射到对应的 Modbus 功能码和寄存器地址。
    /// 根据数据类型自动选择写入方式（位写入或寄存器写入）。
    /// </remarks>
    /// <param name="node">节点对象</param>
    /// <param name="tag">施耐德标签地址，如 "MW100"、"M0.1"</param>
    /// <param name="value">要写入的数据值</param>
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
                2 => "UInt32",
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
    #endregion
}