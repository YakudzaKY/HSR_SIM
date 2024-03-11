using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_CONTENT.Content.Relics;

internal class MessengerTraversingHackerspace : DefaultRelicSet
{
    public MessengerTraversingHackerspace(IFighter parent, int num) : base(parent, num)
    {
    }

    public override void DefaultRelicSet_HandleStep(Step step)
    {
        if (Num >= 4)
            if (step.StepType == Step.StepTypeEnm.UnitUltimate && step.Actor == Parent.Parent)
                //buf spd 12
                if (step.ActorAbility.AbilityType == Ability.AbilityTypeEnm.Ultimate)
                    foreach (var unit in step.Actor.GetTargetsForUnit(Ability.TargetTypeEnm.Friend))
                    {
                        ApplyBuff eventBuff = new(null, this, Parent.Parent)
                        {
                            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
                            ParentStep = step,
                            TargetUnit = unit,
                            AppliedBuffToApply = new AppliedBuff(Parent.Parent,null,this)
                            {
                                UniqueStr = GetType().ToString(),
                                Type = Buff.BuffType.Buff,
                                Effects = new List<Effect> { new EffSpeedPrc { Value = 0.12 } },
                                BaseDuration = 1,
                                Dispellable = true
                            }
                        };
                        step.Events.Add(eventBuff);
                    }

        base.DefaultRelicSet_HandleStep(step);
    }
}