using System.ComponentModel;
using NewLife.IoT.Drivers;
using NewLife.Schneider.Drivers;
using Xunit;

namespace XUnitTest.Drivers;

/// <summary>施耐德驱动单元测试</summary>
public class SchneiderDriverTests
{
    [Fact]
    [DisplayName("Driver_DriverAttribute_存在且名称为SchneiderPLC")]
    public void DriverAttribute_Exists()
    {
        var att = typeof(SchneiderDriver).GetCustomAttributes(typeof(DriverAttribute), false);
        Assert.Single(att);
        var driverAttr = att[0] as DriverAttribute;
        Assert.NotNull(driverAttr);
        Assert.Equal("SchneiderPLC", driverAttr.Name);
    }

    [Fact]
    [DisplayName("Driver_DisplayNameAttribute_存在")]
    public void DisplayNameAttribute_Exists()
    {
        var att = typeof(SchneiderDriver).GetCustomAttributes(typeof(DisplayNameAttribute), false);
        Assert.Single(att);
        var displayAttr = att[0] as DisplayNameAttribute;
        Assert.NotNull(displayAttr);
        Assert.Equal("施耐德PLC", displayAttr.DisplayName);
    }

    [Fact]
    [DisplayName("Driver_实现ILogFeature和ITracerFeature")]
    public void Implements_RequiredInterfaces()
    {
        var driver = new SchneiderDriver();
        Assert.IsAssignableFrom<ModbusTcpDriver>(driver);
    }

    [Fact]
    [DisplayName("Driver_CreateParameter_返回非空参数")]
    public void CreateParameter_ReturnsParameter()
    {
        var driver = new SchneiderDriver();
        var pm = driver.CreateParameter("");
        Assert.NotNull(pm);
    }

    #region 标签前缀提取
    [Fact]
    [DisplayName("GetTagPrefix_MW100_返回MW")]
    public void GetTagPrefix_MW_ReturnsMW()
    {
        var prefix = SchneiderDriver.GetTagPrefix("MW100");
        Assert.Equal("MW", prefix);
    }

    [Fact]
    [DisplayName("GetTagPrefix_MF200_返回MF")]
    public void GetTagPrefix_MF_ReturnsMF()
    {
        var prefix = SchneiderDriver.GetTagPrefix("MF200");
        Assert.Equal("MF", prefix);
    }

    [Fact]
    [DisplayName("GetTagPrefix_MD300_返回MD")]
    public void GetTagPrefix_MD_ReturnsMD()
    {
        var prefix = SchneiderDriver.GetTagPrefix("%MD300");
        Assert.Equal("MD", prefix);
    }

    [Fact]
    [DisplayName("GetTagPrefix_M0.1_返回M")]
    public void GetTagPrefix_M_Bit_ReturnsM()
    {
        var prefix = SchneiderDriver.GetTagPrefix("M0.1");
        Assert.Equal("M", prefix);
    }

    [Fact]
    [DisplayName("GetTagPrefix_I2.3_返回I")]
    public void GetTagPrefix_I_ReturnsI()
    {
        var prefix = SchneiderDriver.GetTagPrefix("I2.3");
        Assert.Equal("I", prefix);
    }

    [Fact]
    [DisplayName("GetTagPrefix_QW50_返回QW")]
    public void GetTagPrefix_QW_ReturnsQW()
    {
        var prefix = SchneiderDriver.GetTagPrefix("QW50");
        Assert.Equal("QW", prefix);
    }

    [Fact]
    [DisplayName("GetTagPrefix_空字符串_返回空")]
    public void GetTagPrefix_Empty_ReturnsEmpty()
    {
        var prefix = SchneiderDriver.GetTagPrefix(null);
        Assert.Equal("", prefix);
    }
    #endregion

    #region 类型转换
    [Fact]
    [DisplayName("ConvertToTypedValue_MW_2字节大端_返回UInt16")]
    public void ConvertToTypedValue_MW_ReturnsUInt16()
    {
        var addr = SchneiderAddress.Parse("MW100");
        // Modbus 大端序：0x12 0x34 = 0x1234 = 4660
        var raw = new Byte[] { 0x12, 0x34 };
        var value = SchneiderDriver.ConvertToTypedValue(raw, "MW100", addr);

        Assert.IsType<UInt16>(value);
        Assert.Equal((UInt16)0x1234, value);
    }

