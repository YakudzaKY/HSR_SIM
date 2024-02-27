using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     generic class for DoT classes
/// </summary>
public abstract class EffDotTemplate : Effect
{
    public Event.CalculateValuePrc DoTCalculateValue { get; init; }
    public abstract Unit.ElementEnm Element { get; init; }
}