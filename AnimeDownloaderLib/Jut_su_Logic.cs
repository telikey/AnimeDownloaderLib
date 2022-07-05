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

        private int _animeCount=1;
        private int _seasonCount = 1;
        private int _elementCount = 1;
        public bool Fill(List<IAnimeItem> observableCollection, int NumberOfElements)
        {
            if (driver != null)
            {
                driver.Navigate().GoToUrl(startPage);

                IncreaseNumberOfElements(NumberOfElements);

                var animeItems = FillAnime(NumberOfElements);
                foreach(var animeItem in animeItems)
                {
                    driver.Navigate().GoToUrl(animeItem.Path);
                    animeItem.SeasonsItems = new ObservableCollection<ISeasonItem>(FillSeasons(animeItem));
                    var seasons = animeItem.SeasonsItems;
                    foreach(var seasonItem in seasons)
                    {
                        seasonItem.ElementItems = new ObservableCollection<IElementItem>(FillElements(seasonItem));
                    }

                    observableCollection.Add(animeItem);
                }

                driver.Navigate().GoToUrl(googlePage);
                return true;
            }

            return false; ;
        }

        private void IncreaseNumberOfElements(int NumberOfElements)
        {
            int num = (int)Math.Round((double)((NumberOfElements / 30)-1),MidpointRounding.ToPositiveInfinity);
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

        private IAnimeItem[] FillAnime(int NumberOfElements)
        {
            List<IAnimeItem> animeLst = new List<IAnimeItem>();

            var animeElements = driver.FindElements(By.CssSelector("div[id^='anime_fs_']"));

            Regex URIregx = new Regex("https://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", RegexOptions.IgnoreCase);

            var count = 1;
            foreach (var animeElement in animeElements)
            {
                var name = animeElement.FindElement(By.ClassName("aaname")).Text;
                var path = animeElement.FindElement(By.TagName("a")).GetAttribute("href");

                var imageURI = animeElement.FindElement(By.ClassName("all_anime_image")).GetAttribute("style");
                var match = URIregx.Matches(imageURI).FirstOrDefault();
                imageURI = match == null?"":match.Value;

                var obj=Injector.GetObject<IAnimeItem>();
                obj.Id = _animeCount;
                _animeCount++;
                obj.Title = name;
                obj.Path = path;
                obj.ImageURI = imageURI;

                animeLst.Add(obj);
                if (count >= NumberOfElements)
                {
                    break;
                }
                count++;
            }

            return animeLst.ToArray();
        }

        private ISeasonItem[] FillSeasons(IAnimeItem anime)
        {
            List<ISeasonItem> seasonLst = new List<ISeasonItem>();

            var seasonElements = driver.FindElements(By.ClassName("the-anime-season"));

            var count = 1;

            foreach (var seasonElement in seasonElements)
            {
                var isFilms = false;
                if(seasonElement.Text== "Полнометражные фильмы")
                {
                    isFilms = true;
                }
                var name = seasonElement.Text;

                var obj = Injector.GetObject<ISeasonItem>();
                obj.Id = _seasonCount;
                _seasonCount++;
                obj.Title = name;
                obj.IsFilms= isFilms;
                obj.Order= count;
                obj.Path = anime.Path;

                seasonLst.Add(obj);
                count++;
            }
            return seasonLst.ToArray();
        }
        private IElementItem[] FillElements(ISeasonItem season)
        {
            List<IElementItem> seasonLst = new List<IElementItem>();

            var elements = driver.FindElements(By.ClassName("video"));
            Regex seasonRegx = new Regex(@"season-(\d\d*)");
            Regex episodeRegx = new Regex(@"episode-(\d\d*)");
            if (season.IsFilms)
            {
                episodeRegx = new Regex(@"film-(\d\d*)");
            }
            var count = 1;
            foreach (var element in elements)
            {
                var path = element.GetAttribute("href");
                if (path != "")
                {
                    if (!season.IsFilms)
                    {
                        var match = seasonRegx.Match(path).Groups;
                        var seasonName = season.Title;
                        if (match.Count > 1)
                        {
                            seasonName = match[1].Value;
                        }
                        if (season.Order.ToString() == seasonName)
                        {
                            var name = episodeRegx.Match(path).Groups[1].Value;

                            var obj = Injector.GetObject<IElementItem>();
                            obj.Id = _elementCount;
                            _elementCount++;
                            obj.Title = name;
                            obj.Path = path;
                            obj.Order = count;

                            seasonLst.Add(obj);
                            count++;
                        }
                    }
                    else
                    {
                        var name = episodeRegx.Match(path).Groups;
                        if (name.Count > 1)
                        {
                            var obj = Injector.GetObject<IElementItem>();
                            obj.Title = name[1].Value;
                            obj.Path = path;

                            seasonLst.Add(obj);
                        }
                    }
                }
            }
            return seasonLst.ToArray();
        }
    }
}
