using OpenQA.Selenium;
using Sfan.Util;

namespace Sfan;

// 成功率(48% 20/41 30m，55% 45/81 1h，51% 275/535 24h)
public class RechargeStatistics
{
    // 成功的单数
    public decimal Success { get; set; }

    // 总单数
    public decimal Total { get; set; }

    // 统计的时长（分钟）
    public int Duration { get; set; }

    public decimal Rate => Total == 0 ? 100 : (Success / Total);

    public static RechargeStatistics Parse(string value)
    {
        var r = new RechargeStatistics();
        var values = value.Split(new char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
        r.Success = int.Parse(values[1]);
        r.Total = int.Parse(values[2]);

        var t = values[3];
        var m = t[^1] switch
        {
            'd' => 24 * 60,
            'h' => 60,
            'm' => 1,
            _ => 1
        };
        var d = t[..(^1)];
        r.Duration = m * int.Parse(d);
        return r;
    }

    public static List<RechargeStatistics> ParseList(string value)
    {
        var items = value.Split('，', StringSplitOptions.RemoveEmptyEntries);
        var rs = new List<RechargeStatistics>(items.Length);
        foreach (var item in items)
        {
            rs.Add(Parse(item));
        }

        return rs;
    }
}

public class RechargeItem
{
    // ID
    public int Id { get; set; }

    // 通道
    public string Way { get; set; } = string.Empty;

    // 支付方式
    public string PayType { get; set; } = string.Empty;

    // 支付下限
    public decimal PayMin { get; set; }

    // 支付上限
    public decimal PayMax { get; set; }

    // 权重
    public int Weights { get; set; }

    // 限额（总额/余额）
    private string Quota { get; set; } = string.Empty;

    // 代码
    public string Code { get; set; }

    // 费率
    public decimal Rate { get; set; }

    // 成功率(48% 20/41 30m，55% 45/81 1h，51% 275/535 24h)
    private string SuccessRateStr { get; set; } = string.Empty;
    
    // 总额
    public int Total { get; set; }
    // 余额
    public int Balance { get; set; }
    
    public List<RechargeStatistics> SuccessRates { get; set; }

    private static readonly string[] Heads = new string[]
    {
        "", "ֵID", "通道", "支付方式", "ֵ下限（元）", "上限（元）", "权重",
        "限额（总额/余额）", "代码", "费率", "权重控制", "成功率", "操作"
    };

    private void init()
    {
        var sp = Quota.IndexOf('/');
        Total = int.Parse(Quota[..sp]);
        Balance = int.Parse(Quota[(sp + 1)..]);
        SuccessRates = RechargeStatistics.ParseList(SuccessRateStr);
    }

    static int ReadInputInt(IWebElement e)
    {
        var txt =  e.FindElement(By.XPath("./input")).GetAttribute("value");
        var r = int.Parse(txt);
        return r;
    }
    
    public static RechargeItem Create(IWebElement element)
    {
        using var span = new Span();
        var ts = element.FindElements(By.XPath("./*"));
        if (ts.Count != Heads.Length)
        {
            throw new ArgumentException("RechargeItem Create");
        }

        RechargeItem log = new RechargeItem()
        {
        };
        try
        {
            log.Id = Helper.ReadInt(ts[1]); // ֵIDֵ
            log.Way = Helper.ReadString(ts[2]); //  通道
            log.PayType = Helper.ReadString(ts[3]); // 支付方式
            log.PayMin = ReadInputInt(ts[4]); // ֵ下限
            log.PayMax = ReadInputInt(ts[5]); // 上限
            log.Weights = ReadInputInt(ts[6]); // 权重

            log.Quota = Helper.ReadString(ts[7]); // 限额（总额/余额）
            log.Code = Helper.ReadString(ts[8]); // 代码
            log.Rate = Helper.ReadDecimal(ts[9]); // 费率
            log.SuccessRateStr = Helper.ReadString(ts[11]); // 成功率
            log.init();
        }
        catch (Exception err)
        {
            Console.WriteLine(err);
        }

        span.Msg = "记录:" + log.Id;
        return log;
    }
}