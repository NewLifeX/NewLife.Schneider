using System.ComponentModel;
using NewLife.IoT;
using NewLife.IoT.Drivers;
using NewLife.Log;

namespace NewLife.Schneider.Drivers;

/// <summary>
/// 施耐德PLC驱动
/// </summary>
[Driver("SchneiderPLC")]
[DisplayName("施耐德PLC")]
public class SchneiderDriver : ModbusTcpDriver, ILogFeature, ITracerFeature
{
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
}