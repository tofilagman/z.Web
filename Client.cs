﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using z.Data.Models;
using z.Data;
using System.Text;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Http.Headers;

namespace z.Web
{
    /// <summary>
    /// Web Service Client Wrapper
    /// Version 1.0, LJ Gomez 20140322
    /// </summary>
    public class Client : IDisposable
    {
        public string Server { private get; set; }
        private HttpWebRequest request;
        private readonly Encoding encoding = Encoding.UTF8;

        /// <summary>
        /// Root Address
        /// </summary>
        /// <param name="mServer"></param>
        public Client(string mServer)
        {
            this.Server = mServer;
        }

        /// <summary>
        /// Throws error when base address not found
        /// </summary>
        public void Connect()
        {
            request = (HttpWebRequest)HttpWebRequest.Create(this.ValidateUrl(this.Server));
            request.Headers.Clear();
            request.AllowAutoRedirect = false;
            request.Method = "HEAD";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK) throw new Exception(response.StatusDescription);
                }
            }
            catch (WebException wex)
            {
                if (this.Server.ToLower().Contains(".asmx"))
                {
                    if (!wex.Message.ToLower().Contains("500")) throw wex; //found but for security its forbidden
                }
                else
                {
                    if (!wex.Message.ToLower().Contains("403")) throw wex; //found but for security its forbidden
                }
            }

        }

        public string PostData(string uri, byte[] fileToSend = null)
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false;

                request = (HttpWebRequest)HttpWebRequest.Create(this.Combine(this.Server, uri));
                request.Headers.Clear();
                request.Method = WebRequestMethods.Http.Post; //"POST";
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/json";
                request.Headers.Add("Accept-Language", "en-us\r\n");
                request.Headers.Add("UA-CPU", "x86 \r\n");
                request.Headers.Add("Cache-Control", "no-cache\r\n");
                request.KeepAlive = true;
                request.Credentials = CredentialCache.DefaultCredentials;

                if (fileToSend != null)
                {
                    request.ContentLength = fileToSend.Length;
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        // Send the file as body request.
                        requestStream.Write(fileToSend, 0, fileToSend.Length);
                        requestStream.Close();
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    if (response.StatusCode != HttpStatusCode.OK) throw new Exception(response.StatusDescription);

                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    string responsefromserver = reader.ReadToEnd();

                    reader.Close();
                    stream.Close();

                    return responsefromserver;
                }
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> PostDataAsync(string uri, byte[] fileToSend = null)
        {
            return await Task.Run<string>(() =>
            {
                return PostData(uri, fileToSend);
            });
        }

        public string PostDataASMX(string uri, byte[] fileToSend)
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false;

                request = (HttpWebRequest)HttpWebRequest.Create(Combine(this.Server, uri));
                request.Headers.Clear();
                request.Method = WebRequestMethods.Http.Post; //"POST";
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/x-www-form-urlencoded"; //"application/json";

                request.Headers.Add("Accept-Language", "en-us\r\n");
                request.Headers.Add("UA-CPU", "x86 \r\n");
                request.Headers.Add("Cache-Control", "no-cache\r\n");
                request.KeepAlive = true;
                request.Credentials = CredentialCache.DefaultCredentials;

                request.ContentLength = fileToSend.Length;

                using (Stream requestStream = request.GetRequestStream())
                {
                    // Send the file as body request.
                    requestStream.Write(fileToSend, 0, fileToSend.Length);
                    requestStream.Close();
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    if (response.StatusCode != HttpStatusCode.OK) throw new Exception(response.StatusDescription);

                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    string responsefromserver = reader.ReadToEnd();

                    reader.Close();
                    stream.Close();

                    return responsefromserver;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string PostDataASMX(string uri, Dictionary<string, object> paramaters)
        {
            List<string> hlst = new List<string>();
            foreach (KeyValuePair<string, object> h in paramaters)
            {
                hlst.Add(string.Format("{0}={1}", h.Key, h.Value));
            }
            return PostDataASMX(uri, DataToBytes(string.Join("&", hlst.ToArray())));
        }

        public virtual void PostFile(string uri, string filename)
        {
            request = (HttpWebRequest)WebRequest.Create(this.Combine(this.Server, uri));
            request.KeepAlive = true;
            request.Credentials = CredentialCache.DefaultCredentials;
            using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
            {
                Stream responseStream = webResponse.GetResponseStream();

                using (FileStream fileStream = new FileStream(filename, FileMode.Create))
                {
                    for (int a = responseStream.ReadByte(); a != -1; a = responseStream.ReadByte())
                        fileStream.WriteByte((byte)a);
                }

                responseStream.Dispose();
            }
        }

        /// <summary>
        /// LJ 20151211
        /// request for WEB API Controllers
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="FilesToSend"></param>
        public async virtual Task<T> UploadFile<T>(string uri, params string[] FilesToSend) where T : class
        {
            try
            {
                using (var client = new HttpClient())
                using (var content = new MultipartFormDataContent())
                {
                    // Make sure to change API address
                    client.BaseAddress = new Uri(this.Server);

                    foreach (string f in FilesToSend)
                    {
                        var fileContent1 = new ByteArrayContent(File.ReadAllBytes(f));
                        fileContent1.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                        {
                            FileName = Path.GetFileName(f)
                        };
                        content.Add(fileContent1);
                    }

                    // Make a call to Web API
                    var result = client.PostAsync(uri, content).Result;

                    if (result.StatusCode != HttpStatusCode.OK)
                        throw new Exception(result.StatusCode.ToString());

                    using (var gh = new MemoryStream())
                    {
                        await result.Content.CopyToAsync(gh);
                        gh.Seek(0, SeekOrigin.Begin);
                        using (StreamReader sr = new StreamReader(gh))
                        {
                            string responsefromserver = sr.ReadToEnd();
                            return GetResult<T>(responsefromserver);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetData(string uri)
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false;
                request = (HttpWebRequest)WebRequest.Create(this.Combine(this.Server, uri));
                request.Headers.Clear();
                request.Method = WebRequestMethods.Http.Get;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/json";
                request.Headers.Add("Accept-Language", "en-us\r\n");
                request.Headers.Add("UA-CPU", "x86 \r\n");
                request.Headers.Add("Cache-Control", "no-cache\r\n");
                request.KeepAlive = true;
                request.Credentials = CredentialCache.DefaultCredentials;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    if (response.StatusCode != HttpStatusCode.OK) throw new Exception(response.StatusDescription);

                    Stream stream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(stream);
                    string responsefromserver = reader.ReadToEnd();

                    reader.Close();
                    stream.Close();

                    return responsefromserver;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="destination"></param>
        /// <param name="OnProcessData">length, nread, percent, currentSpeed</param>
        public void GetData(string uri, string destination, Action<MemoryStream> Done, Action<Arguments.ProcessDataArgs> OnProcessData = null)
        {
            try
            {
                System.Net.ServicePointManager.Expect100Continue = false;
                request = (HttpWebRequest)WebRequest.Create(this.Combine(this.Server, uri));
                request.Headers.Clear();
                request.Method = WebRequestMethods.Http.Get;
                request.ProtocolVersion = HttpVersion.Version10;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add("Accept-Language", "en-us\r\n");
                request.Headers.Add("UA-CPU", "x86 \r\n");
                request.Headers.Add("Cache-Control", "no-cache\r\n");
                request.KeepAlive = true;
                request.Credentials = CredentialCache.DefaultCredentials;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    if (response.StatusCode != HttpStatusCode.OK) throw new Exception(response.StatusDescription);

                    long length = response.ContentLength;
                    var speedtimer = new Stopwatch();
                    double currentSpeed = -1;
                    int readings = 0;
                    int nread = 0;
                    bool IsProcessing = true;

                    //FileStream fs = new FileStream(destination, FileMode.Create);
                    MemoryStream ms = new MemoryStream();

                    while (IsProcessing)
                    {

                        speedtimer.Start();

                        byte[] readbytes = new byte[4096];
                        int bytesread = response.GetResponseStream().Read(readbytes, 0, 4096);
                        nread += bytesread;
                        short percent = Convert.ToInt16((nread * 100) / length);

                        if (OnProcessData != null) OnProcessData(new Arguments.ProcessDataArgs() { length = length, position = nread, percent = Math.Abs(percent), speed = currentSpeed });

                        if (bytesread == 0) IsProcessing = false;

                        ms.Write(readbytes, 0, bytesread);
                        //fs.Write(readbytes, 0, bytesread);

                        speedtimer.Stop();

                        readings++;
                        if (readings >= 5)
                        {
                            double lk = (speedtimer.ElapsedMilliseconds / 1000);
                            currentSpeed = (lk == 0) ? 20480 : 20480 / lk;
                            speedtimer.Reset();
                            readings = 0;
                        }
                    }

                    response.GetResponseStream().Close();
                    response.Close();

                    ms.Seek(0, SeekOrigin.Begin);
                    Done(ms);

                    ms.Close();
                    ms.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DownloadFile(string uri, string file, Parameter Param = null)
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
                client.DownloadFile($"{this.Combine(this.Server, uri)}?{Param.ToString()}", file);
        }

        public async Task DownloadFileAsync(string uri, string file, Parameter Param = null, Action<object, DownloadProgressChangedEventArgs> Progress = null)
        {
            var manual = new System.Threading.ManualResetEvent(false);
            await Task.Run(() =>
            {
                Int32 CurrProg = -1;
                System.Net.WebClient client = new System.Net.WebClient();
                client.DownloadProgressChanged += (a, b) =>
                {
                    if (CurrProg != b.ProgressPercentage)
                        Progress?.Invoke(a, b);
                    CurrProg = b.ProgressPercentage;
                };
                client.DownloadFileCompleted += (a, b) => manual.Set();
                client.DownloadFileAsync(new Uri($"{this.Combine(this.Server, uri)}?{Param.ToString()}"), file);
                manual.WaitOne();
            });
        }

        public async Task DownloadFileAsync(string uri, string filename, Parameter param = null)
        {
            await Task.Run(() => DownloadFile(uri, filename, param));
        }

        public async Task SaveToFile(Stream response, string Filename)
        {
            await Task.Run(() =>
            {
                using (FileStream kp = new FileStream(Filename, FileMode.Create, FileAccess.Write))
                {
                    Write(response, kp);
                }
            });
        }

        public Stream GetDataStream(string uri)
        {
            try
            {
                WebRequest request = WebRequest.Create(this.Combine(this.Server, uri));
                // Get response
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    return response.GetResponseStream();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GC.Collect();
            }
        }

        public void WriteFile(string filepath, byte[] buffer)
        {

            using (FileStream fileStream = new FileStream(filepath, FileMode.Create))
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    fileStream.WriteByte(buffer[i]);
                }
                fileStream.Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < fileStream.Length; i++)
                {
                    if (buffer[i] != fileStream.ReadByte())
                    {
                        throw new Exception("Error Writing File");
                    }
                }
                //success
            }
        }

        /// <summary>
        /// ResultName Must be the root JSON Container.
        /// </summary>
        /// <param name="ResponseData"></param>
        /// <param name="ResultName"></param>
        /// <returns></returns>
        public virtual Result GetResult(string ResponseData, string ResultName)
        {
            try
            {
                dynamic fleobj = JsonConvert.DeserializeObject(ResponseData);

                Result rs = new Result();
                rs.Status = Convert.ToInt32(fleobj[ResultName].Status);
                rs.ErrorMsg = Convert.ToString(fleobj[ResultName].ErrorMsg);

                if (rs.Status == 1) throw new Exception(rs.ErrorMsg);

                rs.ResultSet = fleobj[ResultName].ResultSet;

                return rs;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual T GetResult<T>(string ResponseData) where T : class
        {
            try
            {
                Result obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(ResponseData);
                if (obj.Status == 1) throw new Exception(obj.ErrorMsg);

                if (typeof(T) == typeof(String))
                {
                    return obj.ResultSet as T;
                }
                else
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj.ResultSet.ToString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual void GetResult(string ResponseData)
        {
            try
            {
                Result obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Result>(ResponseData);
                if (obj.Status == 1) throw new Exception(obj.ErrorMsg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public dynamic GetXMLResult(string ResponseData)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ResponseData);
            string jsontext = JsonConvert.SerializeXmlNode(doc);
            return JsonConvert.DeserializeObject(jsontext);
        }

        public string GetXMLSerializeResult(string ResponseData)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ResponseData);
            return JsonConvert.SerializeXmlNode(doc);
        }

        public string Combine(params string[] uri)
        {
            List<string> ss = new List<string>();
            foreach (string s in uri)
            {
                if (s.Contains("/"))
                {
                    string fs = s;
                    if (fs.Substring(0, 1) == "/")
                    {
                        fs = fs.Remove(0, 1);
                    }

                    if (fs.Substring(fs.Length - 1, 1) == "/")
                    {
                        fs = fs.Remove(fs.Length - 1);
                    }
                    ss.Add(fs);
                }
                else
                {
                    ss.Add(s);
                }
            }

            return string.Join("/", ss.ToArray());
        }

        public byte[] ConvertFile(string FileName)
        {
            if (!File.Exists(FileName)) throw new Exception("File could not located.");
            return File.ReadAllBytes(FileName);
        }

        /// <summary>
        /// Get Data Result Set
        /// </summary>
        /// <typeparam name="T">
        ///     T = Represents any object wich your resultset need to produce
        /// </typeparam>
        /// <param name="rs"></param>
        /// <returns></returns>
        public T GetDataSet<T>(Result rs)
        {
            return JsonConvert.DeserializeObject<T>(rs.ResultSet.ToString());
        }

        public void Write(Stream from, Stream to)
        {
            for (int a = from.ReadByte(); a != -1; a = from.ReadByte())
                to.WriteByte((byte)a);

            from.Dispose();
        }

        public byte[] GetJSONBytes(string Json)
        {
            return System.Text.ASCIIEncoding.Default.GetBytes(Json);
        }

        public byte[] DataToBytes(string Data)
        {
            return GetJSONBytes(Data);
        }

        public byte[] DataToBytes(object Data)
        {
            return GetJSONBytes(Newtonsoft.Json.JsonConvert.SerializeObject(Data));
        }

        public byte[] GetJSONBytes(Dictionary<string, object> Json)
        {
            string j = Newtonsoft.Json.JsonConvert.SerializeObject(Json);
            return System.Text.ASCIIEncoding.Default.GetBytes(j);
        }

        public string ValidateUrl(string url)
        {
            if (url == "") return url;
            if (url.Substring(url.Length - 1, 1) == "/")
            {
                return url;
            }
            else
            {
                return url + "/";
            }
        }

        public void Dispose()
        {
            this.request = null;
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}