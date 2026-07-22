using NewLife.IoT.ThingModels;

namespace NewLife.Schneider.Drivers;

/// <summary>施耐德点位。实现 IPoint 接口，用于标签式读写 API / Schneider point. Implements IPoint interface for tag-based R/W API</summary>
class SchneiderPoint : IPoint
{
    /// <summary>名称 / Point name</summary>
    public String Name { get; set; }

    /// <summary>地址。Modbus寄存器/线圈地址 / Modbus register/coil address</summary>
    public String Address { get; set; }

    /// <summary>数据类型。如 Boolean、UInt16、UInt32 / Data type, e.g. Boolean, UInt16, UInt32</summary>
    public String Type { get; set; }

    /// <summary>大小。数据字节数 / Data size in bytes</summary>
    public Int32 Length { get; set; }
}
