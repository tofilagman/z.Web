using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Text.RegularExpressions;
using z.Web.Model;

namespace z.Web
{
    /// <summary>
    /// LJ 20150722
    /// WCF Service NetPipe Helper
    /// Using SystemModel
    /// 
    /// Test Interface
    /// 
    ///  [ServiceContract(Namespace = "http://www.intellismartinc.com")]
    ///  public interface ICommunicate
    ///  {
    ///  [OperationContract]
    ///  Caller Add(int a, int b);
    ///
    ///  [OperationContract]
    ///  Caller Subtract(int a, int b);
    ///  }
    /// </summary>
    public static class NetPipe
    {

        /// <summary>
        /// dont use http://, if localhost, localhost only will provide
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public static string SetAddress(string Url)
        {
            Regex rgx = new Regex("^(https|http)");
            Regex rgc = new Regex("^(net.pipe://)");
            if (rgx.IsMatch(Url)) Url = rgx.Replace(Url, "net.pipe");
            if (!rgc.IsMatch(Url)) Url = string.Format("net.pipe://{0}", Url);
            return Url;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ClassType"></typeparam>
        /// <typeparam name="InterfaceType"></typeparam>
        /// <param name="Address"></param>
        public static void StartServer<ClassType, InterfaceType>(string Address)
        {
            ServiceHost serviceHost = new ServiceHost(typeof(ClassType));
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            serviceHost.AddServiceEndpoint(typeof(InterfaceType), binding, SetAddress(Address));
            serviceHost.Open();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Address"></param>
        /// <returns></returns>
        public static InterfaceType StartClient<InterfaceType>(string Address)
        {
            NetNamedPipeBinding binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);
            EndpointAddress ep = new EndpointAddress(SetAddress(Address));
            return ChannelFactory<InterfaceType>.CreateChannel(binding, ep);
        }

        public static InstanceContext StartListener(object CallbackClass)
        {
            return new InstanceContext(CallbackClass);
            //CalculatorDuplexClient client = new CalculatorDuplexClient(instanceContext);
        }

        /// <summary>
        /// method wrapper for Caller functions
        /// automatically handles errors
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static Caller VoidCaller(Action<Caller> action, bool Log = true)
        {
            Caller e = new Caller();
            try
            {
                action(e);
            }
            catch (Exception ex)
            {
                e.Message = ex.Message;
                e.LogType = LogType.Error;
            }
            if (Log) Console.Write(string.Format("[{0}]<{1}> {2}\r\n", e.LogTime.ToString("HH:mm:ss tt"), e.LogType.ToString(), e.Message));
            return e;
        }

        /// <summary>
        /// Handles Client Method
        /// </summary>
        /// <param name="action"></param>
        /// <param name="SuccessMessage"></param>
        public static void CallHandler(Caller action, Action<string> SuccessMessage = null)
        {
            try
            {
                if (action.LogType == LogType.Error) throw new Exception(action.Message);
                if (SuccessMessage != null) SuccessMessage(action.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void CallHandler(Caller action, Action<Caller> LogHandled)
        {
            LogHandled(action);
        }

    }
}
