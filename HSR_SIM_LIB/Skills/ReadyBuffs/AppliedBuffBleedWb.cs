using System.Collections.Generic;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

/// <summary>
///     Bleed DoT on weakness break
/// </summary>
public class AppliedBuffBleedWb : AppliedBuff
{
    public AppliedBuffBleedWb(Unit sourceUnit, AppliedBuff reference =null ) : base(sourceUnit, reference,typeof(AppliedBuffBleedWb))
    {
        Type = BuffType.Dot;
        BaseDuration = 2;
        Effects = new List<Effect> { new EffBleed { DoTCalculateValue = FighterUtils.CalculateShieldBrokeDmg } };
    }
}