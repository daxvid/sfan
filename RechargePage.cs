using Sfan.Util;

namespace Sfan;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class RechargePage : LabelPage
{
    public RechargePage(ChromeDriver driver, AppConfig config) : base(driver, config, 3, "tab-1004")
    {
        // //*[@id="tab-1004"]/span[2]
        this.MaxPage = config.RechargeMaxPage;
    }

    public void SetItem(int index, string way)
    {
        Driver.SwitchTo().Frame(0);
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
                                          index.ToString()+
                                          "]/span[text()='" +
                                          way +
                                          "']", 100);
            }
        }
        
        // //*[@id="changelist-search"]/div/button
        
        FindAndClickByXPath("//div/button/span[text()='搜索']", 5000);
        
        Driver.SwitchTo().ParentFrame();
    }
}