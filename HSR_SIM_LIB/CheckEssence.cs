using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    public class CheckEssence : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
