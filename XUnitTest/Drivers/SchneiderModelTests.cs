using System.ComponentModel;
using NewLife.Schneider.Drivers;
using Xunit;

namespace XUnitTest.Drivers;

/// <summary>施耐德型号预设单元测试</summary>
public class SchneiderModelTests
{
    [Fact]
    [DisplayName("GetModelInfo_M200_返回正确预设")]
    public void GetModelInfo_M200_ReturnsCorrectInfo()
    {
        var info = SchneiderModelHelper.GetModelInfo(SchneiderModel.M200);
        Assert.Equal("M200", info.Name);
        Assert.Equal(502, info.Port);
        Assert.Equal(3, info.ReadCode);
        Assert.Equal(16, info.WriteCode);
        Assert.Equal(256, info.MaxCoils);
        Assert.Equal(256, info.MaxRegisters);
    }

    [Fact]
    [DisplayName("GetModelInfo_M221_返回正确预设")]
    public void GetModelInfo_M221_ReturnsCorrectInfo()
    {
        var info = SchneiderModelHelper.GetModelInfo("M221");
        Assert.Equal("M221", info.Name);
        Assert.Equal(512, info.MaxRegisters);
    }

    [Fact]
    [DisplayName("GetModelInfo_M241_返回正确预设")]
    public void GetModelInfo_M241_ReturnsCorrectInfo()
    {
        var info = SchneiderModelHelper.GetModelInfo(SchneiderModel.M241);
        Assert.Equal("M241", info.Name);
        Assert.Equal(1024, info.MaxCoils);
        Assert.Equal(1024, info.MaxRegisters);
    }

    [Fact]
    [DisplayName("GetModelInfo_M251_返回正确预设")]
    public void GetModelInfo_M251_ReturnsCorrectInfo()
    {
        var info = SchneiderModelHelper.GetModelInfo(SchneiderModel.M251);
        Assert.Equal("M251", info.Name);
        Assert.Equal(2048, info.MaxCoils);
    }

    [Fact]
    [DisplayName("GetModelInfo_M258_返回正确预设")]
    public void GetModelInfo_M258_ReturnsCorrectInfo()
    {
        var info = SchneiderModelHelper.GetModelInfo(SchneiderModel.M258);
        Assert.Equal("M258", info.Name);
        Assert.Equal(4096, info.MaxRegisters);
    }

    [Fact]
    [DisplayName("GetModelInfo_M262_返回正确预设")]
    public void GetModelInfo_M262_ReturnsCorrectInfo()
    {
        var info = SchneiderModelHelper.GetModelInfo(SchneiderModel.M262);
        Assert.Equal("M262", info.Name);
        Assert.Equal(4096, info.MaxRegisters);
        Assert.Contains("IoT", info.Description);
    }

    [Fact]
    [DisplayName("GetAllModels_返回所有6个型号")]
    public void GetAllModels_ReturnsAllSixModels()
    {
        var models = SchneiderModelHelper.GetAllModels();
        Assert.Equal(6, models.Length);
        Assert.Contains(models, m => m.Name == "M200");
        Assert.Contains(models, m => m.Name == "M262");
    }

    [Fact]
    [DisplayName("GetModelInfo_未知型号名称_抛出异常")]
    public void GetModelInfo_UnknownName_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SchneiderModelHelper.GetModelInfo("UNKNOWN"));
    }

    [Theory]
    [DisplayName("GetModelInfo_所有型号_端口均为502")]
    [InlineData(SchneiderModel.M200)]
    [InlineData(SchneiderModel.M221)]
    [InlineData(SchneiderModel.M241)]
    [InlineData(SchneiderModel.M251)]
    [InlineData(SchneiderModel.M258)]
    [InlineData(SchneiderModel.M262)]
    public void GetModelInfo_AllModels_PortIs502(SchneiderModel model)
    {
        var info = SchneiderModelHelper.GetModelInfo(model);
        Assert.Equal(502, info.Port);
    }
}
