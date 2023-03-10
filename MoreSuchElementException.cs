namespace Sfan;

using OpenQA.Selenium;
using System.Collections.ObjectModel;

public class MoreSuchElementException : NoSuchElementException
{
    public By By { get; set; }
    public ReadOnlyCollection<IWebElement>? Elemens { get; set; }

    public MoreSuchElementException(By by, string msg, ReadOnlyCollection<IWebElement>? es) : base(msg)
    {
        Elemens = es;
        By = by;
    }
}