namespace Sfan;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Sfan.Util;

public class LoginPage: PageBase
{
    private readonly AuthConfig authConfig;
    
    public LoginPage(ChromeDriver driver, AppConfig config, AuthConfig authConfig) : base(driver, config)
    {
        this.authConfig = authConfig;
    }
    
    // 登录
    public bool Login()
    {
        Driver.Navigate().GoToUrl(authConfig.Home);
        // <input type="text" name="username" autofocus="" required="" id="id_username">
        // //input[@id="id_username"]
        
        const string namePath = "//input[@id='id_username' and @name='username']";
        SetTextElementByXPath(namePath, authConfig.UserName);

        // <input type="password" name="password" required="" id="id_password">
        // //input[@id="id_password"]
        const string pwdPath = "//input[@id='id_password' and @name='password']";
        SetTextElementByXPath(pwdPath, authConfig.Password);
        for (var i = 1; i < 1000; i++)
        {
            if (Login(i))
            {
                return true;
            }
        }

        return false;
    }
    
    private bool Login(int i)
    {
        // 登录按钮
        // <input type="submit" id="CheckTwoStep" data-url="/two_factor/verification/" value="登录">
        // //input[@id="CheckTwoStep"]
        const string path = "//input[@id='CheckTwoStep' and @type='submit']";
        FindAndClickByXPath(path, 1000);

        try
        {
            // //div[@id='main']/section/aside/ul/div/li[1]/span
            var e = FindElementByXPath("//div[@id='main']/section/aside/ul/div/li[1]/span");
            var txt = Helper.ReadString(e);
            if (txt == "首页")
            {
                SendMsg("登入成功:" + authConfig.UserName);
                return true;
            }
        }
        catch (WebDriverTimeoutException)
        {
            SendMsg("登入超时:" + authConfig.UserName + "_" + i.ToString());
            Thread.Sleep( (i>60?60:i)*1000);
        }

        return false;
    }
}