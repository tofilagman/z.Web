using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace z.Web
{

    public class Parameter
    {
        public Dictionary<string, object> args;

        public Parameter()
        {
            this. args = new Dictionary<string, object>();
        }

        public Parameter(string Key, object Value) : this()
        {
            this.Add(Key, Value);
        }

        public Parameter Add(string Key, object Value)
        {
            args.Add(Key, Value);
            return this;
        }

        public override string ToString()
        {
            List<string> j = new List<string>();
            foreach (KeyValuePair<string, object> a in args)
            {
                j.Add($"{a.Key}={a.Value}");
            }
            return string.Join("&", j.ToArray());
        }

       // public static Parameter Construct = new Parameter();
    }
}
