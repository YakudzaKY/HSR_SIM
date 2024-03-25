using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     WeaknessImpair debuff. Used by silver wolf
/// </summary>
public class EffWeaknessImpair() : Effect (Condition.ConditionCheckParam.Weakness)
{
    public Ability.ElementEnm Element { get; init; }
    public override bool DynamicValue { get; } = true;

   
}