using AnimeDownloaderLib.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDownloaderLib
{
    public interface IAnimeDownloaderLogic
    {
        void Init();
        List<IAnimeItem> FillAnime(int skipNumberofElements,int NumberOfElements);
        List<ISeasonItem> FillSeasons(List<IAnimeItem> observableAnimeCollection);
        List<IElementItem> FillElements(List<ISeasonItem> observableSeasonCollection);
        void DownloadElements(List<IElementItem> observableSeasonCollection, string folderPath);
    }
}
