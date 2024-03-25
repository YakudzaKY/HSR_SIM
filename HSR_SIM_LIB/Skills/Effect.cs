using System;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB.Skills;

public class Effect : CloneClass
{
    /// <summary>
    /// can be Formula or ref to Formula func
    ///     func ref that Calculate effect value on buff apply.
    ///     if RealTimeRecalculateValue== true then recalculate will be every time on buff parsing
    /// </summary>
    public object CalculateValue
    {
        get => calculateValue;
        set
        {
            if (value != null && value is not Formula && value is not Func<Event, Formula>)
                throw new Exception("CalculateValue should be Formula or Func<Event, Formula>");
            calculateValue = value;
        }
    }

    public object ResetDependency { get; }
    private object calculateValue;

    protected Effect(Condition.ConditionCheckParam? resetDependency=null)
    {
        ResetDependency = resetDependency??(object)this.GetType();
    }

    public double? Value { get; set; }

    /// <summary>
    ///     DynamicValue - value of effect can be changed
    /// </summary>
    public virtual bool DynamicValue => CalculateValue != null;


    public bool StackAffectValue { get; init; } = true; // do we multiply final value by stack count ?

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
        ResetDependencies(buff, target);
    }

    private void ResetDependencies(Buff buff, Unit target = null)
    {
        if (ResetDependency != null)
            (target ?? buff.CarrierUnit).ResetCondition(ResetDependency);
        (target ?? buff.CarrierUnit).ResetCondition(Condition.ConditionCheckParam.Buff);
        if (buff.Type is Buff.BuffType.Debuff or Buff.BuffType.Dot)
            (target ?? buff.CarrierUnit).ResetCondition(Condition.ConditionCheckParam.AnyDebuff);
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
        ResetDependencies(buff, target);
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

    public string Explain(double? calculatedValue = null)
    {
        string val = (Value == null) ? "*" : Value.ToString();
        if (Value is null && calculatedValue != null)
            val = "~" + calculatedValue;
        return $"{GetType().Name} ({val}) ";
    }
}