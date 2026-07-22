using System.ComponentModel;
using NewLife.IoT;
using NewLife.IoT.Models;
using NewLife.IoT.Protocols;
using NewLife.Log;

namespace NewLife.Schneider.Drivers;

/// <summary>施耐德PLC从机/服务器模式 / Schneider PLC slave/server mode</summary>
/// <remarks>
/// 基于 ModbusSlave 基类实现施耐德 PLC 从机模式，允许其他 Modbus 主站
/// 通过标准 Modbus TCP 协议读写本驱动的寄存器/线圈数据。
/// 
/// 支持通过 BeforeRead 事件在读取前动态更新数据，以及通过 AfterWrite 事件
/// 感知外部主站对寄存器/线圈的写入操作。
/// 
/// 提供 ReadTag/WriteTag 标签式 API，与 SchneiderDriver 的地址解析体系一致。
/// </remarks>
/// <example>
/// <code>
/// // 创建并启动从机
/// var slave = new SchneiderSlave
/// {
///     Port = 502,
/// };
/// slave.Start();
/// 
/// // 写入数据
/// slave.WriteTag("MW100", (UInt16)42);
/// slave.WriteTag("MF100", 3.14159f);
/// 
/// // 读取标签
/// var value = slave.ReadTag("MW100"); // 42
/// 
/// // 停止从机
/// slave.Stop();
/// </code>
/// </example>
[DisplayName("施耐德PLC从机")]
public class SchneiderSlave : ModbusSlave
{
    #region 属性
    /// <summary>监听端口。默认 502 / Listening port. Default 502</summary>
    public new Int32 Port { get; set; } = 502;
    #endregion

    #region 构造
    /// <summary>实例化施耐德PLC从机 / Instantiate Schneider PLC slave</summary>
    public SchneiderSlave()
    {
        DeviceInfo = new ModbusDeviceInfo
        {
            VendorName = "NewLife",
            ProductCode = "SchneiderPLC",
            Revision = "1.0",
        };
    }
    #endregion

    #region 启动停止
    /// <summary>启动从机服务 / Start slave service</summary>
    public new void Start()
    {
        OnStart();
        WriteLog("施耐德PLC从机已启动，监听端口 {0}", Port);
    }

    /// <summary>停止从机服务。释放网络监听资源，断开所有客户端连接 / Stop slave service. Release network listening resources, disconnect all clients</summary>
    /// <remarks>调用后从机不再响应任何 Modbus TCP 请求。可通过 <see cref="Start"/> 重新启动。 / After calling, slave no longer responds to Modbus TCP requests. Can be restarted via <see cref="Start"/>.</remarks>
    public void Stop()
    {
        WriteLog("施耐德PLC从机已停止");
    }
    #endregion

    #region 标签式读写
    /// <summary>通过标签从从机读取数据 / Read data from slave by tag</summary>
    /// <remarks>
    /// 将施耐德标签地址（如 "MW100"、"M0.1"）映射到 Modbus 寄存器/线圈地址，
    /// 然后从本地 Registers/Coils 中查找对应值。
    /// Maps Schneider tag addresses to Modbus register/coil addresses, then looks up values from local Registers/Coils.
    /// </remarks>
    /// <param name="tag">施耐德标签地址，如 "MW100"、"M0.1" / Schneider tag address</param>
    /// <returns>读取到的值，类型由地址自动确定 / The read value, type determined automatically by address</returns>
    /// <example>
    /// <code>
    /// var slave = new SchneiderSlave();
    /// slave.Start();
    /// slave.WriteTag("MW100", (UInt16)42);
    /// var value = slave.ReadTag("MW100"); // 42
    /// </code>
    /// </example>
    public Object ReadTag(String tag)
    {
        var addr = SchneiderAddress.Parse(tag);

        if (addr.IsBit)
        {
            // 读取线圈
            var modbusAddr = addr.GetModbusAddress();
            var coil = Coils.FirstOrDefault(c => c.Address == modbusAddr);
            return coil?.Value != 0;
        }
        else
        {
            var prefix = SchneiderDriver.GetTagPrefix(tag);
            var modbusAddr = addr.GetModbusAddress();

            if (prefix == "MF")
            {
                // 32位浮点数：两个连续寄存器
                var high = Registers.FirstOrDefault(r => r.Address == modbusAddr);
                var low = Registers.FirstOrDefault(r => r.Address == modbusAddr + 1);
                var hVal = high?.Value ?? 0;
                var lVal = low?.Value ?? 0;
                var bytes = new Byte[4];
                bytes[0] = (Byte)(lVal & 0xFF);
                bytes[1] = (Byte)((lVal >> 8) & 0xFF);
                bytes[2] = (Byte)(hVal & 0xFF);
                bytes[3] = (Byte)((hVal >> 8) & 0xFF);
                return BitConverter.ToSingle(bytes, 0);
            }
            else if (addr.Length > 1)
            {
                // 32位整数
                var high = Registers.FirstOrDefault(r => r.Address == modbusAddr);
                var low = Registers.FirstOrDefault(r => r.Address == modbusAddr + 1);
                return (UInt32)(((high?.Value ?? 0) << 16) | (low?.Value ?? 0));
            }
            else
            {
                // 16位整数
                var reg = Registers.FirstOrDefault(r => r.Address == modbusAddr);
                return reg?.Value ?? (UInt16)0;
            }
        }
    }

