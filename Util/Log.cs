namespace Sfan.Util;

using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

public class Log
{
    public static void Debug()
    {

    }

    public static void SaveException(Exception e, ChromeDriver? driver = null)
    {
        var msg = e.ToString();
        string dir = Path.Join(Environment.CurrentDirectory, "log");
        if (!Path.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string t = DateTime.Now.ToString("yyMMddHHmmssfff");
        if (e != null)
        {
            if (e is WebDriverException)
            {
                Console.WriteLine(msg);
                File.WriteAllText(Path.Join(dir, t + ".txt"), msg);
            }
            else
            {
                var st = e.StackTrace ?? string.Empty;
                Console.WriteLine(msg);
                Console.WriteLine(st);
                File.WriteAllLines(Path.Join(dir, t + ".txt"), new string[] { msg, st });
            }
        }

        if (driver != null)
        {
            TakeScreenshot(driver, dir, t);
        }
    }

    public static void TakeScreenshot(ChromeDriver driver, string dir, string t)
    {
        ITakesScreenshot ssdriver = driver as ITakesScreenshot;
        Screenshot screenshot = ssdriver.GetScreenshot();
        screenshot.SaveAsFile(Path.Join(dir, t + ".png"), ScreenshotImageFormat.Png);
    }
}