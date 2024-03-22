using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     increase or reduce speed by x
/// </summary>
public class EffSpeed() : Effect(Condition.ConditionCheckParam.Spd)
{
}