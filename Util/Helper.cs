namespace Sfan.Util;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Sfan.Bot;

public class Helper
{
    public static decimal GetDecimal(Dictionary<string, string> dic, string key)
    {
        string value = GetValue(dic, key);
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }

        decimal d = decimal.Parse(value);
        //decimal.TryParse(value, out d);
        return d;
    }


    public static string GetValue(Dictionary<string, string> dic, string key)
    {
        return dic.GetValueOrDefault(key, string.Empty);
    }

    public static decimal ReadDecimal(Dictionary<string, string> head, string key,
        Dictionary<string, IWebElement> dicCell)
    {
        string value = ReadString(head, key, dicCell);
        decimal d;
        decimal.TryParse(value, out d);
        return d;
    }

    public static DateTime ReadTime(Dictionary<string, string> head, string key,
        Dictionary<string, IWebElement> dicCell)
    {
        string value = ReadString(head, key, dicCell);
        DateTime d = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
        return d;
    }

    public static string ReadString(Dictionary<string, string> head, string key,
        Dictionary<string, IWebElement> dicCell)
    {
        if (head.TryGetValue(key, out string? className))
        {
            if (dicCell.TryGetValue(className??string.Empty, out IWebElement? cell))
            {
                var value = (cell!.Text??string.Empty).Trim();
                return value;
            }
        }
        return string.Empty;
    }

    public static Dictionary<string, IWebElement> Ele2Dic(IWebElement element)
    {
        var tdList = element.FindElements(By.XPath(".//td"));
        Dictionary<string, IWebElement> row = new Dictionary<string, IWebElement>(tdList.Count * 2);
        foreach (var td in tdList)
        {
            var key = td.GetAttribute("class")??string.Empty;
            row.Add(key, td);
        }

        return row;
    }

    // 设置查询的时间范围
    public static void SetTimeRang(IWebElement et, int hour)
    {
        et.Click();
        et.SendKeys(Keys.Control + "a");
        et.SendKeys(Keys.Delete);
        et.SendKeys(Keys.Command + "a");
        et.SendKeys(Keys.Delete);

        var now = DateTime.Now;
        string start = now.AddHours(-hour).ToString("yyyy-MM-dd HH:mm:ss");
        string end = now.ToString("yyyy-MM-dd 23:59:59");
        et.SendKeys(start + " - " + end);
        Thread.Sleep(10);
    }

    // 设置查询的日期范围
    public static void SetDayRang(IWebElement et, int day)
    {
        et.Click();
        et.SendKeys(Keys.Control + "a");
        et.SendKeys(Keys.Delete);
        et.SendKeys(Keys.Command + "a");
        et.SendKeys(Keys.Delete);

        var now = DateTime.Now;
        string start = now.AddDays(-(day - 1)).ToString("yyyy-MM-dd");
        string end = now.ToString("yyyy-MM-dd");
        et.SendKeys(start + " - " + end);
        Thread.Sleep(10);
    }


    public static bool TryClick(WebDriverWait wait, IWebElement btn)
    {
        if (btn.Enabled)
        {
            return wait.Until(driver =>
            {
                try
                {
                    btn.Click();
                    return true;
                }
                catch (WebDriverException)
                {
                }

                return false;
            });
        }

        return true;
    }



    public static decimal ReadBetDecimal(IWebElement e)
    {
        var txt = e.Text;
        var index = txt.IndexOf('：');
        txt = txt.Substring(index + 1);
        decimal r;
        decimal.TryParse(txt, out r);
        return r;
    }

    public static decimal ReadDecimal(IWebElement e)
    {
        var txt = ReadString(e);
        decimal r = decimal.Parse(txt);
        return r;
    }

    public static decimal ReadDecimalOrDefault(IWebElement e, decimal def = 0)
    {
        var txt = ReadString(e);
        decimal r;
        if (!decimal.TryParse(txt, out r))
        {
            r = def;
        }

        return r;
    }

    public static string ReadString(IWebElement e)
    {
        var txt = e.Text.Trim();
        if (txt == "--")
        {
            return string.Empty;
        }

        return txt;
    }

    public static DateTime ReadDateTime(IWebElement e)
    {
        var txt = ReadString(e);
        var r = DateTime.ParseExact(txt, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
        return r;
    }

    public static DateTime ReadShortTime(IWebElement e)
    {
        var str = Helper.ReadString(e);
        if (str.IndexOf('/') >= 0)
        {
            str = str.Replace("/", "-");
        }

        var now = DateTime.Now;
        for (var year = now.Year; year >= 2022; year--)
        {
            DateTime d = DateTime.ParseExact(year + "-" + str, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            if (d < now)
            {
                return d;
            }
        }

        return now;
    }

    static readonly List<char> hexSet = new List<char>()
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f'
    };

    // 判断十六进制字符串hex是否正确
    public static bool IsHexadecimal(string hex)
    {
        foreach (char item in hex)
        {
            if (hexSet.Contains<char>(item) == false)
            {
                return false;
            }
        }

        return true;
    }


    public static void SendMsg(string msg)
    {
        Console.WriteLine(msg);
        TelegramBot.SendMsg(msg);
    }

    public static T SafeExec<T>(ChromeDriver driver, Func<T> fun, int sleep = 1000, int tryCount = int.MaxValue)
    {
        var i = 0;
        while (true)
        {
            Exception? err = null;
            try
            {
                return fun();
            }
            catch (WebDriverException e)
            {
                err = e;
                if ((i >= tryCount) ||
                    (e is InvalidElementStateException ||
                     e is NotFoundException ||
                     e is WebDriverTimeoutException) == false)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                err = e;
                throw;
            }
            finally
            {
                if (i == 0 && err != null)
                {
                    Log.SaveException(err, driver);
                }
                i++;
            }
            Thread.Sleep(sleep);
        }
    }

    public static string EncryptMD5(string s)
    {
        var md5 = MD5.Create();
        return BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(s))).Replace("-","");
    }

    public static string? GetJsonValue(string key, string content)
    {
        string keyName = "\"" + key + "\"";
        int index = content.IndexOf(keyName);
        if (index > 0)
        {
            var i = index + keyName.Length;
            int start = content.IndexOf("\"", i, content.Length - i);
            i = start + 1;
            int end = content.IndexOf("\"", i, content.Length - i);
            var name = content.Substring(start + 1, end - start - 1);
            if (!string.IsNullOrEmpty(name))
            {
                name = System.Text.RegularExpressions.Regex.Unescape(name);
            }
            return name;
        }
        return null;
    }

    // 名字掩码处理，只保留最后一个字
    public static string MaskName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        var len = name.Length;
        var mask = new string('*', len-1);
        return mask + name[len - 1];
    }
}

