using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDownloaderLib.Model
{
    public interface ISeasonItem
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Order { get; set; }

        public bool IsFilms { get; set; }

        public bool CanDownload { get; }

        public string Path { get; set; }
        public ObservableCollection<IElementItem> ElementItems { get; set; }
    }
}

