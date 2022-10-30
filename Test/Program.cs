using NewLife;
using NewLife.IoT.Drivers;
using NewLife.IoT.Protocols;
using NewLife.IoT.ThingModels;
using NewLife.Schneider.Drivers;

Console.WriteLine("服务端地址默认为：127.0.0.1:502，保持默认请回车开始连接，否则请输入服务端地址：");
var address = Console.ReadLine();

if (address == null || address == "") address = "127.0.0.1:502";

var driver = new SchneiderDriver();
var pm = new SchneiderParameter
{
    Host = 1,
    Server = address,
    ReadCode = FunctionCodes.ReadCoil,
    WriteCode = FunctionCodes.WriteCoil
};
var node = driver.Open(null, pm);

// 测试打开两个通道
node = driver.Open(null, pm);

Console.WriteLine($"连接成功=>{address}！");

Console.WriteLine($"读写模式输入1，循环读输入2：");

var mode = Console.ReadLine();

var str = "0";

if (mode == "1")
{
    Console.WriteLine("请输入整数值，按q退出：");
    str = Console.ReadLine();
}

var point = new Point
{
    Name = "test",
    Address = "100",
    Type = "UInt16",
    Length = 2
};

do
{
    if (mode == "1")
    {
        // 写入
        var data = BitConverter.GetBytes(Int32.Parse(str));

        var res = driver.Write(node, point, data);

        // 读取
        var dic = driver.Read(node, new[] { point });
        var data1 = dic[point.Name] as Byte[];

        Console.WriteLine($"读取结果：{BitConverter.ToInt32(data1)}");
        Console.WriteLine($"");
        Console.WriteLine("请输入整数值，按q退出：");
    }
    else
    {
         // 读取
        var dic = driver.Read(node, new[] { point });
        var data1 =  dic[point.Name] as Byte[];
        //var res = BitConverter.ToInt32(data1);
        var res = data1?.Swap(true, false).ToInt();
        Console.WriteLine($"读取结果：{res}");
        Console.WriteLine($"");
        Thread.Sleep(1000);
    }
} while (
(mode == "1" && (str = Console.ReadLine()) != "q")
|| mode == "2");

// 断开连接
driver.Close(node);
driver.Close(node);


public class Point : IPoint
{
    public String Name { get; set; }
    public String Address { get; set; }
    public String Type { get; set; }
    public Int32 Length { get; set; }
}