using AnimeDownloaderLib.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDownloaderLib
{
    public interface IDownloaderLogic<TItem>
    {
        bool Fill(List<IAnimeItem> observableCollection, int NumberOfElements);

    }
}
