using System.ComponentModel;

namespace NewLife.Schneider.Drivers;

/// <summary>施耐德PLC常用Modbus功能码</summary>
/// <remarks>参考 Modbus 标准功能码定义，适配施耐德 PLC 常用操作</remarks>
[DisplayName("施耐德功能码")]
public static class SchneiderFunctionCodes
{
    #region 位操作
    /// <summary>读线圈状态。读取%M、%Q等线圈输出，对应Modbus FC01</summary>
    public const Byte ReadCoil = 1;

    /// <summary>读离散输入。读取%I等输入状态，对应Modbus FC02</summary>
    public const Byte ReadDiscreteInput = 2;

    /// <summary>写单线圈。写入单个%M、%Q，对应Modbus FC05</summary>
    public const Byte WriteCoil = 5;

    /// <summary>写多线圈。批量写入%M、%Q，对应Modbus FC15</summary>
    public const Byte WriteMultipleCoils = 15;
    #endregion

    #region 寄存器操作
    /// <summary>读保持寄存器。读取%MW、%MD、%MF等，对应Modbus FC03</summary>
    public const Byte ReadHoldingRegister = 3;

    /// <summary>读输入寄存器。读取%IW、%SW等，对应Modbus FC04</summary>
    public const Byte ReadInputRegister = 4;

    /// <summary>写单寄存器。写入单个%MW，对应Modbus FC06</summary>
    public const Byte WriteSingleRegister = 6;

    /// <summary>写多寄存器。批量写入%MW、%QW等，对应Modbus FC16</summary>
    public const Byte WriteMultipleRegisters = 16;

    /// <summary>读/写多寄存器。复合操作，对应Modbus FC23</summary>
    public const Byte ReadWriteMultipleRegisters = 23;
    #endregion
}
