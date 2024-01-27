using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.NPC.MaraStruckSoldierStuff;

public class MsSoldRejuvenate : Buff
{
    public MsSoldRejuvenate(Unit caster, Buff reference = null, Event ent=null) : base(caster, reference)
    {
        DoNotClone = true;
        Dispellable = true;
        Type = BuffType.Buff;
        Effects = new List<Effect>
        {
            new EffRebirth()
        };
        EventHandlerProc += RejuvenateEventHandler;
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

    private void RejuvenateEventHandler(Event ent)
    {
        //ability cast is finish. 
        if (ent is ExecuteAbilityFinish && ent.ParentStep.ActorAbility.Attack)
            //if got hit by ability and no debuff applied in this action
            if (UnitGotHitByAbility(ent.ParentStep.ProceedEvents) && !UnitGotDebuffByAbility(ent.ParentStep.ProceedEvents))
              ent.ChildEvents.Add(new ApplyBuffStack(ent.ParentStep,this,Caster){Stacks=1,BuffToApply = this,TargetUnit = Owner});  
    }
}