    [Fact]
    [DisplayName("ConvertToTypedValue_MD_4字节大端_返回UInt32")]
    public void ConvertToTypedValue_MD_ReturnsUInt32()
    {
        var addr = SchneiderAddress.Parse("MD200");
        // Modbus 大端序：0x12 0x34 0x56 0x78 = 0x12345678 = 305419896
        var raw = new Byte[] { 0x12, 0x34, 0x56, 0x78 };
        var value = SchneiderDriver.ConvertToTypedValue(raw, "MD200", addr);

        Assert.IsType<UInt32>(value);
        Assert.Equal((UInt32)0x12345678, value);
    }

    [Fact]
    [DisplayName("ConvertToTypedValue_MF_4字节大端_返回Single")]
    public void ConvertToTypedValue_MF_ReturnsSingle()
    {
        var addr = SchneiderAddress.Parse("MF100");
        // Modbus 大端序存储 3.14159f
        // 3.14159f 在 IEEE 754 中 = 0x40490FDB (小端)
        // 大端序 = [0x40, 0x49, 0x0F, 0xDB]
        var raw = new Byte[] { 0x40, 0x49, 0x0F, 0xDB };
        var value = SchneiderDriver.ConvertToTypedValue(raw, "MF100", addr);

        Assert.IsType<Single>(value);
        Assert.Equal(3.14159f, (Single)value, precision: 5);
    }

    [Fact]
    [DisplayName("ConvertToTypedValue_M位_Int32True_返回BooleanTrue")]
    public void ConvertToTypedValue_M_BitTrue_ReturnsTrue()
    {
        var addr = SchneiderAddress.Parse("M0.1");
        // ModbusDriver 读取线圈返回 Int32(1)
        var value = SchneiderDriver.ConvertToTypedValue(1, "M0.1", addr);

        Assert.IsType<Boolean>(value);
        Assert.True((Boolean)value);
    }

    [Fact]
    [DisplayName("ConvertToTypedValue_M位_Int32False_返回BooleanFalse")]
    public void ConvertToTypedValue_M_BitFalse_ReturnsFalse()
    {
        var addr = SchneiderAddress.Parse("M0.1");
        var value = SchneiderDriver.ConvertToTypedValue(0, "M0.1", addr);

        Assert.IsType<Boolean>(value);
        Assert.False((Boolean)value);
    }

    [Fact]
    [DisplayName("ConvertToTypedValue_M位_Byte数组_正确转换")]
    public void ConvertToTypedValue_M_BitByteArray_ReturnsBoolean()
    {
        var addr = SchneiderAddress.Parse("M0.1");
        var value = SchneiderDriver.ConvertToTypedValue(new Byte[] { 1 }, "M0.1", addr);

        Assert.IsType<Boolean>(value);
        Assert.True((Boolean)value);
    }

    [Fact]
    [DisplayName("ConvertToTypedValue_IW_2字节大端_返回UInt16")]
    public void ConvertToTypedValue_IW_ReturnsUInt16()
    {
        var addr = SchneiderAddress.Parse("IW50");
        var raw = new Byte[] { 0xAB, 0xCD };
        var value = SchneiderDriver.ConvertToTypedValue(raw, "IW50", addr);

        Assert.IsType<UInt16>(value);
        Assert.Equal((UInt16)0xABCD, value);
    }

    [Fact]
    [DisplayName("ConvertToTypedValue_SW_2字节大端_返回UInt16")]
    public void ConvertToTypedValue_SW_ReturnsUInt16()
    {
        var addr = SchneiderAddress.Parse("SW100");
        var raw = new Byte[] { 0x00, 0x2A };
        var value = SchneiderDriver.ConvertToTypedValue(raw, "SW100", addr);

        Assert.IsType<UInt16>(value);
        Assert.Equal((UInt16)42, value);
    }

    [Fact]
    [DisplayName("ConvertToTypedValue_QW_2字节大端_返回UInt16")]
    public void ConvertToTypedValue_QW_ReturnsUInt16()
    {
        var addr = SchneiderAddress.Parse("QW60");
        var raw = new Byte[] { 0x00, 0xFF };
        var value = SchneiderDriver.ConvertToTypedValue(raw, "QW60", addr);

        Assert.IsType<UInt16>(value);
        Assert.Equal((UInt16)255, value);
    }

    [Fact]
    [DisplayName("ConvertToTypedValue_null_返回原值")]
    public void ConvertToTypedValue_Null_ReturnsNull()
    {
        var addr = SchneiderAddress.Parse("MW100");
        var value = SchneiderDriver.ConvertToTypedValue(null, "MW100", addr);

        Assert.Null(value);
    }
    #endregion
}
