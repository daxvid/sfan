namespace Sfan.Util;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using Sfan.Bot;

public class Helper
{
    public static decimal GetDecimal(Dictionary<string, string> dic, string key)
    {
        var value = GetValue(dic, key);
        if (string.IsNullOrEmpty(value))
        {
            return 0;
        }

        var d = decimal.Parse(value);
        return d;
    }


    public static string GetValue(Dictionary<string, string> dic, string key)
    {
        return dic.GetValueOrDefault(key, string.Empty);
    }

    public static decimal ReadDecimal(Dictionary<string, string> head, string key,
        Dictionary<string, IWebElement> dicCell)
    {
        var value = ReadString(head, key, dicCell);
        decimal.TryParse(value, out var d);
        return d;
    }

    public static DateTime ReadTime(Dictionary<string, string> head, string key,
        Dictionary<string, IWebElement> dicCell)
    {
        var value = ReadString(head, key, dicCell);
        var d = DateTime.ParseExact(value, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
        return d;
    }

    public static string ReadString(Dictionary<string, string> head, string key,
        Dictionary<string, IWebElement> dicCell)
    {
        if (head.TryGetValue(key, out var className))
        {
            if (dicCell.TryGetValue(className, out var cell))
            {
                var value = (cell.Text ?? string.Empty).Trim();
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
            var key = td.GetAttribute("class") ?? string.Empty;
            row.Add(key, td);
        }

        return row;
    }

    public static decimal ReadBetDecimal(IWebElement e)
    {
        var txt = e.Text;
        var index = txt.IndexOf('：');
        txt = txt[(index + 1)..];
        decimal.TryParse(txt, out var r);
        return r;
    }

    public static decimal ReadDecimal(IWebElement e)
    {
        var txt = ReadString(e);
        var r = decimal.Parse(txt);
        return r;
    }

    public static decimal ReadDecimalOrDefault(IWebElement e, decimal def = 0)
    {
        var txt = ReadString(e);
        if (!decimal.TryParse(txt, out var r))
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
            var d = DateTime.ParseExact(year + "-" + str, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture);
            if (d < now)
            {
                return d;
            }
        }

        return now;
    }

    static readonly List<char> HexSet = new List<char>()
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f'
    };

    // 判断十六进制字符串hex是否正确
    public static bool IsHexadecimal(string hex)
    {
        foreach (var item in hex)
        {
            if (HexSet.Contains<char>(item) == false)
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
                    Log.SaveException(err, driver, "exec_");
                }

                i++;
            }

            Thread.Sleep(sleep);
        }
    }

    public static string EncryptMd5(string s)
    {
        var md5 = MD5.Create();
        return BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(s))).Replace("-", "");
    }

    public static string? GetJsonValue(string key, string content)
    {
        var keyName = "\"" + key + "\"";
        var index = content.IndexOf(keyName, StringComparison.Ordinal);
        if (index > 0)
        {
            var i = index + keyName.Length;
            var start = content.IndexOf("\"", i, content.Length - i, StringComparison.Ordinal);
            i = start + 1;
            var end = content.IndexOf("\"", i, content.Length - i, StringComparison.Ordinal);
            var name = content.Substring((start + 1),(end - start - 1));
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
        var mask = new string('*', len - 1);
        return mask + name[len - 1];
    }
}