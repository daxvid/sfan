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

    // 成功率
    public decimal RateDecimal => Total == 0 ? 0 : (Success * 100 / Total);

    public int Rate { get; set; }

    public static RechargeStatistics Parse(string value)
    {
        var r = new RechargeStatistics();
        var values = value.Split(new char[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
        r.Rate = int.Parse(values[0][..^1]);
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

    public static RechargeStatistics[] ParseList(string value)
    {
        var items = value.Split('，', StringSplitOptions.RemoveEmptyEntries);
        var rs = new List<RechargeStatistics>(items.Length);
        foreach (var item in items)
        {
            rs.Add(Parse(item));
        }

        return rs.ToArray();
    }
}

public class RechargeItem : IComparable<RechargeItem>
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
    public string Code { get; set; } = string.Empty;

    // 费率
    public decimal Rate { get; set; }

    // 成功率(48% 20/41 30m，55% 45/81 1h，51% 275/535 24h)
    private string SuccessRateStr { get; set; } = string.Empty;

    // 总额
    public int Total { get; set; }

    // 余额
    public int Balance { get; set; }

    public RechargeStatistics[] SuccessRates { get; set; } = new RechargeStatistics[] { };

    // 所在页码
    public int Page { get; set; }
    
    // 新的权重
    public int NewWeights { get; set; }

    // 同等级中费率排行
    public int GradeRank { get; set; }

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

    // 成功率1-20%，权重1（等通道优化完毕，或过30分钟后加权重测试）
    // 成功率20-30，权重100
    // 成功率30-40，权重300
    // 成功率40-50，权重500
    // 成功率50以上，权重1000
    private static int[] GradePowerList = new[] { 0, 1, 100, 300, 500, 1000 };

    // 30-200   100-200  区间的金额   权重按之前说的来区分
    // 500-1000 区间的金额 成功率在40-50以上给500权重，成功率在30-40给300权重，20-30给100权重，20-10权重给50，10以下给1权重
    // 500-5000 还有  数字人民币 权重   40-50以上300权重，20-40权重100，20-10权重给50，10以下给1权重
    
    // 权重分为5个等级
    public int Grade => (SuccessRates == null ? 0 : SuccessRates[0].Rate) switch
    {
        (>= 0 and <= 20) => 1,
        (> 20 and <= 30) => 2,
        (> 30 and <= 40) => 3,
        (> 40 and <= 50) => 4,
        (> 50) => 5,
        _ => 0
    };

    public int CompareTo(RechargeItem that)
    {
        if (object.ReferenceEquals(this, that))
        {
            return 0;
        }

        // 先按等级排，再按费率排
        var a = this.Grade;
        var b = that.Grade;
        var r = -a.CompareTo(b);
        if (r == 0)
        {
            r = this.Rate.CompareTo(that.Rate);
        }

        return r;
    }

    static int ReadInputInt(IWebElement e)
    {
        var txt = e.FindElement(By.XPath("./input")).GetAttribute("value");
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

        var log = new RechargeItem();

        log.Id = (int)(Helper.ReadDecimal(ts[1])); // ֵIDֵ
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

        span.Msg = "记录:" + log.Id;
        return log;
    }
}