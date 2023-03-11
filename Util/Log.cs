namespace Sfan.Util;

using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class Log
{
    public static void Debug()
    {

    }

    public static void SaveException(Exception e, ChromeDriver? driver, string head)
    {
        var dir = Path.Join(Environment.CurrentDirectory, "log");
        if (!Path.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var fileName = head + DateTime.Now.ToString("yyMMddHHmmssfff");
        var fullName = Path.Join(dir, fileName + ".txt");
        SaveException(e, head, fullName);

        if (driver != null)
        {
            fullName = Path.Join(dir, fileName + ".png");
            SaveScreenshot(driver, fullName);
        }
    }

    private static void SaveException(Exception e, string head, string fullName)
    {
        var msg = e.ToString();
        if (e is WebDriverException)
        {
            Console.WriteLine(msg);
            File.WriteAllText(fullName, msg);
        }
        else
        {
            var st = e.StackTrace ?? string.Empty;
            Console.WriteLine(head + msg);
            Console.WriteLine(st);
            if (e.InnerException != null)
            {
                File.WriteAllLines(fullName,
                    new string[]
                        { msg, st, e.InnerException.Message, e.InnerException.StackTrace ?? string.Empty });
            }
            else
            {
                File.WriteAllLines(fullName, new string[] { msg, st });
            }
        }
    }

    private static void SaveScreenshot(ChromeDriver driver, string fullName)
    {
        var drv = driver as ITakesScreenshot;
        var screenshot = drv.GetScreenshot();
        screenshot.SaveAsFile(fullName, ScreenshotImageFormat.Png);
    }
}