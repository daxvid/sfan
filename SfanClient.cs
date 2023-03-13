using System.Reflection.Metadata;

namespace Sfan;

using OpenQA.Selenium.Chrome;
using Sfan.Util;

public class SfanClient : IDisposable
{
    private readonly ChromeDriver driver;

    private readonly LoginPage loginPage;
    private RechargePage rechargePage;
    private OrderPage orderPage;

    private readonly AppConfig cnf;
    private readonly AuthConfig authCnf;
    private bool closed;

    public SfanClient(AppConfig cnf, AuthConfig authCnf)
    {
        this.cnf = cnf;
        this.authCnf = authCnf;
        this.driver = NewDriver(cnf.Headless, cnf.WindowSize);
        Cache.Init(authCnf.Redis, authCnf.Platform);

        loginPage = new LoginPage(driver, cnf, authCnf);
        rechargePage = new RechargePage(driver, cnf);
        orderPage = new OrderPage(driver, cnf);
    }

    public void Close()
    {
        if (closed)
        {
            return;
        }

        closed = true;
        driver.Quit();
    }

    public void SaveException(Exception err)
    {
        try
        {
            Log.SaveException(err, driver, string.Empty);
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        Close();
    }

    private static ChromeDriver NewDriver(bool headless, string windowSize)
    {
        var op = new ChromeOptions();

        if (headless)
        {
            // 为Chrome配置无头模式
            op.AddArgument("--headless");
        }

        if (!string.IsNullOrEmpty(windowSize))
        {
            //windowSize = "window-size=1366,768";
            op.AddArgument("window-size=" + windowSize);
        }
        //op.AddAdditionalChromeOption("excludeSwitches", new string[] { "enable-automation"});
        //op.AddAdditionalChromeOption("useAutomationExtension", false);

        var driver = new ChromeDriver(op);
        //var session = ((IDevTools)driver).GetDevToolsSession();
        return driver;
    }

    public void Run()
    {
        if (!loginPage.Login())
        {
            return;
        }

        var zeroCount = 0;
        DateTime heartbeatTime = DateTime.Now;
        while (true)
        {
            var count = LoadRecharge();
            //SendMsg("order count:" + orders.Count);
            if (count > 0)
            {
                zeroCount = 0;
                // ReviewOrders(orders);
                heartbeatTime = DateTime.Now;
            }
            else
            {
                Thread.Sleep((zeroCount > 20 ? 20 : zeroCount) * 1000);
                zeroCount++;
                var now = DateTime.Now;
                if ((now - heartbeatTime).TotalSeconds >= 60)
                {
                    heartbeatTime = now;
                    Helper.SendMsg(now.ToString("ok[HH:mm:ss]"));
                }
            }
        }
    }

    private Dictionary<string, List<RechargeItem>> Classify(List<RechargeItem> items)
    {
        Dictionary<string, List<RechargeItem>> dic = new Dictionary<string, List<RechargeItem>>();
        foreach (var item in items)
        {
            if (item.Weights < cnf.WeightsRang[0]
                || item.Weights > cnf.WeightsRang[1]
                || item.SuccessRates[0].Total < cnf.TotalMin
                || (!cnf.PayTypes.Contains(item.PayType)))
            {
                continue;
            }

            if (!dic.TryGetValue(item.PayType, out var list))
            {
                list = new List<RechargeItem>();
                dic.Add(item.PayType, list);
            }
            list.Add(item);
        }

        foreach (var itemList in dic.Values)
        {
            itemList.Sort();
            // GradeRank
            var rank = 0;
            decimal rate = -1;
            for (var i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                if (item.Rate != rate)
                {
                    rank = i + 1;
                    rate = item.Rate;
                }
                item.GradeRank = rank;
            }
        }

        return dic;
    }

    private int LoadRecharge()
    {
        rechargePage.Open();
        Thread.Sleep(15000);

        var items = rechargePage.Select("");
        // 过滤和分类
        var dic = Classify(items);

        var changeItems = new List<RechargeItem>();
        foreach (var kv in dic)
        {
            Proccess(kv.Key, kv.Value);
            foreach (var i in kv.Value)
            {
                if (i.Weights != i.NewWeights)
                {
                    changeItems.Add(i);
                }
            }
        }
        rechargePage.SaveWeights(changeItems);
        return changeItems.Count;
    }

    // 成功率→费率，费率差距不大，以成功率优先判别
    // 费率→成功率，成功率差距不大时，以费率优先判别
    // 成功率调控：确保启用通道成功率，刷新频率：20秒/次。
    // 成功率1-20%，权重1（等通道优化完毕，或过30分钟后加权重测试）
    // 成功率20-30，权重100
    // 成功率30-40，权重300
    // 成功率40-50，权重500
    // 成功率50以上，权重1000
    // （*以此类推进行参考调整）
    // 1，通道中的起使权重可以给100，除了QQ钱包～汇潮～NF952保持原有的权重，在调控过程中保持不变。
    // 2，当通道中有金额30-200的和100-200，在成功率都好的情况下，如果30-200费率超过19.5可以把30-200中的100-200金额切掉，当100-200成功率下降（20%-30%）时补回。
    // 3，当某个通道拉起4-5单订单没有成功，可以进行报警处理。
    // 4，当余额成为负数，可以进行报警处理。
    // 当通道权重降为1，看看半小时1小时的成功率将为0时，可以重新给100权重跑跑，不行的话报警切停
    // 卡卡和usdt的权重也不要改变
    private void Proccess(string key, List<RechargeItem> items)
    {
        foreach (var item in items)
        {
            item.NewWeights = item.Weights;
            if (item.Total > 0)
            {
                var pow = item.Grade;
                // TODO: 计算新的权，测试暂时+1
                item.NewWeights = item.Weights + 1;
            }
        }

    }
}