using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using z.Web.Downloader.Extensions;

namespace z.Web.Downloader.Protocols
{
    public class HttpFtpProtocolExtension: IExtension
    {
        internal static IHttpFtpProtocolParameters parameters;

        #region IExtension Members

        public string Name
        {
            get { return "HTTP/FTP"; }
        }
        
        public HttpFtpProtocolExtension():
            this(new HttpFtpProtocolParametersSettingsProxy())
        {
        }

        public HttpFtpProtocolExtension(IHttpFtpProtocolParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            if (HttpFtpProtocolExtension.parameters != null)
            {
                throw new InvalidOperationException("The type HttpFtpProtocolExtension is already initialized.");
            }

            HttpFtpProtocolExtension.parameters = parameters;

            ProtocolProviderFactory.RegisterProtocolHandler("http", typeof(HttpProtocolProvider));
            ProtocolProviderFactory.RegisterProtocolHandler("https", typeof(HttpProtocolProvider));
            ProtocolProviderFactory.RegisterProtocolHandler("ftp", typeof(FtpProtocolProvider));
        }

        #endregion
    }
}
