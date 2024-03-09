using System.Collections.Generic;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class Buff(Unit sourceUnit, Buff reference = null) : CloneClass
{
    //calculated stacks will overwrite Stack value
    public delegate int CalculateIntVal(Event ent);

    public enum BuffType
    {
        Buff,
        Debuff,
        Dot
    }

    public string CustomIconName { get; init; }
    public List<Effect> Effects { get; set; } = [];
    public Unit CarrierUnit { get; set; }
    public Buff Reference { get; set; } = reference;
    public Unit SourceUnit { get; init; } = sourceUnit;
    public int Stack { get; set; } = 1;
    public CalculateIntVal CalculateStacks { get; init; }
    public int MaxStack { get; init; } = 1;
    public BuffType Type { get; init; } = BuffType.Buff;

    public override object Clone()
    {
        var newClone = (Buff)MemberwiseClone();
        var oldEff = newClone.Effects;
        newClone.Effects = new List<Effect>();
        //clone calculated effects
        foreach (var eff in oldEff)
            if (eff.DynamicValue)
                newClone.Effects.Add((Effect)eff.Clone());
            else
                newClone.Effects.Add(eff);


        return newClone;
    }

    public string Explain()
    {
        var explainString = $"{GetType().Name} is {Type} {nameof(CarrierUnit)}:{CarrierUnit.PrintName}" +
                            $" {nameof(SourceUnit)}:{SourceUnit.PrintName}";
        if (this is not PassiveBuff passiveBuff) return explainString;
        explainString += $" {nameof(passiveBuff.SourceObject)}={passiveBuff.SourceObject.GetType().Name}" +
                         $" {nameof(passiveBuff.Target)}={passiveBuff.Target.GetType().Name}";
        if (passiveBuff.WorkCondition != null)
        {
            //additional info if condition
            explainString +=
                $" (!) Condition for buff: {nameof(PassiveBuff.IsTargetCheck)}={passiveBuff.IsTargetCheck} ";
            explainString += $" {nameof(passiveBuff.WorkCondition)} =( {passiveBuff.WorkCondition.ConditionParam}" +
                             $" {passiveBuff.WorkCondition.ConditionExpression} {passiveBuff.WorkCondition.Value}" +
                             $" {passiveBuff.WorkCondition.ElemValue} {passiveBuff.WorkCondition.AppliedBuffValue})";
        }

        return explainString;
    }
}