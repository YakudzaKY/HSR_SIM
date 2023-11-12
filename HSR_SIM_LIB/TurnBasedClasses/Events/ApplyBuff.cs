﻿using System;
using System.Linq;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//apply buff debuff dot etc
public class ApplyBuff : BuffEventTemplate
{
    public ApplyBuff(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    //Base Chance to apply debuff
    public double BaseChance { get; set; }

    public override string GetDescription()
    {
        return $"Apply buff on {TargetUnit.Name}. Source: {Source?.GetType()?.Name:s}";
    }

    public override void ProcEvent(bool revert)
    {
        //Apply mod
        BuffToApply.AbilityValue ??= AbilityValue;
        if (!TargetUnit.IsAlive) return;
        //calc value first
        foreach (var modEffect in BuffToApply.Effects.Where(modEffect =>
                     modEffect.CalculateValue != null && modEffect.Value == null))
            modEffect.Value = modEffect.CalculateValue(this);

        if (!revert)
            TargetUnit.ApplyBuff(this, BuffToApply);
        else
            TargetUnit.RemoveBuff(this, BuffToApply);

        base.ProcEvent(revert);
    }
}