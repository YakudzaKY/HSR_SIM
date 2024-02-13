using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;
/// <summary>
/// shock dot debuff
/// </summary>
public class EffShock : EffDotTemplate
{
    public override Unit.ElementEnm Element { get; init; } = Unit.ElementEnm.Lightning;
}