using System.Collections.Generic;

namespace HSR_SIM_LIB.TurnBasedClasses;

public class Fight
{
    public string Name { get; internal set; }
    internal List<Wave> Waves { get; set; }
}