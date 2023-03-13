namespace Sfan;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class OrderPage : LabelPage
{
    private const string FrameId = "1002";

    public OrderPage(ChromeDriver driver, AppConfig config) : base(driver, config, 2, "tab-" + FrameId)
    {
    }
}