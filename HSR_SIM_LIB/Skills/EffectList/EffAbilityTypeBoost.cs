namespace HSR_SIM_LIB.Skills.EffectList;
/// <summary>
/// Boost ability type damage. For example buff to increase ultimate damage by x%
/// </summary>
public class EffAbilityTypeBoost : Effect
{
    public Ability.AbilityTypeEnm AbilityType { get; set; }
}