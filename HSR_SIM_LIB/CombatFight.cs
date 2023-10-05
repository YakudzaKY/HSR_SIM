using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    internal class CombatFight : Fight
    {
        List<CombatUnit> units;

        public CombatFight()
        {

        }

        internal List<CombatUnit> Units { get => units; set => units = value; }
    }
}
