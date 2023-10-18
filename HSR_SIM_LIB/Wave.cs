using System.Collections.Generic;

namespace HSR_SIM_LIB
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
