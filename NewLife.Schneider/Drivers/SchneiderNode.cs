using NewLife.IoT;
using NewLife.IoT.Drivers;

namespace NewLife.Schneider.Drivers;

/// <summary>施耐德PLC连接节点。封装单次设备连接状态，实现 INode 接口</summary>
public class SchneiderNode : INode
{
    /// <summary>主机地址</summary>
    public String Address { get; set; }

    /// <summary>通道</summary>
    public IDriver Driver { get; set; }

    /// <summary>设备</summary>
    public IDevice Device { get; set; }

    /// <summary>参数</summary>
    public IDriverParameter Parameter { get; set; }

    /// <summary>是否已连接。驱动维护的连接状态，无需触发一次采集才能感知连接健康</summary>
    public Boolean IsConnected { get; set; }
}