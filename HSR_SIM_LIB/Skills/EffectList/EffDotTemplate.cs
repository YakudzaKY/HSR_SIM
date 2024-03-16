using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     generic class for DoT classes
/// </summary>
public abstract class EffDotTemplate : Effect
{
    public Formula DoTCalculateValue { get; init; }
    public abstract Ability.ElementEnm Element { get; init; }
}