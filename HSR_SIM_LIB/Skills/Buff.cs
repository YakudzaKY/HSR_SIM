﻿using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class Buff : CloneClass
{
    public delegate void EventHandler(Event ent);

    public delegate void StepHandler(Step step);


    public enum ModType
    {
        Buff,
        Debuff,
        Dot
    }

    //dot will be auto on start
    public static List<Type> EarlyProcMods = new() { typeof(EffEntanglement) };


    public Buff(Unit caster, Buff reference = null)
    {
        Reference = reference;
        Caster = caster;
    }

    public ModType Type { get; init; }

    public string CustomIconName { get; set; }
    public List<Effect> Effects { get; init; } = new();
    public Ability AbilityValue { get; set; }

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


    public Unit Owner { get; set; }


    public IFighter.EventHandler EventHandlerProc { get; set; }
    public IFighter.StepHandler StepHandlerProc { get; set; }
    public int Stack { get; set; } = 1;

    public int MaxStack { get; set; } = 1;
    public int? BaseDuration { get; set; }
    public int? DurationLeft { get; set; }
    public Unit Caster { get; set; }
    public object SourceObject { get; set; }

    public string UniqueStr { get; set; }

    public Buff Reference { get; set; }

    public bool Dispellable { get; init; } = true;
    public Unit UniqueUnit { get; set; }
    public bool DoNotClone { get; set; } = false;

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
        if (Type == ModType.Dot)
            foreach (var effect in Effects)
                if (Reference == Caster.Fighter.ShieldBreakMod)
                {
                    var dotProcEvent = new ToughnessBreakDoTDamage(step, Caster, Caster)
                    {
                        CalculateValue = effect.CalculateValue, TargetUnit = step.Actor, Modification = this,
                        AbilityValue = AbilityValue
                    };
                    step.Events.Add(dotProcEvent);
                }

                else
                {
                    var dotProcEvent = new DoTDamage(step, Caster, Caster)
                    {
                        CalculateValue = effect.CalculateValue, TargetUnit = step.Actor, Modification = this,
                        AbilityValue = AbilityValue
                    };
                    step.Events.Add(dotProcEvent);
                }

        //only Buffs aplied before turn started
        if (IsOld)
        {
            //minus duration
            Event reduceModDuration = new ReduceDuration(step, Caster, Caster)
            {
                BuffToApply = this,
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