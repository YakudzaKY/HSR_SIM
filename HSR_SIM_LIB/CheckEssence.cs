using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public class CheckEssence : ICloneable
    {
        internal List<Condition> ExecuteWhen { get => executeWhen; set => executeWhen = value; }
        private List<Condition> executeWhen = new List<Condition>();

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
