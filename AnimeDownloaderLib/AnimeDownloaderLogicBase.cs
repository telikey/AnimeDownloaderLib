using AnimeDownloaderLib.Exceptions;
using AnimeDownloaderLib.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDownloaderLib
{
    public abstract class AnimeDownloaderLogicBase : IAnimeDownloaderLogic
    {
        private readonly string geckoDriverFolder = @"C:\temp\downloader";
        private readonly string geckoDriverName = @"geckodriver.exe";
        private readonly string binary = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";

        protected static IWebDriver driver = null;
        public void Init()
        {
            driver = FirefoxDriverInit();
        }

        private IWebDriver FirefoxDriverInit()
        {
            var service = FirefoxDriverService.CreateDefaultService(geckoDriverFolder, geckoDriverName);
            service.FirefoxBinaryPath = binary;
            service.HideCommandPromptWindow = true;

            try
            {
                return new FirefoxDriver(service);
            }
            catch
            {
                throw;
            }
        }

        public abstract List<IAnimeItem> FillAnime(int skipNumberofElements, int NumberOfElements);

        public abstract List<ISeasonItem> FillSeasons(List<IAnimeItem> observableAnimeCollection);

        public abstract List<IElementItem> FillElements(List<ISeasonItem> observableSeasonCollection);

        public abstract void DownloadElements(List<IElementItem> observableSeasonCollection, string folderPath);
    }
}
