using AnimeDownloaderLib.Model;
using ClassInjector;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeDownloaderLib
{
    public class Jut_su_downloader_Logic: IAnimeDownloaderLogic
    {

        private readonly string startPage = @"https://jut.su/anime/";
        private readonly string geckoDriverFolder = @"C:\temp\downloader";
        private readonly string geckoDriverName = @"geckodriver.exe";
        private readonly string binary = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";
        private IWebDriver driver=null;

        public readonly DownloaderType DownloaderType = DownloaderType.Jut_su;

        public Jut_su_downloader_Logic()
        {
            this.driver=FirefoxDriverInit();
        }

        public List<IAnimeItem> FillAnime(int NumberOfElements)
        {
            if (driver != null)
            {
                GoToPage(startPage);

                IncreaseNumberOfElements(NumberOfElements);

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
                    imageURI = match == null ? "" : match.Value;

                    var obj = Injector.GetObject<IAnimeItem>();
                    obj.Id = count;
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
                return animeLst;
            }
            throw new Exception();
        }

        public List<ISeasonItem> FillSeasons(List<IAnimeItem> observableAnimeCollection)
        {
            if (driver != null)
            {
                List<ISeasonItem> seasonLst = new List<ISeasonItem>();
                var count = 1;
                var oCount = 1;
                foreach (var animeItem in observableAnimeCollection)
                {
                    oCount = 1;
                    GoToPage(animeItem.Path);

                    var seasonElements = driver.FindElements(By.ClassName("the-anime-season"));

                    foreach (var seasonElement in seasonElements)
                    {
                        var isFilms = false;
                        if (seasonElement.Text == "Полнометражные фильмы")
                        {
                            isFilms = true;
                        }
                        var name = seasonElement.Text;

                        var obj = Injector.GetObject<ISeasonItem>();
                        obj.Id = count;
                        obj.Title = name;
                        obj.IsFilms = isFilms;
                        obj.Order = oCount;
                        obj.Path = animeItem.Path;

                        animeItem.SeasonsItems.Add(obj);
                        seasonLst.Add(obj);
                        count++;
                        oCount++;
                    }
                }
                return seasonLst;
            }
            throw new Exception();
        }

        public List<IElementItem> FillElements(List<ISeasonItem> observableSeasonCollection)
        {
            if (driver != null)
            {
                List<IElementItem> seasonLst = new List<IElementItem>();
                var count = 1;
                foreach (var seasonItem in observableSeasonCollection)
                {
                    GoToPage(seasonItem.Path);

                    var paths = driver.FindElements(By.ClassName("video")).Select(x=>x.GetAttribute("href")).ToArray();
                    Regex seasonRegx = new Regex(@"season-(\d\d*)");
                    Regex episodeRegx = new Regex(@"episode-(\d\d*)");

                    if (seasonItem.IsFilms)
                    {
                        episodeRegx = new Regex(@"film-(\d\d*)");
                    }

                    var CanDownload = true;

                    foreach (var path in paths)
                    {
                        if (path != "")
                        {
                            if (!seasonItem.IsFilms)
                            {
                                var match = seasonRegx.Match((string)path).Groups;
                                var seasonName = seasonItem.Title;

                                if (match.Count > 1)
                                {
                                    seasonName = match[1].Value;
                                }
                                if (seasonItem.Order.ToString() == seasonName)
                                {
                                    var name = episodeRegx.Match((string)path).Groups[1].Value;

                                    var obj = Injector.GetObject<IElementItem>();
                                    obj.Id = count;
                                    obj.Title = name;
                                    obj.Path = path;
                                    obj.Order = count;

                                    var dPath = "";
                                    if (CanDownload)
                                    {
                                        GoToPage((string)path);
                                        dPath = FindDownloadPath();
                                        if (dPath == "")
                                        {
                                            CanDownload = false;
                                        }
                                    }

                                    obj.DownloadPath = dPath;
                                    obj.CanDownload = dPath != "";

                                    seasonItem.ElementItems.Add(obj);
                                    seasonLst.Add(obj);
                                    count++;
                                }
                            }
                            else
                            {
                                var name = episodeRegx.Match((string)path).Groups;
                                if (name.Count > 1)
                                {
                                    var obj = Injector.GetObject<IElementItem>();
                                    obj.Id = count;
                                    obj.Title = name[1].Value;
                                    obj.Path = path;
                                    obj.Order = count;

                                    var dPath = "";
                                    if (CanDownload)
                                    {
                                        GoToPage((string)path);
                                        dPath = FindDownloadPath();
                                        if (dPath == "")
                                        {
                                            CanDownload = false;
                                        }
                                    }

                                    obj.DownloadPath = dPath;
                                    obj.CanDownload = dPath != "";


                                    seasonItem.ElementItems.Add(obj);
                                    seasonLst.Add(obj);
                                }
                            }
                        }
                    }
                }
                return seasonLst;
            }
            throw new Exception();
        }

        private void GoToPage(string path)
        {
            var whileFlag = true;
            while (whileFlag)
            {
                try
                {
                    driver.Navigate().GoToUrl(path);

                    whileFlag = false;
                }
                catch
                {
                    Thread.Sleep(500);
                }
            }
        }

        public void DownloadElements(List<IElementItem> observableElementsCollection, string folderPath)
        {
            foreach(var elementItem in observableElementsCollection)
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(elementItem.DownloadPath, Path.Combine(folderPath,elementItem.Title));
                }
            }
        }

        private string FindDownloadPath()
        {
            var dPath = "";
            var element = driver.FindElements(By.TagName("video")).FirstOrDefault();
            IWebElement max = null;

            if (element != null)
            {
                var inner = element.FindElements(By.TagName("source"));
                foreach (var inn in inner)
                {
                    if (max == null)
                    {
                        max = inn;
                    }
                    else
                    {
                        var res = Convert.ToInt32(inn.GetAttribute("res"));
                        var maxRes = Convert.ToInt32(max.GetAttribute("res"));
                        if (res > maxRes)
                        {
                            max = inn;
                        }
                    }
                }
            }
            if (max != null)
            {
                dPath = max.GetAttribute("src");
            }

            return dPath;
        }

        private void IncreaseNumberOfElements(int NumberOfElements)
        {
            int num = (int)Math.Round((double)((NumberOfElements / 30) - 1), MidpointRounding.ToPositiveInfinity);
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
    }
}
