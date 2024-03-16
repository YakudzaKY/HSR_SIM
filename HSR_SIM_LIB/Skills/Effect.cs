using System;
using System.Globalization;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB.Skills;

public class Effect : CloneClass
{
    /// <summary>
    ///     func ref that Calculate effect value on buff apply.
    ///     if RealTimeRecalculateValue== true then recalc will be every time on buff parsing
    /// </summary>
    public Event.CalculateValuePrc CalculateValue { get; init; }

    public double? Value { get; set; }

    /// <summary>
    ///     DynamicValue - value of effect can be changed
    /// </summary>
    public virtual bool DynamicValue => CalculateValue != null;

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
    public virtual void OnApply(Event ent, Buff buff, Unit target = null)
    {
    }

    /// <summary>
    ///     Before buff applied. buff.Owner will be null
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="buff"></param>
    /// <param name="target">override buff owner</param>
    public virtual void BeforeApply(Event ent, Buff buff, Unit target = null)
    {
    }

    /// <summary>
    ///     after buff removed. buff.Owner can be null
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="buff"></param>
    /// <param name="target">override buff owner</param>
    public virtual void OnRemove(Event ent, Buff buff, Unit target = null)
    {
    }

    /// <summary>
    ///     Before buff removed
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="buff"></param>
    /// <param name="target">override buff owner</param>
    public virtual void BeforeRemove(Event ent, Buff buff, Unit target = null)
    {
    }

    public string Explain(double? calculatedValue=null)
    {
        string val = (Value == null) ? "*" : Value.ToString();
        if (Value is null&&calculatedValue!=null)
            val= "~"+calculatedValue;
        return $"{GetType().Name} ({val}) ";
    }


}