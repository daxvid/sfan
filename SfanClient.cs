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
            Log.SaveException(err, driver,string.Empty);
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
            var configs = LoadRecharge();
            //SendMsg("order count:" + orders.Count);
            if (configs.Count > 0)
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

    private List<RechargeItem> LoadRecharge()
    {
        rechargePage.Open();
        Thread.Sleep(10000);
        
        
        var items =  rechargePage.ReadData("");

        foreach (var key in new string[]{"支付宝","微信","银联","数字人民币","QQ钱包","" })
        {
            Thread.Sleep(10000);
            rechargePage.ReadData(key);
        }

        return items;
    }
}