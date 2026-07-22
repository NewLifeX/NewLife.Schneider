using System.ComponentModel;
using NewLife.Schneider.Drivers;
using Xunit;

namespace XUnitTest.Drivers;

/// <summary>施耐德从机模式单元测试</summary>
public class SchneiderSlaveTests
{
    [Fact]
    [DisplayName("Slave_默认属性_初始值正确")]
    public void DefaultProperties_AreCorrect()
    {
        var slave = new SchneiderSlave();

        Assert.Equal(502, slave.Port);
        Assert.NotNull(slave.DeviceInfo);
        Assert.Equal("NewLife", slave.DeviceInfo.VendorName);
        Assert.Equal("SchneiderPLC", slave.DeviceInfo.ProductCode);
    }

    [Fact]
    [DisplayName("Slave_WriteTag_MW_然后ReadTag_返回正确值")]
    public void WriteTag_MW_ThenReadTag_ReturnsCorrectValue()
    {
        var slave = new SchneiderSlave();

        slave.WriteTag("MW100", (UInt16)42);
        var result = slave.ReadTag("MW100");

        Assert.Equal((UInt16)42, result);
    }

    [Fact]
    [DisplayName("Slave_WriteTag_MD_然后ReadTag_返回正确值")]
    public void WriteTag_MD_ThenReadTag_ReturnsCorrectValue()
    {
        var slave = new SchneiderSlave();

        slave.WriteTag("MD200", (UInt32)0x12345678);
        var result = slave.ReadTag("MD200");

        Assert.Equal((UInt32)0x12345678, result);
    }

    [Fact]
    [DisplayName("Slave_WriteTag_MF_然后ReadTag_返回正确值")]
    public void WriteTag_MF_ThenReadTag_ReturnsCorrectValue()
    {
        var slave = new SchneiderSlave();

        slave.WriteTag("MF100", 3.14159f);
        var result = slave.ReadTag("MF100");

        Assert.Equal(3.14159f, (Single)result, precision: 5);
    }

    [Fact]
    [DisplayName("Slave_WriteTag_M位_然后ReadTag_返回正确值")]
    public void WriteTag_M_Bit_ThenReadTag_ReturnsCorrectValue()
    {
        var slave = new SchneiderSlave();

        slave.WriteTag("M0.1", true);
        var result = slave.ReadTag("M0.1");

        Assert.Equal(true, result);
    }

    [Fact]
    [DisplayName("Slave_WriteTag_M位_False_然后ReadTag_返回False")]
    public void WriteTag_M_BitFalse_ThenReadTag_ReturnsFalse()
    {
        var slave = new SchneiderSlave();

        slave.WriteTag("M0.1", false);
        var result = slave.ReadTag("M0.1");

        Assert.Equal(false, result);
    }

    [Fact]
    [DisplayName("Slave_未写入的寄存器_返回默认值0")]
    public void ReadTag_UnwrittenRegister_ReturnsZero()
    {
        var slave = new SchneiderSlave();

        var result = slave.ReadTag("MW999");

        Assert.Equal((UInt16)0, result);
    }

    [Fact]
    [DisplayName("Slave_WriteTag_QW_然后ReadTag_返回正确值")]
    public void WriteTag_QW_ThenReadTag_ReturnsCorrectValue()
    {
        var slave = new SchneiderSlave();

        slave.WriteTag("QW50", (UInt16)255);
        var result = slave.ReadTag("QW50");

        Assert.Equal((UInt16)255, result);
    }

    [Fact]
    [DisplayName("Slave_WriteTag_Int32_然后ReadTag_返回正确值")]
    public void WriteTag_Int32_ThenReadTag_ReturnsCorrectValue()
    {
        var slave = new SchneiderSlave();

        slave.WriteTag("MD300", (Int32)12345);
        var result = slave.ReadTag("MD300");

        Assert.Equal((UInt32)12345, result);
    }
}
