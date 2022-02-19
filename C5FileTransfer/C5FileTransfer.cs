using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome; //We use Chrome only here
//using OpenQA.Selenium.Firefox;
using CommandLine;
using System.Linq;
using NLog;

namespace C5FileTransfer
{
    public class C5FileTransfer
    {
        private Options opts;
        private readonly ILogger logger;
        private ChromeDriverService driverService;
        private ChromeDriver driver;
        private readonly string binarylocationChrome;
        private readonly string downloadPath;
        private readonly int loginTimeHMI;

        public C5FileTransfer(ILogger logger, Options cmdoptions, string binarylocationChrome, int loginTimeHMI)
        {
            this.logger = logger;
            this.opts = cmdoptions;
            this.loginTimeHMI = loginTimeHMI;
            this.binarylocationChrome = binarylocationChrome;

            downloadPath = opts.Dir;
        }

        public void DownloadFiles()
        {
            LoginHMI();
            DownloadFilesHMI();      
        }

        private ChromeOptions GetBrowserOptions()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless");
            options.AddArgument("disable-gpu");
            options.AddArgument("no-sandbox");
            options.AddArgument("window-size=1920,1080");
            return options;
        }

        private void CreateBrowserDriverService()
        {
            driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
        }
        private void CreateBrowserDriver()
        {
            var options = GetBrowserOptions();
            var param = GetBrowserDriverParameters();
            driver = new ChromeDriver(driverService, options);
            driver.ExecuteChromeCommand("Page.setDownloadBehavior", param);
        }


        private Dictionary<string, object> GetBrowserDriverParameters()
        {
            var param = new Dictionary<string, object>();
            param.Add("behavior", "allow");
            param.Add("downloadPath", downloadPath);
            return param;
        }

        private void LoginHMI()
        {
            CreateBrowserDriverService();
            CreateBrowserDriver();
            
            logger.Info("Connecting with the HMI web interface...");
            driver.Url = string.Format("http://{0}/start.html", opts.IPAddress);
            logger.Info("Webpage title: " + driver.Title);

            IWebElement usernamefield;
            IWebElement passwordfield;
            IWebElement loginbutton;
            IWebElement logoutbutton;
            try
            {
                //Try to find the elements in the webpage
                usernamefield = driver.FindElementByName("Login");
                passwordfield = driver.FindElementByName("Password");
                loginbutton = driver.FindElementByXPath("//input[@type='submit'][@value='Login']");
                passwordfield.SendKeys(opts.Password);
                usernamefield.SendKeys(opts.Username);
                loginbutton.Click();
                Thread.Sleep(loginTimeHMI);
                logoutbutton = driver.FindElementByXPath("//input[@type='submit'][@value='Logout']");
            }
            catch (Exception ex)
            {
                logger.Error("Someting wend wrong in an attempt to login into the HMI");
                logger.Error(ex.ToString());                
                driver.Quit();
                Environment.Exit(-1);
            }
        }

        private List<Tuple<string, string>> GetListOfFilesToDownload()
        {
            //Create a list of files to download
            List<Tuple<string, string>> filesToDownload = new List<Tuple<string, string>>();
            foreach (var file in opts.Files)
            {
                if (file.EndsWith("*.*"))
                {
                    //Other files not supported
                    logger.Warn("Using *.* is not supported");
                    continue;
                }

                //Get all .csv files from C5 HMI first
                if (file.EndsWith("*.csv"))
                {
                    //Get all urls on this page
                    string path = Path.GetDirectoryName(file);
                    driver.Url = string.Format("http://{0}/{1}?UP=TRUE&FORCEBROWSE", opts.IPAddress, path);
                    var allAhrefs = driver.FindElementsByXPath("//a[@href]");
                    var ahrefs = allAhrefs.Where(e => e.GetAttribute("href").EndsWith(".csv?UP=TRUE&FORCEBROWSE")).ToList();

                    //Reverse the list such that the newest files are on top of the list.
                    ahrefs.Reverse();
                    int numberOfFilesToDownload = opts.Number;
                    foreach (var ahref in ahrefs)
                    {
                        var hmifilename = ahref.GetAttribute("href");
                        var filename = hmifilename.Replace("?UP=TRUE&FORCEBROWSE", "");
                        filesToDownload.Add(new Tuple<string, string>(filename, hmifilename));

                        numberOfFilesToDownload--;
                        if (numberOfFilesToDownload <= 0) break;
                    }
                }
                else
                {
                    var hmifilename = string.Format("http://{0}/{1}?UP=TRUE&FORCEBROWSE", opts.IPAddress, file);
                    filesToDownload.Add(new Tuple<string, string>(file, hmifilename));
                }
            }

            return filesToDownload;
        }

        private void DownloadFilesHMI()
        {
            var watcher = new FileSystemWatcher(downloadPath);
            var filesToDownload = GetListOfFilesToDownload();

            //Try to download all the files from the HMI
            logger.Info("Going to download " + filesToDownload.Count().ToString() + " file(s)...");
            foreach (var file in filesToDownload)
            {
                driver.Url = file.Item2;

                //File watcher on the file to be downloaded, with timeout            
                string filename = Path.GetFileName(file.Item1);
                watcher.Filter = filename;
                watcher.NotifyFilter = NotifyFilters.LastWrite;
                var watchresult = watcher.WaitForChanged(WatcherChangeTypes.Created | WatcherChangeTypes.Changed, 10000);
                if (watchresult.TimedOut == true)
                {
                    logger.Warn("Timeout occured when downloading the file " + filename);

                    //When the file does not exists, the Title is an empty string.
                    if (String.IsNullOrEmpty(driver.Title)) logger.Warn((string.Format("The file {0} does not seem to exist on the HMI...", file)));
                    continue;
                }
                else if (watchresult.Name == filename)
                {
                    logger.Info("Download of file " + filename + " is completed.");
                }

                logger.Info("File " + filename + " ChangeType: " + watchresult.ChangeType.ToString());
            }

            driver.Quit();
        }
    }
}
