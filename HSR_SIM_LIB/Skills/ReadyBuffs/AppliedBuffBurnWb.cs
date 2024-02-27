using System.Collections.Generic;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

/// <summary>
///     burn DoT on weakness break
/// </summary>
public class AppliedBuffBurnWb : AppliedBuff
{
    public AppliedBuffBurnWb(Unit sourceUnit, AppliedBuff reference = null) : base(sourceUnit, reference)
    {
        Type = BuffType.Dot;
        BaseDuration = 2;
        Effects = new List<Effect> { new EffBurn { DoTCalculateValue = FighterUtils.CalculateShieldBrokeDmg } };
    }
}