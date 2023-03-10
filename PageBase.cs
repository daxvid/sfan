namespace Sfan;

using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Sfan.Util;

public class PageBase : IDisposable
{
    protected readonly ChromeDriver Driver;
    protected readonly WebDriverWait wait;
    protected int MaxPage = 4;
    protected readonly AppConfig Config;

    private static readonly ReadOnlyCollection<IWebElement> EmptyElements =
        new ReadOnlyCollection<IWebElement>(new List<IWebElement>());

    protected PageBase(ChromeDriver driver, AppConfig config)
    {
        this.Config = config;
        this.Driver = driver;
        this.wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
    }

    protected bool SetTextElementByXPath(string path, string txt)
    {
        return SetTextElement(By.XPath(path), txt);
    }

    protected bool SetTextElement(By by, string txt)
    {
        var result = wait.Until(drv =>
        {
            try
            {
                var es = drv.FindElements(by);
                if (es.Count == 0)
                {
                    return false;
                }

                if (es.Count == 1)
                {
                    var r = es[0];
                    r.Clear();
                    r.SendKeys(txt);
                    return true;
                }
                else
                {
                    throw new MoreSuchElementException(by, "set text", es);
                }
            }
            catch (NoSuchElementException)
            {
            }

            return false;
        });
        return result;
    }

    protected bool SetTextElementByXPath(IWebElement e, string path, string txt)
    {
        return SetTextElement(e, By.XPath(path), txt);
    }

    protected bool SetTextElement(IWebElement e, By by, string txt)
    {
        var result = wait.Until(drv =>
        {
            try
            {
                var es = e.FindElements(by);
                if (es.Count == 0)
                {
                    return false;
                }

                if (es.Count == 1)
                {
                    var r = es[0];
                    r.Clear();
                    r.SendKeys(txt);
                    return true;
                }
                else
                {
                    throw new MoreSuchElementException(by, "set text", es);
                }
            }
            catch (NoSuchElementException)
            {
            }

            return false;
        });
        return result;
    }

    protected IWebElement FindElementByXPath(string path)
    {
        return FindElement(By.XPath(path));
    }

    protected IWebElement FindElement(By by)
    {
        return wait.Until(drv => drv.FindElement(by));
        IWebElement r = null;
        var result = wait.Until(drv =>
        {
            try
            {
                var es = drv.FindElements(by);
                if (es.Count == 0)
                {
                    return false;
                }

                if (es.Count == 1)
                {
                    r = es[0];
                    return true;
                }
                else
                {
                    throw new MoreSuchElementException(by, "find", es);
                }
            }
            catch (NoSuchElementException)
            {
            }

            return false;
        });
        return r;
    }

    protected IWebElement FindElementByXPath(IWebElement e, string path)
    {
        return FindElement(e, By.XPath(path));
    }

    protected IWebElement FindElement(IWebElement e, By by)
    {
        return wait.Until(drv => e.FindElement(by));
        IWebElement r = null;
        var result = wait.Until(drv =>
        {
            try
            {
                var es = e.FindElements(by);
                if (es.Count == 0)
                {
                    return false;
                }

                if (es.Count == 1)
                {
                    r = es[0];
                    return true;
                }
                else
                {
                    throw new MoreSuchElementException(by, "find", es);
                }
            }
            catch (NoSuchElementException)
            {
            }

            return false;
        });
        return r;
    }

    protected ReadOnlyCollection<IWebElement> FindElementsByXPath(string path)
    {
        return FindElements(By.XPath(path));
    }

    protected ReadOnlyCollection<IWebElement> FindElements(By by)
    {
        var result = wait.Until(drv =>
        {
            try
            {
                var es = drv.FindElements(by);
                return es;
            }
            catch (NoSuchElementException)
            {
            }

            return EmptyElements;
        });
        return result;
    }

    protected ReadOnlyCollection<IWebElement> FindElementsByXPath(IWebElement e, string path)
    {
        return FindElements(e, By.XPath(path));
    }

    protected ReadOnlyCollection<IWebElement> FindElements(IWebElement e, By by)
    {
        var result = wait.Until(drv => e.FindElements(by));
        return result;
    }

    protected void FindAndClickByXPath(string path, int ms)
    {
        FindAndClick(By.XPath(path), ms);
    }

    protected void FindAndClick(By by, int ms)
    {
        wait.Until(drv =>
        {
            var btn = drv.FindElement(by);
            btn.Click();
            Thread.Sleep(ms);
            return true;
        });
    }

    protected void FindAndClickByXPath(IWebElement e, string path, int ms)
    {
        FindAndClick(e, By.XPath(path), ms);
    }

    protected void FindAndClick(IWebElement e, By by, int ms)
    {
        wait.Until(drv =>
        {
            var btn = e.FindElement(by);
            btn.Click();
            Thread.Sleep(ms);
            return true;
        });
    }

    protected bool SafeClick(IWebElement btn, int ms = 0)
    {
        return wait.Until(drv =>
        {
            try
            {
                if (btn.Enabled)
                {
                    btn.Click();
                    Thread.Sleep(ms);
                }

                return true;
            }
            catch (ElementClickInterceptedException)
            {
            }

            return false;
        });
    }

    public virtual bool Open()
    {
        return true;
    }

    public virtual void Close()
    {
    }

    public virtual void SendMsg(string msg)
    {
        Helper.SendMsg(msg);
    }

    public static Dictionary<string, string> ReadHeadDic(IWebElement table)
    {
        var heads = table.FindElements(By.XPath(".//div[@class='ivu-table-header']/table/thead/tr/th"));
        var dicHead = new Dictionary<string, string>(29);
        foreach (var th in heads)
        {
            var tag = th.GetAttribute("class");
            if (!string.IsNullOrEmpty(tag))
            {
                var key = th.Text;
                dicHead.Add(key, tag);
            }
        }

        return dicHead;
    }

    public virtual void Dispose()
    {
        this.Close();
    }

    protected bool GoToNextPage(IWebElement table, int ms = 500)
    {
        // 检查是否有下一页
        var nextPage = FindElementByXPath(table,
            ".//button/span/i[@class='ivu-icon ivu-icon-ios-arrow-forward']/../..");
        var next = nextPage.Enabled;
        if (!next)
        {
            return false;
        }

        nextPage.Click();
        //TODO: 检查是否加载完成
        Thread.Sleep(ms);
        return true;
    }
}

