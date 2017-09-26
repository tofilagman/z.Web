using System;
using System.Collections.Generic;
using System.Text;

namespace z.Web.Downloader
{
    public interface IMirrorSelector
    {
        void Init(Downloader downloader);

        ResourceLocation GetNextResourceLocation(); 
    }
}
