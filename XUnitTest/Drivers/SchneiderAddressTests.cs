using System.ComponentModel;
using NewLife.Schneider.Drivers;
using Xunit;

namespace XUnitTest.Drivers;

/// <summary>施耐德地址解析器单元测试</summary>
public class SchneiderAddressTests
{
    #region 保持寄存器
    [Fact]
    [DisplayName("Parse_MW100_正确解析为保持寄存器")]
    public void Parse_MW100_ReturnsHoldingRegister()
    {
        var addr = SchneiderAddress.Parse("MW100");
        Assert.Equal(SchneiderAddressType.HoldingRegister, addr.Type);
        Assert.Equal(100, addr.Offset);
        Assert.False(addr.IsBit);
        Assert.Equal(1, addr.Length);
        Assert.Equal(100, addr.GetModbusAddress());
        Assert.Equal(3, addr.GetReadCode());
        Assert.Equal(6, addr.GetWriteCode());
    }

    [Fact]
    [DisplayName("Parse_带百分号_MW200_正确解析")]
    public void Parse_PercentPrefix_Works()
    {
        var addr = SchneiderAddress.Parse("%MW200");
        Assert.Equal(SchneiderAddressType.HoldingRegister, addr.Type);
        Assert.Equal(200, addr.Offset);
    }

    [Fact]
    [DisplayName("Parse_MD300_正确解析为双字保持寄存器")]
    public void Parse_MD300_ReturnsDoubleWord()
    {
        var addr = SchneiderAddress.Parse("MD300");
        Assert.Equal(SchneiderAddressType.HoldingRegister, addr.Type);
        Assert.Equal(300, addr.Offset);
        Assert.Equal(2, addr.Length);
        Assert.Equal(16, addr.GetWriteCode()); // 双字必须用 FC16
    }

    [Fact]
    [DisplayName("Parse_MF100_正确解析为浮点保持寄存器")]
    public void Parse_MF100_ReturnsFloat()
    {
        var addr = SchneiderAddress.Parse("MF100");
        Assert.Equal(SchneiderAddressType.HoldingRegister, addr.Type);
        Assert.Equal(100, addr.Offset);
        Assert.Equal(2, addr.Length);
    }
    #endregion

    #region 线圈
    [Fact]
    [DisplayName("Parse_M0.1_正确解析为线圈位")]
    public void Parse_M_BitAddress_ReturnsCoil()
    {
        var addr = SchneiderAddress.Parse("M0.1");
        Assert.Equal(SchneiderAddressType.Coil, addr.Type);
        Assert.Equal(0, addr.Offset);
        Assert.True(addr.IsBit);
        Assert.Equal(1, addr.BitIndex);
        Assert.Equal(1, addr.GetModbusAddress()); // 0*8 + 1
        Assert.Equal(1, addr.GetReadCode()); // FC01
        Assert.Equal(5, addr.GetWriteCode()); // FC05
    }

    [Fact]
    [DisplayName("Parse_M10.7_正确解析高位线圈")]
    public void Parse_M_HighBit_ReturnsCoil()
    {
        var addr = SchneiderAddress.Parse("M10.7");
        Assert.Equal(SchneiderAddressType.Coil, addr.Type);
        Assert.Equal(10, addr.Offset);
        Assert.Equal(7, addr.BitIndex);
        Assert.Equal(87, addr.GetModbusAddress()); // 10*8 + 7
    }

    [Fact]
    [DisplayName("Parse_Q5.0_正确解析为输出线圈")]
    public void Parse_Q_BitAddress_ReturnsCoil()
    {
        var addr = SchneiderAddress.Parse("Q5.0");
        Assert.Equal(SchneiderAddressType.Coil, addr.Type);
        Assert.Equal(5, addr.Offset);
        Assert.Equal(0, addr.BitIndex);
        Assert.Equal(40, addr.GetModbusAddress()); // 5*8 + 0
    }
    #endregion

    #region 离散输入
    [Fact]
    [DisplayName("Parse_I2.3_正确解析为离散输入")]
    public void Parse_I_BitAddress_ReturnsDiscreteInput()
    {
        var addr = SchneiderAddress.Parse("I2.3");
        Assert.Equal(SchneiderAddressType.DiscreteInput, addr.Type);
        Assert.Equal(2, addr.Offset);
        Assert.True(addr.IsBit);
        Assert.Equal(3, addr.BitIndex);
        Assert.Equal(19, addr.GetModbusAddress()); // 2*8 + 3
        Assert.Equal(2, addr.GetReadCode()); // FC02
        Assert.Equal(0, addr.GetWriteCode()); // 只读
    }
    #endregion

    #region 输入寄存器
    [Fact]
    [DisplayName("Parse_IW50_正确解析为输入寄存器")]
    public void Parse_IW50_ReturnsInputRegister()
    {
        var addr = SchneiderAddress.Parse("IW50");
        Assert.Equal(SchneiderAddressType.InputRegister, addr.Type);
        Assert.Equal(50, addr.Offset);
        Assert.False(addr.IsBit);
        Assert.Equal(4, addr.GetReadCode()); // FC04
        Assert.Equal(0, addr.GetWriteCode()); // 只读
    }

    [Fact]
    [DisplayName("Parse_SW100_正确解析为系统字")]
    public void Parse_SW100_ReturnsInputRegister()
    {
        var addr = SchneiderAddress.Parse("SW100");
        Assert.Equal(SchneiderAddressType.InputRegister, addr.Type);
        Assert.Equal(100, addr.Offset);
    }
    #endregion

    #region 输出寄存器
    [Fact]
    [DisplayName("Parse_QW60_正确解析为保持寄存器(输出)")]
    public void Parse_QW60_ReturnsHoldingRegister()
    {
        var addr = SchneiderAddress.Parse("QW60");
        Assert.Equal(SchneiderAddressType.HoldingRegister, addr.Type);
        Assert.Equal(60, addr.Offset);
        Assert.Equal(6, addr.GetWriteCode()); // FC06 写单寄存器
    }
    #endregion

    #region 边界与异常
    [Fact]
    [DisplayName("Parse_空字符串_抛出ArgumentNullException")]
    public void Parse_EmptyString_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => SchneiderAddress.Parse(null));
    }

    [Fact]
    [DisplayName("Parse_无效格式_抛出FormatException")]
    public void Parse_InvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => SchneiderAddress.Parse("INVALID"));
    }

    [Fact]
    [DisplayName("Parse_不支持前缀_抛出FormatException")]
    public void Parse_UnknownPrefix_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => SchneiderAddress.Parse("XYZ100"));
    }

    [Fact]
    [DisplayName("TryParse_有效地址_返回True")]
    public void TryParse_ValidAddress_ReturnsTrue()
    {
        Assert.True(SchneiderAddress.TryParse("MW100", out var addr));
        Assert.NotNull(addr);
        Assert.Equal(100, addr.Offset);
    }

    [Fact]
    [DisplayName("TryParse_无效地址_返回False")]
    public void TryParse_InvalidAddress_ReturnsFalse()
    {
        Assert.False(SchneiderAddress.TryParse("INVALID", out var addr));
        Assert.Null(addr);
    }

    [Fact]
    [DisplayName("ToString_返回施耐德格式")]
    public void ToString_ReturnsSchneiderFormat()
    {
        var addr = SchneiderAddress.Parse("MW100");
        Assert.Equal("%MW100", addr.ToString());
    }
    #endregion
}
