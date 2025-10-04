using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Atlassian.Jira.Test.Integration.Setup;

public class SetupProgram
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Attlasian SDK integration tests setup");

        var hostOption = new Option<string>("--host", "-h")
        {
            Description = "Jira server host",
            Required = true,
        };
        rootCommand.Options.Add(hostOption);

        var portOption = new Option<int>("--port", "-p")
        {
            Description = "Jira server port",
            Required = true,
        };
        rootCommand.Options.Add(portOption);

        var testDataFileSuffixOption = new Option<string>("--testDataFileSuffix", "-tdfs")
        {
            Description = "Test data file suffix"
        };
        rootCommand.Options.Add(testDataFileSuffixOption);

        rootCommand.SetAction(async parseResult =>
        {
            var host = parseResult.GetValue(hostOption);
            var port = parseResult.GetValue(portOption);
            var testDataFileSuffix = parseResult.GetValue(testDataFileSuffixOption);

            await RunAsync(host, port, testDataFileSuffix);
            return 0;
        });

        var parseResult = rootCommand.Parse(args);
        if (parseResult.Errors.Any())
        {
            rootCommand.Parse("-h").Invoke();
            return 1;
        }
        else
        {
            return await parseResult.InvokeAsync();
        }
    }

    private static async Task RunAsync(string host, int port, string testDataFileSuffix)
    {
        var url = $"http://{host}:{port}";

        await WaitForJira(url);

        var chromeService = ChromeDriverService.CreateDefaultService();
        var options = new ChromeOptions();
        options.LeaveBrowserRunning = true;
        options.AddArgument("no-sandbox");
        using (var webDriver = new ChromeDriver(chromeService, options, TimeSpan.FromMinutes(5)))
        {
            webDriver.Url = url;

            try
            {
                SetupJira(webDriver, testDataFileSuffix);
                webDriver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("-- Setup Failed. Browser will be kept running until you press a key. -- ");
                Console.ResetColor();

                Console.ReadKey();
                webDriver.Quit();
            }
        }
        ;
    }

    private static async Task WaitForJira(string url)
    {
        using (var client = new HttpClient())
        {
            HttpResponseMessage response = null;
            var retryCount = 0;

            do
            {
                try
                {
                    Console.Write($"Pinging server {url}.");

                    retryCount++;
                    await Task.Delay(2000);
                    response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine($" Failed, retry count: {retryCount}");
                }
            } while (retryCount < 60 && (response == null || response.StatusCode != HttpStatusCode.OK));

            Console.WriteLine($" Success!");
        }
    }

    private static int GetStep(ChromeDriver webDriver)
    {
        if (webDriver.UrlContains("SetupMode"))
        {
            return 1;
        }
        else if (webDriver.UrlContains("SetupDatabase"))
        {
            return 2;
        }
        else if (webDriver.UrlContains("SetupApplicationProperties"))
        {
            return 3;
        }
        else
        {
            return 4;
        }
    }

    private static void SetupJira(ChromeDriver webDriver, string testDataFileSuffix)
    {
        Console.WriteLine("--- Starting to setup Jira ---");
        webDriver.WaitForElement(By.Id("logo"), TimeSpan.FromMinutes(5));
        var step = GetStep(webDriver);

        if (step <= 1)
        {
            Console.WriteLine("Choose to manually setup jira.");
            webDriver.WaitForElement(By.XPath("//div[@data-choice-value='classic']"), TimeSpan.FromMinutes(5)).Click();

            Console.WriteLine("Click the next button.");
            webDriver.WaitForElement(By.Id("jira-setup-mode-submit")).Click();
        }

        if (step <= 2)
        {
            Console.WriteLine("Wait for database page, and click on the next button.");
            webDriver.WaitForElement(By.Id("jira-setup-database-submit")).Click();

            Console.WriteLine("Wait for the built-in database to be setup.");
            webDriver.WaitForElement(By.Id("jira-setupwizard-submit"), TimeSpan.FromMinutes(10));
        }

        if (step <= 3)
        {
            Console.WriteLine("Click on the import link.");
            webDriver.WaitForElement(By.TagName("a")).Click();
        }

        if (step <= 4)
        {
            var testDataFile = "TestData.zip";
            if (!string.IsNullOrWhiteSpace(testDataFileSuffix))
            {
                testDataFile = $"TestData_{testDataFileSuffix}.zip";
            }

            Console.WriteLine($"Wait for the import data page and import the test data. Using data file: {testDataFile}");

            webDriver.WaitForElement(By.Name("filename")).SendKeys(testDataFile);
            webDriver.WaitForElement(By.Id("jira-setupwizard-submit")).Click();

            Console.WriteLine("Wait until restore is complete (may take up to 20 minutes).");
            webDriver.WaitForElement(By.Id("login-form-username"), TimeSpan.FromMinutes(20));
        }

        Console.WriteLine("--- Finished setting up Jira ---");
    }
}
