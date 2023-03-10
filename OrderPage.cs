namespace Sfan;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class OrderPage : LabelPage
{
    public OrderPage(ChromeDriver driver, AppConfig config) : base(driver, config, 2, "tab-1002")
    {
        this.MaxPage = config.RechargeMaxPage;
    }
}