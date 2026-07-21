using System.ComponentModel;
using NewLife.IoT;
using NewLife.IoT.Drivers;
using NewLife.Schneider.Drivers;
using Xunit;

namespace XUnitTest.Drivers;

/// <summary>施耐德连接节点单元测试</summary>
public class SchneiderNodeTests
{
    [Fact]
    [DisplayName("Node_默认属性_初始值为空")]
    public void DefaultProperties_AreNullOrZero()
    {
        var node = new SchneiderNode();
        Assert.Null(node.Address);
        Assert.Null(node.Driver);
        Assert.Null(node.Device);
        Assert.Null(node.Parameter);
        Assert.False(node.IsConnected);
    }

    [Fact]
    [DisplayName("Node_设置属性_正确读取")]
    public void SetProperties_AreReadBack()
    {
        var node = new SchneiderNode
        {
            Address = "192.168.1.100:502",
            IsConnected = true,
        };

        Assert.Equal("192.168.1.100:502", node.Address);
        Assert.True(node.IsConnected);
    }

    [Fact]
    [DisplayName("Node_实现INode接口_成功")]
    public void Implements_INode()
    {
        var node = new SchneiderNode();
        Assert.IsAssignableFrom<INode>(node);
    }
}
