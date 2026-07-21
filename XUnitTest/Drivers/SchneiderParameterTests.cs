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
}
