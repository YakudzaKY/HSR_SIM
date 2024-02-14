using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;
/// <summary>
/// Entanglement on weakness break
/// </summary>
public class BuffEntanglementWB : Buff
{
    public BuffEntanglementWB(Unit caster, Buff reference = null, Event ent=null) : base(caster, reference)
    {
        DoNotClone = true;
        Dispellable = true;
        Type = BuffType.Debuff;
        BaseDuration = 1;
        MaxStack = 5;
        Effects = new List<Effect>
        {
            new EffEntanglement { DoTCalculateValue = FighterUtils.CalculateShieldBrokeDmg },
            new EffDelay
            {
                Value = 0.20 * (1 + caster.GetBreakDmg(ent)),
                StackAffectValue = false
            }
        };
        EventHandlerProcAfter += EntanglementEventHandler;
    }

    private bool UnitGotHitByAbility(List<Event> events)
    {
        foreach (Event ent in events)
        {
            if (ent is DirectDamage && ent.TargetUnit == Owner)
            {
                return true;
            }
   
        }
        return false;
    }

    private bool UnitGotDebuffByAbility(List<Event> events)
    {
        foreach (Event ent in events)
        {
            if (ent is ApplyBuff aBuff && ent.TargetUnit == Owner && aBuff.BuffToApply == this)
            {
                return true;
            }


        }
        return false;
    }

    private void EntanglementEventHandler(Event ent)
    {
        //ability cast is finish. 
        if (ent is ExecuteAbilityFinish && ent.ParentStep.ActorAbility.Attack)
            //if got hit by ability and no debuff applied in this action
            if (UnitGotHitByAbility(ent.ParentStep.ProceedEvents) && !UnitGotDebuffByAbility(ent.ParentStep.ProceedEvents))
              ent.ChildEvents.Add(new ApplyBuffStack(ent.ParentStep,this,Caster){Stacks=1,BuffToApply = this,TargetUnit = Owner});  
    }
}