﻿using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

/// <summary>
///     shock DoT on weakness break
/// </summary>
public class AppliedBuffShockWb : AppliedBuff
{
    public AppliedBuffShockWb(Unit sourceUnit, AppliedBuff reference = null) : base(sourceUnit, reference)
    {
        Type = BuffType.Dot;
        BaseDuration = 2;
        Effects = [new EffShock { DoTCalculateValue = FighterUtils.CalculateShieldBrokeDmg }];
    }
}