using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public class ToughnessShred : Event
{

    public List<Condition> DefaultToughnessCondition()
    {
        return
            [
                new Condition()
                {
                    ConditionParam = Condition.ConditionCheckParam.Weakness,
                    ConditionExpression = Condition.ConditionCheckExpression.Exists,
                    ElemValue =null
                },
                new Condition()
                {
                    ConditionParam = Condition.ConditionCheckParam.Resource,
                    ConditionExpression = Condition.ConditionCheckExpression.More,
                    Value = 0,
                    ResourceValue = Resource.ResourceType.Toughness
                },
            ]
            ;
    }
    
    public ToughnessShred(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
        ApplyConditions = DefaultToughnessCondition();
    }



    public override void ProcEvent(bool revert)
    {
        if (!TriggersHandled)
            if ((TargetUnit.AppliedBuffs.All(x => x.Effects.All(y => y is not EffBarrier)) &&
                 TargetUnit.GetWeaknesses(this).Any(x => x == ParentStep.ActorAbility.Element)) ||
                ParentStep.ActorAbility.IgnoreWeakness)
                ChildEvents.Add(new ResourceDrain(null, null, ParentStep.Actor)
                {
                    ParentStep = ParentStep,
                    TargetUnit = TargetUnit,
                    ResType = Resource.ResourceType.Toughness,
                    Value = Value
                });
        base.ProcEvent(revert);
    }

    public override string GetDescription()
    {
        return $"shred {TargetUnit.Name:s} toughness by {Value:f}";
    }
}