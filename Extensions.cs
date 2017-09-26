using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace z.Web
{
    public static class Extensions
    {
        public static string ReSerializeJson(this string json, string root, string arrlst)
        {
            dynamic hj = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(hj[root][arrlst]);
        }

        public static T Deserialize<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string GetWithBackslash(this string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar.ToString();
            }

            return path;
        }
        
    }
}
