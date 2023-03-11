using OpenQA.Selenium.Chrome;
using Sfan.Util;

namespace Sfan;

public class RechargePage : LabelPage
{
    private const string FrameId = "1004";

    public RechargePage(ChromeDriver driver, AppConfig config) : base(driver, config, 3, "tab-" + FrameId)
    {
        // //*[@id="tab-1004"]/span[2]
        this.MaxPage = config.RechargeMaxPage;
    }


    public void SetItem(string way)
    {
        using var _ = new FrameHelper(Driver, FrameId);
        WayIndex.TryGetValue(way, out var index);
        var form = FindElementByXPath(".//form[@id='changelist-search']/div[@class='simpleui-form']");

        // //*[@id="changelist-search"]/div/div[1]/div/input
        var input1 = FindElementByXPath(form, "./div[1]/div/input");
        var hit = input1.GetAttribute("value");
        if (string.IsNullOrEmpty(hit))
        {
            // //*[@id="changelist-search"]/div/div[1]/div/span/span/i
            // FindAndClickByXPath(form, "./div[1]/div[1]/span/span/i", 100); 
            input1.Click();
            Thread.Sleep(100);
            FindAndClickByXPath("//div[@class='el-scrollbar']/div/ul/li[1]/span[text()='是']", 100);
        }

        // //*[@id="changelist-search"]/div/div[4]/div/input
        var input4 = FindElementByXPath(form, "./div[4]/div/input");
        var hit4 = input4.GetAttribute("value");
        if (hit4 != way)
        {
            if (string.IsNullOrEmpty(way))
            {
                // //*[@id="changelist-search"]/div/div[4]/div/span/span/i
                FindAndClickByXPath(form, "./div[4]/div[1]/span/span/i", 100);
            }
            else
            {
                // 支付宝 /html/body/div[6]/div[1]/div[1]/ul/li[1]
                // 微信   /html/body/div[6]/div[1]/div[1]/ul/li[2]
                input4.Click();
                Thread.Sleep(1000);
                FindAndClickByXPath("//div[@class='el-scrollbar']/div/ul/li[" +
                                    index.ToString() +
                                    "]/span[text()='" +
                                    way +
                                    "']", 100);
            }
        }
    }

    private static readonly Dictionary<string, int> WayIndex = new Dictionary<string, int>()
    {
        [""] = 0,
        ["支付宝"] = 1,
        ["微信"] = 2,
        ["银行卡"] = 3,
        ["USDT"] = 4,
        ["银联"] = 5,
        ["数字人民币"] = 6,
        ["QQ钱包"] = 7
    };

    public List<RechargeItem> ReadData(string way)
    {
        SetItem(way);

        using var _ = new FrameHelper(Driver, FrameId);
        // //*[@id="changelist-search"]/div/button
        FindAndClickByXPath("//div/button/span[text()='搜索']", 5000);


        // <span class="el-pagination__total">
        // //*[@id="pagination"]/div/span[1]
        var itemEmt = FindElementByXPath("//*[@id='pagination']/div/span[@class='el-pagination__total']");
        var itemTxt = Helper.ReadString(itemEmt);
        var rowCount = int.Parse(itemTxt[2..^2]);
        var pageIndex = 1;

        // //form[@id="changelist-form"]
        const string rowPath = ".//form[@id='changelist-form']/div/table[@id='result_list']/tbody/tr";
        
        checkPage(pageIndex);
        var rows = FindElementsByXPath(rowPath);
        var items = new List<RechargeItem>(rowCount);
        foreach (var row in rows)
        {
            var item = RechargeItem.Create(row);
            items.Add(item);
        }

        // 下一页
        // //div[@id="pagination"]/div/button[2]
        const string nextPath = "//div[@id='pagination']/div/button[2]";
        var nextPage = FindElementByXPath(nextPath);
        while (nextPage.Enabled)
        {
            nextPage.Click();
            pageIndex++;
            checkPage(pageIndex);
            rows = FindElementsByXPath(rowPath);
            foreach (var row in rows)
            {
                var item = RechargeItem.Create(row);
                items.Add(item);
            }

            nextPage = FindElementByXPath(nextPath);
        }

        return items;
    }

    private bool checkPage(int page)
    {
        // <li class="number active">2</li>
        // //*[@id="pagination"]/div/ul/li[2]
        var path = "//div[@id='pagination']/div/ul/li[text()='" +
                   page +
                   "']";
        var e = FindElementByXPath(path);
        while (e.GetAttribute("class") != "number active")
        {
            Thread.Sleep(1000);
            e = FindElementByXPath(path);
        }

        return true;
    }
}