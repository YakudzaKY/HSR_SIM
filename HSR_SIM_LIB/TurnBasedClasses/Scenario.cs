using System;
using System.Collections.Generic;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses;

/// <summary>
///     Scenario class. Using for combat
/// </summary>
public class Scenario : ICloneable
{
    public string Name { get; internal set; }
    public string ShortName { get; internal set; }
    internal List<Fight> Fights { get; set; }

    internal List<Unit> Party { get; set; }

    public List<Unit> SpecialUnits { get; set; }

    public object Clone()
    {
        var newClone = (Scenario)MemberwiseClone();
        if (newClone.Party != null)
        {
            var oldParty = newClone.Party;
            newClone.Party = new List<Unit>();
            foreach (var unit in oldParty) newClone.Party.Add((Unit)unit.Clone());
        }

        if (newClone.SpecialUnits != null)
        {
            var oldSpecial = newClone.SpecialUnits;
            newClone.SpecialUnits = new List<Unit>();
            foreach (var unit in oldSpecial) newClone.SpecialUnits.Add((Unit)unit.Clone());
        }

        return newClone;
    }
}