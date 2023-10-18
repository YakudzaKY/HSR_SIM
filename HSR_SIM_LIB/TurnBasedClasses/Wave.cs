using System.Collections.Generic;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses
{
    /// <summary>
    /// Waves ins the fight
    /// </summary>
    internal class Wave
    {
        List<Unit> units;
        internal List<Unit> Units { get => units; set => units = value; }
    }
}
