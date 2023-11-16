using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Relics.Set;

internal class LongevousDisciple : DefaultRelicSet
{
    private readonly Buff uniqueBuff;

    public LongevousDisciple(IFighter parent, int num) : base(parent, num)
    {
        uniqueBuff = new Buff(Parent.Parent)
        {
            Type = Buff.ModType.Buff, BaseDuration = 2, MaxStack = 2,
            Effects = new List<Effect> { new EffCritPrc { Value = 0.08 } }
        };
    }

    private Step lastDamageStep = null;
    public override void DefaultRelicSet_HandleEvent(Event ent)
    {
        //if friend unit consume our hp or got attack then apply buff
        if (Num >= 4)
            if ((ent is ResourceDrain
                 && ent.AbilityValue?.Parent.Parent.ParentTeam == Parent.Parent.ParentTeam
                 && ent.TargetUnit == Parent.Parent
                 && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal != 0)
                || (ent.TargetUnit == Parent.Parent && ent is DirectDamage && ent.ParentStep!=lastDamageStep))
            {
                //only one proc per action 
                if (ent is DirectDamage)
                    lastDamageStep = ent.ParentStep;
                ApplyBuff newEvent = new(ent.ParentStep, this, Parent.Parent)
                {
                    TargetUnit = Parent.Parent,
                    BuffToApply = uniqueBuff
                };
                ent.ChildEvents.Add(newEvent);
            }


        base.DefaultRelicSet_HandleEvent(ent);
    }

    public override void DefaultRelicSet_HandleStep(Step step)
    {
        base.DefaultRelicSet_HandleStep(step);
    }
}