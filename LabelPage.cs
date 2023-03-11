namespace Sfan;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class LabelPage : PageBase
{
    private readonly int labIndex;
    private readonly string labId;

    protected LabelPage(ChromeDriver driver, AppConfig config, int index, string id) : base(driver, config)
    {
        this.labIndex = index;
        this.labId = id;
    }

    public override bool Open()
    {
        return GoToPage(labIndex, labId);
    }
    
    protected bool GoToPage(int index, string item)
    {
        // <i class="el-submenu__icon-arrow el-icon-arrow-down"></i>
        // //*[@id='main']/section/aside/ul/div/li[2]/div/i[2]
        // //*[@id='main']/section/aside/ul/div/li[3]/div/i[2]
        var liPath = ".//div[@id='main']/section/aside/ul/div/li[" + index.ToString() + "]";
        var li = FindElementByXPath(liPath);
        var className = li.GetAttribute("class");
        if (className == "el-submenu")
        {
            const string path = "./div/i[@class='el-submenu__icon-arrow el-icon-arrow-down']";
            FindAndClickByXPath(li, path, 100);
        }

        // <span>1) 充值订单</span>
        // <span>2) 充值方式</span>
        // //div[@id="main"]/section/aside/ul/div/li[2]/ul/div/li
        // //div[@id='main']/section/aside/ul/div/li[3]/ul/div/li
        FindAndClickByXPath( liPath + "/ul/div/li", 10);
        return true;
    }


    public override void Close()
    {
        // <span class="el-icon-close"></span>
        // //*[@id="tab-1002"]/span[2]
        // //*[@id="tab-1004"]/span[2]
        var path = "//*[@id='" +
                   labId +
                   "']/span[@class='el-icon-close']";
        // 关闭窗口
        try
        {
            wait.Until(drv =>
            {
                var btn = drv.FindElement(By.XPath(path));
                if (btn.Enabled && btn.Displayed)
                {
                    btn.Click();
                    Thread.Sleep(10);
                    return true;
                }

                return false;
            });
        }
        catch
        {
        }
    }
}