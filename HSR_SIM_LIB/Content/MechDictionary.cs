using System.Collections.Generic;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.Content;

/// <summary>
///     dictionary with mechanics
/// </summary>
public class MechDictionary
{
    public Dictionary<Ability, double> Values { get; } = new();


    //reset values in mich dictionary
    public void Reset()
    {
        foreach (var item in Values) Values[item.Key] = 0;
    }


    public void AddVal(Ability ability)
    {
        Values.Add(ability, 0);
    }
}