using System.Collections.Generic;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

/// <summary>
///     Entanglement on weakness break
/// </summary>
public class AppliedBuffEntanglementWb : AppliedBuff
{
    public AppliedBuffEntanglementWb(Unit sourceUnit, AppliedBuff reference = null) : base(sourceUnit, reference,
        typeof(AppliedBuffEntanglementWb))
    {
        Type = BuffType.Debuff;
        BaseDuration = 1;
        MaxStack = 5;
        Effects = new List<Effect>
        {
            new EffEntanglement { DoTCalculateValue = FighterUtils.WeaknessBreakFormula() },
            new EffDelay
            {
                CalculateValue = new Formula
                {
                    Expression =
                        $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.BreakDmg)} * 0.20"
                },
                StackAffectValue = false
            }
        };
        EventHandlerProc += EntanglementEventHandler;
    }


    private bool UnitGotHitByAbility(List<Event> events)
    {
        foreach (var ent in events)
            if (ent is DirectDamage && ent.TargetUnit == CarrierUnit)
                return true;
        return false;
    }

    private bool UnitGotDebuffByAbility(List<Event> events)
    {
        foreach (var ent in events)
            if (ent is ApplyBuff aBuff && ent.TargetUnit == CarrierUnit && aBuff.AppliedBuffToApply == this)
                return true;
        return false;
    }

    private void EntanglementEventHandler(Event ent)
    {
        //ability cast is finish. 
        if (ent is ExecuteAbilityFinish && ent.ParentStep.ActorAbility.Attack)
            //if got hit by ability and no debuff applied in this action
            if (UnitGotHitByAbility(ent.ParentStep.ProceedEvents) &&
                !UnitGotDebuffByAbility(ent.ParentStep.ProceedEvents))
                ent.ChildEvents.Add(new ApplyBuffStack(ent.ParentStep, this, SourceUnit)
                    { Stacks = 1, AppliedBuffToApply = this, TargetUnit = CarrierUnit });
    }
}