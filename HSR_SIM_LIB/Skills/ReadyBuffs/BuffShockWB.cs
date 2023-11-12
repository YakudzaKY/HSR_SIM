using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

public class BuffShockWB : Buff
{
    public BuffShockWB(Unit caster, Buff reference = null) : base(caster, reference)
    {
        DoNotClone = true;
        Type = ModType.Dot;
        BaseDuration = 2;
        Effects = new List<Effect> { new EffShock { CalculateValue = FighterUtils.CalculateShieldBrokeDmg } };
    }
}