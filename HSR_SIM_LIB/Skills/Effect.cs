﻿using System;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class Effect:CloneClass
{
    /// <summary>
    /// func ref that Calculate effect value on buff apply.
    /// if RealTimeRecalculateValue== true then recalc will be every time on buff parsing 
    /// </summary>
    public Event.CalculateValuePrc CalculateValue { get; init; }

    public double? Value { get; set; }

    /// <summary>
    /// DynamicValue - value of effect can be changed
    /// </summary>
    public virtual bool DynamicValue => CalculateValue != null;

    /// <summary>
    /// Do recalc value when we parse buff on unit
    /// (!) Should use on  ConditionBuff && PassiveBuff because no buff apply event for them
    /// </summary>
    public virtual bool RealTimeRecalculateValue { get; init; } = false;

    public bool StackAffectValue { get; set; } = true; // do we multiply final value by stack count ?

    /// <summary>
    ///     on natural (by timer) expire but not expired
    /// </summary>
    public virtual void OnNaturalExpire(Event ent, Buff buff)
    {
    }

    /// <summary>
    ///     After buff applied
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="buff"></param>
    /// <param name="target">override buff owner</param>
    public virtual void OnApply(Event ent, Buff buff,Unit target=null)
    {
    }

    /// <summary>
    ///     Before buff applied. buff.Owner will be null
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="buff"></param>
    /// <param name="target">override buff owner</param>
    public virtual void BeforeApply(Event ent, Buff buff,Unit target=null)
    {
    }

    /// <summary>
    ///     after buff removed. buff.Owner can be null
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="buff"></param>
    /// <param name="target">override buff owner</param>
    public virtual void OnRemove(Event ent, Buff buff,Unit target=null)
    {
    }

    /// <summary>
    ///     Before buff removed
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="buff"></param>
    /// <param name="target">override buff owner</param>
    public virtual void BeforeRemove(Event ent, Buff buff,Unit target=null)
    {
    }

  
}