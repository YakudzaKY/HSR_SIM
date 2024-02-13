using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;
/// <summary>
/// shock DoT on weakness break
/// </summary>
public class BuffShockWB : Buff
{
    public BuffShockWB(Unit caster, Buff reference = null) : base(caster, reference)
    {
        DoNotClone = true;
        Type = BuffType.Dot;
        BaseDuration = 2;
        Effects = new List<Effect> { new EffShock { DoTCalculateValue = FighterUtils.CalculateShieldBrokeDmg } };
    }
}