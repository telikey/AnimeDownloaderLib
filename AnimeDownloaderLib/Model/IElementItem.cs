using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDownloaderLib.Model
{
    public interface IElementItem
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Path { get; set; }
    }
}

