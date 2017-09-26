using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace z.Web.Model
{
    /// <summary>
    /// Use for NetPipe Implementation Class
    /// </summary>
    [DataContract, Serializable]
    public class Caller
    {
        public Caller()
        {
            Message = "";
            LogType = LogType.Info;
            LogTime = DateTime.Now;
        }

        public Caller(string Message, LogType type = LogType.Info)
            : this()
        {
            this.Message = Message;
            this.LogType = type;
        }

        public void Log(string Message)
        {
            this.Message = Message;
        }

        /// <summary>
        /// does not accept class type
        /// </summary>
        /// <param name="Data"></param>
        public void SetData(object Data)
        {
            this.Data = Data;
        }

        //public T GetData<T>()
        //{
        //    return (T)Data;
        //}

        public string GetData()
        {
            return Data.ToString();
        }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public LogType LogType { get; set; }

        [DataMember]
        public DateTime LogTime { get; private set; }

        [DataMember]
        private object Data { get; set; }
    }

    public enum LogType : int
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }
}
