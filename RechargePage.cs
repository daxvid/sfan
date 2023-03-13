using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Sfan.Util;

namespace Sfan;

public class RechargePage : LabelPage
{
    private const string FrameId = "1004";
    private int pageIndex = 1;

    public RechargePage(ChromeDriver driver, AppConfig config) : base(driver, config, 3, "tab-" + FrameId)
    {
        // //*[@id="tab-1004"]/span[2]
    }
    
    private void SetItem(string way)
    {
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

    public List<RechargeItem> Select(string way)
    {
        using var _ = new FrameHelper(Driver, FrameId);
        
        SetItem(way);

        // //*[@id="changelist-search"]/div/button
        FindAndClickByXPath("//div/button/span[text()='搜索']", 5000);
        
        // 总条数
        // <span class="el-pagination__total">
        // //*[@id="pagination"]/div/span[1]
        var itemEmt = FindElementByXPath("//*[@id='pagination']/div/span[@class='el-pagination__total']");
        var itemTxt = Helper.ReadString(itemEmt);
        var rowCount = int.Parse(itemTxt[2..^2]);
        
        pageIndex = 1;
        var items = new List<RechargeItem>(rowCount);

        ReadRows(items);

        // 下一页
        // //div[@id="pagination"]/div/button[2]
        const string nextPath = "//div[@id='pagination']/div/button[2]";
        var nextPage = FindElementByXPath(nextPath);
        while (nextPage.Enabled)
        {
            SafeClick(nextPage, 10);
            pageIndex++;

            ReadRows(items);

            nextPage = FindElementByXPath(nextPath);
        }

        return items;
    }

    private int ReadRows(List<RechargeItem> items)
    {
        // //form[@id="changelist-form"]
        const string rowPath = ".//form[@id='changelist-form']/div/table[@id='result_list']/tbody/tr";
        checkPage(pageIndex);
        var rows = FindElementsByXPath(rowPath);
        foreach (var row in rows)
        {
            var item = RechargeItem.Create(row);
            item.Page = pageIndex;
            items.Add(item);
        }

        return rows.Count;
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

    private void SavePage()
    {
        // //*[@id="changelist-form"]/div[2]/button[3]/span[text()='保存']
        const string path = ".//form[@id='changelist-form']/div[2]/button[3]/span[text()='保存']";
        var btn = FindElementByXPath(path);
        // SafeClick(btn,1000);
    }

    public int SaveWeights(List<RechargeItem> items)
    {
        using var _ = new FrameHelper(Driver, FrameId);
        int count = SetWeights(items);
        // 上一页
        // //*[@id="pagination"]/div/button[1]
        const string nextPath = "//div[@id='pagination']/div/button[1]";
        var nextPage = FindElementByXPath(nextPath);
        while (nextPage.Enabled && nextPage.Displayed)
        {
            SafeClick(nextPage, 1000);
            pageIndex--;
            count += SetWeights(items);
            nextPage = FindElementByXPath(nextPath);
        }
        return count;
    }

    private int SetWeights(List<RechargeItem> items)
    {
        int count = 0;
        foreach (var item in items)
        {
            if (item.Page == pageIndex)
            {
                if (SetValue(item))
                {
                    count++;
                }
            }
        }

        if (count > 0)
        {
            // 点击保存
            SavePage();
        }

        return count;
    }
    
    private bool SetValue(RechargeItem item)
    {
        // //table[@id="result_list"]/tbody/tr[1]/th/a
        var path = ".//table[@id='result_list']/tbody/tr/th/a[text()='" +
                   item.Id.ToString() +
                   "']/../../td[6]/input";
        var inputs = FindElementsByXPath(path);
        if (inputs.Count != 1)
        {
            return false;
        }

        var input = inputs[0];
        var x = input.GetAttribute("value");
        input.Clear();
        input.SendKeys(item.NewWeights.ToString());
        return true;
    }
}