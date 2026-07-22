using System.ComponentModel;
using NewLife.IoT.Drivers;
using NewLife.Serialization;

namespace NewLife.Schneider.Drivers;

/// <summary>施耐德PLC参数 / Schneider PLC parameters</summary>
public class SchneiderParameter : ModbusTcpParameter
{
    #region 序列化
    /// <summary>序列化为JSON字符串 / Serialize to JSON string</summary>
    /// <returns>JSON字符串 / JSON string</returns>
    /// <example>
    /// <code>
    /// var pm = new SchneiderParameter { Host = 1, Server = "192.168.1.100:502" };
    /// var json = pm.ToJson();
    /// Console.WriteLine(json);
    /// // {"Host":1,"Server":"192.168.1.100:502","ReadCode":3,"WriteCode":16}
    /// </code>
    /// </example>
    public String ToJson() => JsonHelper.ToJson(this, false, false, true);

    /// <summary>从JSON字符串反序列化 / Deserialize from JSON string</summary>
    /// <param name="json">JSON字符串 / JSON string</param>
    /// <returns>反序列化后的参数对象 / Deserialized parameter object</returns>
    /// <exception cref="ArgumentNullException">json为null时抛出 / Thrown when json is null</exception>
    /// <example>
    /// <code>
    /// var json = @"{""Host"":1,""Server"":""192.168.1.100:502""}";
    /// var pm = SchneiderParameter.FromJson(json);
    /// Console.WriteLine(pm.Server); // "192.168.1.100:502"
    /// </code>
    /// </example>
    public static SchneiderParameter FromJson(String json)
    {
        if (String.IsNullOrEmpty(json)) throw new ArgumentNullException(nameof(json));

        return JsonHelper.ToJsonEntity<SchneiderParameter>(json);
    }
    #endregion
}