using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     bleed DoT debuff
/// </summary>
public class EffBleed : EffDotTemplate
{
    public override Unit.ElementEnm Element { get; init; } = Unit.ElementEnm.Physical;
}