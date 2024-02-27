using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.TurnBasedClasses.Step;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

/// <summary>
///     Events. Situation changed when event was pro-ceded
/// </summary>
public abstract class Event(Step parentStep, ICloneable source, Unit sourceUnit) : CloneClass
{
    public delegate IEnumerable<Unit> CalculateTargetPrc();

    public delegate double? CalculateValuePrc(Event ent);

    public readonly List<Event> ChildEvents = [];
    private readonly int procRatio = 1;

    private int procRatioCounter = 1; //counter
    private double? val; //Theoretical value

    public CalculateValuePrc CalculateValue { get; set; }
    public CalculateTargetPrc CalculateTargets { get; init; }


    public ICloneable Source { get; } = source;
    public Ability.TargetTypeEnm? TargetType { get; init; }

    public int ProcRatio
    {
        get => procRatio;
        init
        {
            if (value < 1) throw new Exception("ProcRatio cant be lower than 1");
            procRatio = value;
        }
    } //how ofter event proc?


    private ProcRatioDirectionEnm ProcRatioDirection { get; } = ProcRatioDirectionEnm.Descending;

    public bool IsReady => procRatioCounter == 1; //event is ready to be applied on target

    public Ability.AbilityCurrentTargetEnm? CurrentTargetType { get; init; }
    public Unit SourceUnit { get; set; } = sourceUnit;
    public Unit TargetUnit { get; set; }
    public StepTypeEnm? OnStepType { get; init; }

    //after calc Val will be multiplied by this number
    public double? CalculateProportion { get; set; }

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

    public double? RealVal { get; protected set; }


    public Step ParentStep { get; set; } = parentStep;
    protected bool TriggersHandled { get; private set; }

    public bool IsDamageEvent => this is ToughnessBreak or DoTDamage or DirectDamage or ToughnessBreakDoTDamage;
    public Event Reference { get; set; }

    /*
    public int Cooldown { get; init; } = 0;
    public int CooldownTimer { get; set; } = 0;*/

    //reset event params
    public void OnEnteringBattle()
    {
        //reset cd
        //CooldownTimer = Cooldown;

        //reset Ratio counter
        if (ProcRatioDirection == ProcRatioDirectionEnm.Descending)
            procRatioCounter = 1;
        else
            SetMaxCounter();
    }

    public abstract string GetDescription();


    /// <summary>
    ///     Proc one event. Used after child
    /// </summary>
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
    ///     Remove modification
    /// </summary>
    /// <param name="mod">BuffToApply</param>
    /// <param name="naturalFinish">If true - MOD exceed by duration. If false - dispelled by ability</param>
    /// <exception cref="NotImplementedException"></exception>
    public void DispelMod(AppliedBuff mod, bool naturalFinish)
    {
        if (naturalFinish) mod.ProceedNaturalExpire(this);
        var dispell = new RemoveBuff(ParentStep, ParentStep.ActorAbility, SourceUnit)
            { AppliedBuffToApply = mod, TargetUnit = TargetUnit };
        ChildEvents.Add(dispell);
    }


    /// <summary>
    ///     attempt to apply debuff
    /// </summary>
    /// <param>
    ///     base chance of debuff
    ///     <name>baseChance</name>
    /// </param>
    /// <param name="mod"></param>
    /// <param name="baseChance"></param>
    protected void TryDebuff(AppliedBuff mod, double baseChance)
    {
        //add Dots and debuffs
        ApplyBuff applyBuff = new(ParentStep, Source, SourceUnit)
        {
            TargetUnit = TargetUnit,
            AppliedBuffToApply = mod
        };

        if (FighterUtils.CalculateDebuffApplied(applyBuff, baseChance))
        {
            ChildEvents.Add(applyBuff);
        }
        else
        {
            //debuff apply failed
            DebuffResisted failEvent = new(ParentStep, Source, SourceUnit)
            {
                TargetUnit = TargetUnit,
                AppliedBuffToApply = applyBuff.AppliedBuffToApply
            };
            ChildEvents.Add(failEvent);
        }
    }

    //reduce the ratio counter by 1
    public void ReduceRatioCounter()
    {
        procRatioCounter--;
        if (procRatioCounter == 0) throw new Exception("procRatioCounter cant be lower than 1");
    }

    //set counter on max value
    public void SetMaxCounter()
    {
        procRatioCounter = ProcRatio;
    }

    private enum ProcRatioDirectionEnm
    {
        Descending, //proc on first use then counter

        // ReSharper disable once UnusedMember.Local
        Ascending //counter and proc at finish
    }
}