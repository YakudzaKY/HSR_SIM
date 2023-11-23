using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

public class BuffEntanglementWB : Buff
{
    public BuffEntanglementWB(Unit caster, Buff reference = null) : base(caster, reference)
    {
        DoNotClone = true;
        Dispellable = true;
        Type = BuffType.Debuff;
        BaseDuration = 1;
        MaxStack = 5;
        Effects = new List<Effect>
        {
            new EffEntanglement { CalculateValue = FighterUtils.CalculateShieldBrokeDmg },
            new EffDelay
            {
                Value = 0.20 * (1 + caster.GetBreakDmg(null)),
                StackAffectValue = false
            }
        };
        EventHandlerProc += EntanglementEventHandler;
    }

    public void EntanglementEventHandler(Event ent)
    {
        if (ent is DirectDamage && ent.TargetUnit == Owner)
            Stack = Math.Min(Stack + 1, MaxStack);
    }
}