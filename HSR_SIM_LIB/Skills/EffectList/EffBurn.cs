namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     burn DoT
/// </summary>
public class EffBurn : EffDotTemplate
{
    public override Ability.ElementEnm Element { get; init; } = Ability.ElementEnm.Fire;
}