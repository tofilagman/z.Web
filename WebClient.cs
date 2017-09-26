﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using z.Data;
using z.Data.Models;

namespace z.Web
{
    public class WebClient : Client
    {
        public string ServiceName { private get; set; }

        public WebClient(string SoapAddress, string ServiceName)
            : base(SoapAddress)
        {
            this.ServiceName = ServiceName;
        }

        public Result Post(string MethodName, byte[] FileData, object[] ParameterValues = null)
        {
            string requestUrl = string.Format("{0}/{1}", ServiceName, MethodName);

            if (ParameterValues != null)
            {
                requestUrl = string.Format("{0}/{1}", requestUrl, string.Join("/", ParameterValues));
            }

            string retdata = this.PostData(requestUrl, FileData);

            return this.GetResult(retdata, string.Format("{0}Result", MethodName));
        }

        public Result GetRest(string MethodName, Dictionary<string, object> Parameters = null)
        {
            string retdata = this.GetData(GetArguments(MethodName, Parameters));
            return this.GetResult(retdata, string.Format("{0}Result", MethodName));
        }

        public T Get<T>(string MethodName, Dictionary<string, object> Parameters = null) where T : class
        {
            string retdata = this.GetData(GetArguments(MethodName, Parameters));
            return GetResult<T>(retdata);
        }

        public void Exec(string MethodName, Dictionary<string, object> Parameters = null)
        {
            string retdata = this.GetData(GetArguments(MethodName, Parameters));
            GetResult(retdata);
        }

        public void Get(string MethodName, string Destination, Action<Result> Done, Action<Arguments.ProcessDataArgs> OnProgress = null, Dictionary<string, object> Parameters = null)
        {
            this.GetData(GetArguments(MethodName, Parameters), Destination, (a) =>
            {
                using (StreamReader sr = new StreamReader(a))
                {
                    Result rs = this.GetResult(sr.ReadToEnd(), string.Format("{0}Result", MethodName));
                    Done(rs);
                }
            }, OnProgress);
        }

        public Dictionary<string, object> Parameters(params KeyValuePair<string, object>[] pair)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> p in pair)
            {
                dict.Add(p.Key, p.Value);
            }

            return dict;
        }

        public override Task<T> UploadFile<T>(string uri, params string[] FilesToSend)
        {
            return base.UploadFile<T>($"{this.ServiceName}/{uri}", FilesToSend);
        }

        string GetArguments(string MethodName, Dictionary<string, object> Parameters = null)
        {
            string requestUrl = string.Format("{0}/{1}", ServiceName, MethodName);

            if (Parameters != null)
            {
                List<string> combi = new List<string>();

                foreach (KeyValuePair<string, object> pair in Parameters) { combi.Add(string.Format("{0}={1}", pair.Key, pair.Value)); }

                requestUrl = string.Format("{0}?{1}", requestUrl, string.Join("&", combi.ToArray()));
            }

            return requestUrl;
        }

    }
}
