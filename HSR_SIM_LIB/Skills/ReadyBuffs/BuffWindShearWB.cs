using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

public class BuffWindShearWB : Buff
{
    public BuffWindShearWB(Unit caster, Buff reference = null) : base(caster, reference)
    {
        DoNotClone = true;
        Type = ModType.Dot;
        BaseDuration = 2;
        MaxStack = 5;
        Effects = new List<Effect> { new EffWindShear { CalculateValue = FighterUtils.CalculateShieldBrokeDmg } };
    }
}