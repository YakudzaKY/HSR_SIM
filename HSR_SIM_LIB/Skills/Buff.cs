using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class Buff(Unit caster, Buff reference = null) : CloneClass
{
    public delegate void EventHandler(Event ent);

    public delegate void StepHandler(Step step);

    public EffectStackingTypeEnm EffectStackingType { get; set; } = EffectStackingTypeEnm.PickMax;
    public enum BuffType
    {
        Buff,
        Debuff,
        Dot
    }

    public enum EffectStackingTypeEnm
    {
        PickMax,
        Plus,
        FullReplace
    }
    //dot will be auto on start
    public static List<Type> EarlyProcMods = new() { typeof(EffEntanglement) };


    public BuffType Type { get; init; }

    public string CustomIconName { get; set; }
    public List<Effect> Effects { get; set; } = new();


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
    public Unit Caster { get; set; } = caster;
    public object SourceObject { get; set; }

    public string UniqueStr { get; set; }

    public Buff Reference { get; set; } = reference;

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
        if (Type == BuffType.Dot)
            foreach (EffDotTemplate effect in Effects.Where(x=>x is EffDotTemplate))
                if (Reference == Caster.Fighter.ShieldBreakDebuff)
                {
                    var dotProcEvent = new ToughnessBreakDoTDamage(step, Caster, Caster,effect.Element)
                    {
                        CalculateValue = effect.DoTCalculateValue, TargetUnit = step.Actor, Modification = this
                    };
                    step.Events.Add(dotProcEvent);
                }

                else
                {
                    var dotProcEvent = new DoTDamage(step, Caster, Caster,effect.Element)
                    {
                        CalculateValue = effect.DoTCalculateValue, TargetUnit = step.Actor, Modification = this
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

    public override object Clone()
    {
        Buff newClone =(Buff)MemberwiseClone();
        var oldEff=newClone.Effects;
        newClone.Effects = new List<Effect>();
        //clone calced effects
        foreach (Effect eff in oldEff)
        {
            if (eff.DynamicValue)
                newClone.Effects.Add((Effect)eff.Clone());
            else
            {
                newClone.Effects.Add(eff);
            }
        }


        return newClone;
    }
 
}