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
    public override INode Open(IDevice device, IDictionary<String, Object> parameters)
    {
        var modbusNode = base.Open(device, parameters);
        if (modbusNode is ModbusNode node && node.Modbus != null)
        {
            node.Modbus.Open();
        }

        return modbusNode;
    }
}
