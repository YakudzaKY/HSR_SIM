using System.Collections.Generic;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

/// <summary>
///     Wind DoT on weakness break
/// </summary>
public class AppliedBuffWindShearWb : AppliedBuff
{
    public AppliedBuffWindShearWb(Unit sourceUnit, AppliedBuff reference = null) : base(sourceUnit, reference)
    {
        Type = BuffType.Dot;
        BaseDuration = 2;
        MaxStack = 5;
        Effects = new List<Effect> { new EffWindShear { DoTCalculateValue = FighterUtils.CalculateShieldBrokeDmg } };
    }
}