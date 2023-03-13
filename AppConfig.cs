namespace Sfan;

using YamlDotNet.Serialization;

public class AppConfig
{
    // 最少下单数
    public int TotalMin { get; set; }
    
    // 处理权重范围
    public int[] WeightsRang { get; set; } = new int[] { };
    
    // 处理的支付方式
    public string[] PayTypes { get; set; } = new string[] { };

    // 审核配置文件
    public string ReviewFile { get; set; } = string.Empty;
    
    // Chrome
    public bool Headless { get; set; }
    public string WindowSize { get; set; } = string.Empty;

    public AppConfig()
    {
    }

    public static AppConfig FromYamlFile(string path)
    {
        string yml = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder().Build();
        var cnf = deserializer.Deserialize<AppConfig>(yml);
        return cnf;
    }

}