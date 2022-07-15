using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDownloaderLib.Exceptions
{
    public class AnimeDownloaderExceptions : Exception
    {
        public AnimeDownloaderExceptions(string message) : base("AnimeDownloaderLib: " + message) { }
    }
}
