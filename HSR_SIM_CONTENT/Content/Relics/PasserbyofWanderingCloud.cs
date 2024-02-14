﻿using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.Relics;

public class PasserbyofWanderingCloud : DefaultRelicSet
{
    public PasserbyofWanderingCloud(IFighter parent, int num) : base(parent, num)
    {
    }

    public override void DefaultRelicSet_HandleStepBefore(Step step)
    {
        if (Num >= 4)
            if (step.StepType == Step.StepTypeEnm.StartCombat)
            {
                PartyResourceGain newEvent = new(step, this, Parent.Parent)
                {
                    ResType = Resource.ResourceType.SP,
                    Val = 1,
                    TargetUnit = Parent.Parent
                };

                step.Events.Add(newEvent);
            }


        base.DefaultRelicSet_HandleStepBefore(step);
    }
}