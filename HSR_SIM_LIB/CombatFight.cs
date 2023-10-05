using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    internal class CombatFight : Fight
    {
        List<Unit> units;

        public CombatFight()
        {

        }

        internal List<Unit> Units { get => units; set => units = value; }
    }
}
