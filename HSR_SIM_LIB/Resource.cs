using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Class for resources. It can be party (like SP) or character HP
    /// </summary>
    public class Resource: CloneClass
    {
        public ResourceType ResType { get; set; }

        public double ResVal { get; set; }

        public enum ResourceType
        {
            nil,//dont use type
            TP,
            SP,
            HP,
            Toughness,
            Barrier
        }
    }
}
