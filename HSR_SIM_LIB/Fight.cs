using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    internal class Fight
    {
        List<Wave> waves;

        public string Name { get; internal set; }
        internal List<Wave> Waves { get => waves; set => waves = value; }
    }
}
