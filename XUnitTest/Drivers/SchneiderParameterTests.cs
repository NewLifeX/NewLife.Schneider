using System.ComponentModel;
using NewLife.IoT.Protocols;
using NewLife.Schneider.Drivers;
using Xunit;

namespace XUnitTest.Drivers;

/// <summary>施耐德连接参数单元测试</summary>
public class SchneiderParameterTests
{
    [Fact]
    [DisplayName("Parameter_默认属性_初始值为默认")]
    public void DefaultProperties_AreDefault()
    {
        var pm = new SchneiderParameter();
        Assert.Equal((Byte)0, pm.Host);
        Assert.Null(pm.Server);
    }

    [Fact]
    [DisplayName("Parameter_设置属性_正确读取")]
    public void SetProperties_AreReadBack()
    {
        var pm = new SchneiderParameter
        {
            Host = 1,
            Server = "192.168.1.100:502",
            ReadCode = FunctionCodes.ReadRegister,
            WriteCode = FunctionCodes.WriteRegisters,
            Timeout = 5000,
        };

        Assert.Equal((Byte)1, pm.Host);
        Assert.Equal("192.168.1.100:502", pm.Server);
        Assert.Equal(FunctionCodes.ReadRegister, pm.ReadCode);
        Assert.Equal(FunctionCodes.WriteRegisters, pm.WriteCode);
        Assert.Equal(5000, pm.Timeout);
    }

    [Fact]
    [DisplayName("Parameter_设置Host_最大支持255")]
    public void Host_MaxValue_255()
    {
        var pm = new SchneiderParameter
        {
            Host = 255,
        };

        Assert.Equal((Byte)255, pm.Host);
    }

    #region 序列化
    [Fact]
    [DisplayName("ToJson_基本参数_产生有效JSON")]
    public void ToJson_ReturnsValidJson()
    {
        var pm = new SchneiderParameter
        {
            Host = 1,
            Server = "192.168.1.100:502",
        };

        var json = pm.ToJson();
        Assert.NotNull(json);
        Assert.Contains("192.168.1.100:502", json);
        Assert.Contains("host", json);
    }

    [Fact]
    [DisplayName("FromJson_有效JSON_还原参数")]
    public void FromJson_ValidJson_RestoresParameter()
    {
        var json = @"{""host"":2,""server"":""10.0.0.1:502""}";
        var pm = SchneiderParameter.FromJson(json);

        Assert.NotNull(pm);
        Assert.Equal((Byte)2, pm.Host);
        Assert.Equal("10.0.0.1:502", pm.Server);
    }

    [Fact]
    [DisplayName("FromJson_空字符串_抛出异常")]
    public void FromJson_EmptyString_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => SchneiderParameter.FromJson(null));
        Assert.Throws<ArgumentNullException>(() => SchneiderParameter.FromJson(""));
    }

    [Fact]
    [DisplayName("ToJson_FromJson_往返序列化一致")]
    public void ToJson_FromJson_Roundtrip()
    {
        var pm = new SchneiderParameter
        {
            Host = 1,
            Server = "192.168.1.100:502",
            ReadCode = FunctionCodes.ReadRegister,
            WriteCode = FunctionCodes.WriteRegisters,
            Timeout = 3000,
        };

        var json = pm.ToJson();
        var restored = SchneiderParameter.FromJson(json);

        Assert.Equal(pm.Host, restored.Host);
        Assert.Equal(pm.Server, restored.Server);
        Assert.Equal(pm.ReadCode, restored.ReadCode);
        Assert.Equal(pm.WriteCode, restored.WriteCode);
        Assert.Equal(pm.Timeout, restored.Timeout);
    }
    #endregion
}
