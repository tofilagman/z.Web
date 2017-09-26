using System;
using System.Collections.Generic;
using System.Text;

namespace z.Web.Downloader
{
    public enum DownloaderState: byte 
    {
        NeedToPrepare = 0,
        Preparing,
        WaitingForReconnect,
        Prepared,
        Working,
        Pausing,
        Paused,
        Ended,
        EndedWithError
    }
}
