using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class AppliedBuff(Unit sourceUnit, Buff reference = null) : Buff(sourceUnit, reference)
{
    public delegate void EventHandler(Event ent);

    public delegate void StepHandler(Step step);



    public enum EffectStackingTypeEnm
    {
        PickMax,
        Plus,
        FullReplace
    }

    //dot will be auto on start
    private static readonly List<Type> EarlyProcMods = [typeof(EffEntanglement)];

    public EffectStackingTypeEnm EffectStackingType { get; init; } = EffectStackingTypeEnm.PickMax;





    //buff marked old at turn end
    public bool IsOld { get; set; } = false;

    //debuf is CC? 
    public bool CrowdControl
    {
        get
        {
            return Effects.Any(x =>
                x is EffEntanglement
                    or EffFreeze
                    or EffImprisonment
                    or EffDominated
                    or EffOutrage);
        }
    }


    public IFighter.EventHandler EventHandlerProc { get; set; }
    public IFighter.StepHandler StepHandlerProc { get; set; }
    public int? BaseDuration { get; set; }
    public int? DurationLeft { get; set; }

    public object SourceObject { get; set; }

    public string UniqueStr { get; set; }

    public bool Dispellable { get; init; } = true;
    public Unit UniqueUnit { get; set; }

    //do buff/debuff work on turn start?(DoT always at start)
    public bool IsEarlyProc()
    {
        foreach (var effect in Effects)
            if (EarlyProcMods.Contains(effect.GetType()))
                return true;

        return false;
    }

    //proceed the mod proc(dot tick etc)
    public void Proceed(Step step)
    {
        //do some shit
        if (Type == BuffType.Dot)
            foreach (EffDotTemplate effect in Effects.Where(x => x is EffDotTemplate))
                if (Reference == SourceUnit.Fighter.WeaknessBreakDebuff)
                {
                    var dotProcEvent = new ToughnessBreakDoTDamage(step, SourceUnit, SourceUnit, effect.Element)
                    {
                        CalculateValue = effect.DoTCalculateValue, TargetUnit = step.Actor, BuffThatDamage = this
                    };
                    step.Events.Add(dotProcEvent);
                }

                else
                {
                    var dotProcEvent = new DoTDamage(step, SourceUnit, SourceUnit, effect.Element)
                    {
                        CalculateValue = effect.DoTCalculateValue, TargetUnit = step.Actor, BuffThatDamage = this
                    };
                    step.Events.Add(dotProcEvent);
                }

        //only Buffs applied before turn started
        if (IsOld)
        {
            //minus duration
            Event reduceModDuration = new ReduceDuration(step, SourceUnit, SourceUnit)
            {
                AppliedBuffToApply = this,
                TargetUnit = step.Actor
            };
            step.Events.Add(reduceModDuration);
        }
    }

    /// <summary>
    ///     handle the Buff
    /// </summary>
    /// <param name="ent"></param>
    public void ProceedNaturalExpire(Event ent)
    {
        //delayed damage
        foreach (var x in Effects) x.OnNaturalExpire(ent, this);
    }

    public string GetDescription()
    {
        var modsStr = "";
        foreach (var eff in Effects) modsStr += $"{eff.GetType().Name:s} val= {eff.Value:f} ; ";

        return
            $">> {Type.ToString():s} for {modsStr:s} duration={BaseDuration.ToString():D} dispellable={Dispellable.ToString():s}";
    }
}