using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     generic class for Elemental effects
/// </summary>
public class EffElementalTemplate (Condition.ConditionCheckParam resetDependency): Effect(resetDependency)
{
    public Ability.ElementEnm Element { get; init; }
}