using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Cookies
{
    internal class Program
    {
        static void Main()
        {
            var options = new ChromeOptions();

            // Отключаем детектор WebDriver
            options.AddArgument("--disable-blink-features=AutomationControlled");

            // Устанавливаем User-Agent реального браузера
            options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/110.0.5481.77 Safari/537.36");

            using (IWebDriver driver = new ChromeDriver(options))
            {
                try
                {
                    string url = "https://auth.lib.social/auth/login"; 
                    driver.Navigate().GoToUrl(url);

                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

                    // Отключаем navigator.webdriver
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");

                    Thread.Sleep(new Random().Next(2000, 4000)); 

                    // === Авторизация ===
                    IWebElement loginField = wait.Until(d => d.FindElement(By.Name("login")));
                    IWebElement passwordField = wait.Until(d => d.FindElement(By.Name("password")));

                    Thread.Sleep(new Random().Next(1000, 4000));

                    loginField.SendKeys("email"); 
                    Thread.Sleep(1000);
                    passwordField.SendKeys("password"); 
                    Thread.Sleep(new Random().Next(1000, 4000));

                    // Нажимаем кнопку "Войти"
                    IWebElement submitButton_ = wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'btn_variant-primary')]")));
                    submitButton_.Click();
                    Thread.Sleep(new Random().Next(1000, 4000)); 

                    // Получаем cookies
                    IReadOnlyCollection<Cookie> cookies = driver.Manage().Cookies.AllCookies;
                    Console.WriteLine("Cookies:");
                    foreach (var cookie in cookies)
                    {
                        Console.WriteLine($"{cookie.Name}: {cookie.Value}");
                    }

                    // Удаляем все cookies
                    DeleteAllCookies(driver);
                    Console.WriteLine("Все cookies удалены.");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка: " + e.Message);
                }

                Thread.Sleep(4000);
            }

            static void DeleteAllCookies(IWebDriver driver)
            {
                driver.Manage().Cookies.DeleteAllCookies();
            }
        }
    }
}
