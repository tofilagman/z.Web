using System;
using System.Collections.Generic;
using System.Text;

namespace z.Web.Downloader
{
    public enum SegmentState
    {
        Idle,
        Connecting,
        Downloading,
        Paused,
        Finished,
        Error,
    }
}