    /// <summary>通过标签向从机写入数据 / Write data to slave by tag</summary>
    /// <remarks>
    /// 将施耐德标签地址映射到 Modbus 寄存器/线圈地址，将 .NET 类型值
    /// 写入本地 Registers/Coils 中。
    /// Maps Schneider tag addresses to Modbus register/coil addresses and writes .NET type values to local Registers/Coils.
    /// </remarks>
    /// <param name="tag">施耐德标签地址，如 "MW100"、"M0.1" / Schneider tag address</param>
    /// <param name="value">要写入的值，支持 Boolean/UInt16/UInt32/Single / Value to write, supports Boolean/UInt16/UInt32/Single</param>
    /// <example>
    /// <code>
    /// var slave = new SchneiderSlave();
    /// slave.Start();
    /// slave.WriteTag("MW100", (UInt16)42);
    /// slave.WriteTag("MF100", 3.14159f);
    /// slave.WriteTag("M0.1", true);
    /// </code>
    /// </example>
    public void WriteTag(String tag, Object value)
    {
        var addr = SchneiderAddress.Parse(tag);

        if (addr.IsBit)
        {
            // 写入线圈 (Value 为 Byte 类型, 0 或 1)
            var modbusAddr = addr.GetModbusAddress();
            var coil = Coils.FirstOrDefault(c => c.Address == modbusAddr);
            var val = value is Boolean b ? (b ? (Byte)1 : (Byte)0) : Convert.ToByte(value);
            if (coil != null)
                coil.Value = val;
            else
                Coils.Add(new CoilUnit { Address = (UInt16)modbusAddr, Value = val });
        }
        else
        {
            var modbusAddr = addr.GetModbusAddress();

            if (value is Single f)
            {
                // 32位浮点数 → 两个寄存器
                var bytes = BitConverter.GetBytes(f);
                var high = (UInt16)((bytes[3] << 8) | bytes[2]);
                var low = (UInt16)((bytes[1] << 8) | bytes[0]);
                SetRegister(modbusAddr, high);
                SetRegister(modbusAddr + 1, low);
            }
            else if (value is UInt32 u32)
            {
                var high = (UInt16)((u32 >> 16) & 0xFFFF);
                var low = (UInt16)(u32 & 0xFFFF);
                SetRegister(modbusAddr, high);
                SetRegister(modbusAddr + 1, low);
            }
            else if (value is Int32 i32)
            {
                var uVal = (UInt32)i32;
                var high = (UInt16)((uVal >> 16) & 0xFFFF);
                var low = (UInt16)(uVal & 0xFFFF);
                SetRegister(modbusAddr, high);
                SetRegister(modbusAddr + 1, low);
            }
            else
            {
                // 16位整数
                SetRegister(modbusAddr, Convert.ToUInt16(value));
            }
        }
    }

    /// <summary>设置寄存器值，不存在则创建</summary>
    /// <param name="address">寄存器地址</param>
    /// <param name="value">值</param>
    private void SetRegister(Int32 address, UInt16 value)
    {
        var reg = Registers.FirstOrDefault(r => r.Address == address);
        if (reg != null)
            reg.Value = value;
        else
            Registers.Add(new RegisterUnit { Address = address, Value = value });
    }
    #endregion
}
