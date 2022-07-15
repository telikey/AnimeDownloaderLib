using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimeDownloaderLib.Exceptions
{
    public class DriverNotInited:AnimeDownloaderExceptions
    {
        public DriverNotInited() : base("Driver not inited, init it using Init()") { }
    }
}
