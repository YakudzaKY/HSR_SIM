using System.Collections.Generic;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses;

/// <summary>
///     Scenario class. Using for combat
/// </summary>
internal class Scenario
{
    public string Name { get; internal set; }
    internal List<Fight> Fights { get; set; }

    internal List<Unit> Party { get; set; }

    public List<Unit> SpecialUnits { get; set; }
}