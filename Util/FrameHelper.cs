using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Sfan.Util;
public class FrameHelper : IDisposable
{
    private readonly ChromeDriver _driver;

    public FrameHelper(ChromeDriver drv, string id)
    {
        this._driver = drv;
        var e = drv.FindElement(By.XPath("//iframe[@id='" +
                                         id +
                                         "']"));
        drv.SwitchTo().Frame(e);
    }

    public void Dispose()
    {
        _driver.SwitchTo().DefaultContent();
    }
}