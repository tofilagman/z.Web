using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace z.Web.Downloader.Extensions
{
    public interface IExtensionParameters
    {
        event PropertyChangedEventHandler ParameterChanged;
    }
}
