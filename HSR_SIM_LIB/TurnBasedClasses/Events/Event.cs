using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.TurnBasedClasses.Step;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

/// <summary>
///     Events. Situation changed when event was pro-ceded
/// </summary>
public abstract class Event : CloneClass
{
    public delegate IEnumerable<Unit> CalculateTargetPrc();

    public delegate double? CalculateValuePrc(Event ent);

    public List<Event> ChildEvents = new();
    private double? val; //Theoretical value

    public Event(Step parentStep, ICloneable source, Unit sourceUnit)
    {
        ParentStep = parentStep;
        Source = source;
        SourceUnit = sourceUnit;
    }

    public CalculateValuePrc CalculateValue { get; set; }
    public CalculateTargetPrc CalculateTargets { get; init; }


    public ICloneable Source { get; }
    public Ability.TargetTypeEnm? TargetType { get; set; }

    public Ability.AbilityCurrentTargetEnm? CurrentTargetType { get; set; }
    public Unit SourceUnit { get; set; }
    public Unit TargetUnit { get; set; }
    public StepTypeEnm? OnStepType { get; init; }

    //after calc Val will be multiplied by this number
    public double? CalculateProportion { get; set; } = null;

    public double? Val
    {
        get
        {
            //calc value first

            if (CalculateValue != null)
            {
                val = CalculateValue(this);
                CalculateValue = null;
            }

            if (CalculateProportion != null)
            {
                val *= CalculateProportion;
                CalculateProportion = null;
                ParentStep.Parent.Parent?.LogDebug($" new val proportion({CalculateProportion:f})={val:f}");
            }



            return val;
        }

        set => val = value;
    }

    public double? RealVal { get; set; }



    public Step ParentStep { get; set; }
    public bool TriggersHandled { get; set; }

    public bool IsDamageEvent => this is ToughnessBreak or DoTDamage or DirectDamage or ToughnessBreakDoTDamage;
    public Event Reference { get; set; }

    public abstract string GetDescription();


    /// <summary>
    ///     Proc one event. Used after child
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="revert"></param>
    public virtual void ProcEvent(bool revert)
    {
        ParentStep.ProceedEvents.Add(this);

        //call handlers
        if (!TriggersHandled)
        {
            TriggersHandled = true;
            //proc events only in battle
            if (ParentStep.Parent.CurrentFight != null)
                ParentStep.Parent.EventHandlerProc?.Invoke(this);
        }

        //Child events
        foreach (var e in ChildEvents) e.ProcEvent(revert);
        ChildEvents.Clear();
    }


    /// <summary>
    ///     Remove modifictaion
    /// </summary>
    /// <param name="mod">BuffToApply</param>
    /// <param name="naturalFinish">If true - MOD exceed by duratiuon. If false - dispeleed by ability</param>
    /// <exception cref="NotImplementedException"></exception>
    public void DispelMod(Buff mod, bool naturalFinish)
    {
        if (naturalFinish) mod.ProceedNaturalExpire(this);
        var dispell = new RemoveBuff(ParentStep, ParentStep.ActorAbility, SourceUnit)
        { BuffToApply = mod, TargetUnit = TargetUnit };
        ChildEvents.Add(dispell);
    }


    /// <summary>
    ///     attempt to apply debuff
    /// </summary>
    /// <param name="modType">What we modificate</param>
    /// <param name="effects">effect list </param>
    /// <param name="baseDuration">Duration of the mod</param>
    /// <param name="baseChance">base chance of debuff</param>
    /// <param name="maxStack">max stacks</param>
    /// <param name="uniqueStr">Unique buff per battle</param>
    /// <param name="uniqueUnit">Unique buff per unit</param>
    public void TryDebuff(Buff mod, double baseChance)
    {
        //add Dots and debuffs
        ApplyBuff dotEvent = new(ParentStep, Source, SourceUnit)
        {
            TargetUnit = TargetUnit,
            BuffToApply = mod
        };

        if (FighterUtils.CalculateDebuffApplied(dotEvent, baseChance))
        {
            ChildEvents.Add(dotEvent);
        }
        else
        {
            //debuff apply failed
            DebuffResisted failEvent = new(ParentStep, Source, SourceUnit)
            {
                TargetUnit = TargetUnit,
                BuffToApply = dotEvent.BuffToApply
            };
            ChildEvents.Add(failEvent);
        }
    }
}