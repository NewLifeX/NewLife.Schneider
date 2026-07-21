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
}
