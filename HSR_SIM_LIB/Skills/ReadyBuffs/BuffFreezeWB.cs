﻿using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

public class BuffFreezeWB : Buff
{
    public BuffFreezeWB(Unit caster, Buff reference = null) : base(caster, reference)
    {
        DoNotClone = true;
        Type = BuffType.Debuff;
        BaseDuration = 1;
        Effects = new List<Effect> { new EffFreeze { CalculateValue = FighterUtils.CalculateShieldBrokeDmg } };
        EventHandlerProc += FreezeEventHandler;
    }

    public void FreezeEventHandler(Event ent)
    {
        if (ent is ResetAV && ent.TargetUnit == Owner) //50% reduce av if frosted
            ent.ChildEvents.Add(new ModActionValue(ent.ParentStep, Owner, Owner)
            {
                TargetUnit = Owner, Val = Owner.GetBaseActionValue(ent) * 0.5
            });
    }
}