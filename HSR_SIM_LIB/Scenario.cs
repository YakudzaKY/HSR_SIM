using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{/// <summary>
/// Scenario class. Using for combat
/// </summary>
    internal class Scenario
    {
        List<Fight> fights;
        public string Name { get; internal set; }
        internal List<Fight> Fights { get => fights; set => fights = value; }
        internal List<Unit> Party { get => party; set => party = value; }
        public List<Unit> SpecialUnits { get; set; }

        List<Unit> party;
    }
}
