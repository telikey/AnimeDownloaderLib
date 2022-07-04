using AnimeDownloaderLib.Model;
using ClassInjector;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeDownloaderLib
{
    public class Jut_su_Logic: IDownloaderLogic<IAnimeItem>
    {

        private readonly string startPage = @"https://jut.su/anime/";
        private readonly string googlePage = @"https://google.com/";
        private readonly string geckoDriverFolder = @"C:\temp\downloader";
        private readonly string geckoDriverName = @"geckodriver.exe";
        private readonly string binary = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";
        private IWebDriver driver=null;

        public readonly DownloaderType DownloaderType = DownloaderType.Jut_su;

        public Jut_su_Logic()
        {
            this.driver=FirefoxDriverInit();
        }

        public bool Fill(ObservableCollection<IAnimeItem> observableCollection, int NumberOfElements=30)
        {
            if (driver != null)
            {
                driver.Navigate().GoToUrl(startPage);

                IncreaseNumberOfElements(NumberOfElements);

                var animeItems = FillAnime();
                foreach(var anime in animeItems)
                {
                    anime. = new ObservableCollection<ISeasonItem>(FillSeasons(anime));
                }


                driver.Navigate().GoToUrl(startPage);
                return true;
            }

            return false; ;
        }

        private void IncreaseNumberOfElements(int NumberOfElements)
        {
            int num = (NumberOfElements / 30)-1;
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            for (int i = 0; i < num; i++)
            {
                js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                Thread.Sleep(500);
            }
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
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IAnimeItem[] FillAnime()
        {
            List<IAnimeItem> animeLst = new List<IAnimeItem>();

            var animeElements = driver.FindElements(By.CssSelector("div[id^='anime_fs_']"));

            Regex URIregx = new Regex("https://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);

            foreach (var animeElement in animeElements)
            {
                var name = animeElement.FindElement(By.ClassName("aaname")).Text;
                var path = animeElement.FindElement(By.TagName("a")).GetAttribute("href");

                var imageURI = animeElement.FindElement(By.ClassName("all_anime_image")).GetAttribute("style");
                var match = URIregx.Matches(imageURI).FirstOrDefault();
                imageURI = match == null?"":match.Value;

                var obj=Injector.GetObject<IAnimeItem>();
                obj.Title = name;
                obj.Path = path;
                obj.ImageURI = imageURI;

                animeLst.Add(obj);
            }

            return animeLst.ToArray();
        }

        private ISeasonItem[] FillSeasons(IAnimeItem anime)
        {
            List<ISeasonItem> animeLst = new List<ISeasonItem>();

            var animeElements = driver.FindElements(By.CssSelector("div[id^='anime_fs_']"));

            foreach (var animeElement in animeElements)
            {
                var name = animeElement.FindElement(By.ClassName("aaname")).Text;
                var path = animeElement.FindElement(By.TagName("a")).GetAttribute("href");

                var obj = Injector.GetObject<ISeasonItem>();
                obj.Title = name;
                obj.Path = path;

                animeLst.Add(obj);
            }

            return animeLst.ToArray();
        }

    }
}
