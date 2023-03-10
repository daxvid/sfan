namespace Sfan;

using YamlDotNet.Serialization;

public class AppConfig
{

    // 订单时间小时
    public int OrderHour { get; set; }

    // 提现最多读取页数
    public int WithdrawMaxPage { get; set; }

    // 提现最多天数
    public int WithdrawMaxDay { get; set; }

    // 充值最多读取页数
    public int RechargeMaxPage { get; set; }

    // 充值最多天数
    public int RechargeMaxDay { get; set; }

    // 游戏记录最多读取页数
    public int GameLogMaxPage { get; set; }

    // 游戏记录最多小时
    public int GameLogMaxHour { get; set; }

    // 最大锁定单数
    public int OrderMaxLock { get; set; }
    
    // 处理金额范围
    public int[] AmountRang { get; set; } = new int[] { };

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