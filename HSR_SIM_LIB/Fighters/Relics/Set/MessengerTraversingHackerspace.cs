using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Relics.Set
{
    internal class MessengerTraversingHackerspace:DefaultRelicSet
    {
        public override  void DefaultRelicSet_HandleStep(Step step)
        {
            if (Num >= 4)
            {
                if (step.StepType==Step.StepTypeEnm.UnitFollowUpAction &&step.Actor==Parent.Parent)
                {
                    //buf spd 12
                    if (step.ActorAbility.AbilityType == Ability.AbilityTypeEnm.Ultimate)
                    {

                        foreach (Unit unit in step.Actor.GetTargetsForUnit(Ability.TargetTypeEnm.Friend))
                        {
                            ApplyBuff eventBuff = new(null, this,Parent.Parent)
                            {
                                OnStepType = Step.StepTypeEnm.ExecuteAbility,
                                AbilityValue = step.ActorAbility,
                                ParentStep = step,
                                TargetUnit = unit,
                                BuffToApply = (new Buff(Parent.Parent)
                                {
                                    UniqueStr=this.GetType().ToString(),
                                    Type = Buff.ModType.Buff,
                                    Effects = new List<Effect>()
                                        { new EffSpeedPrc() { Value = 0.12 }  },
                                    BaseDuration = 1, Dispellable = true
                                })
                            };
                            step.Events.Add(eventBuff);
                        }
                    }
                }
            }
            base.DefaultRelicSet_HandleStep(step);
        }

        public MessengerTraversingHackerspace(IFighter parent,int num) : base(parent,num)
        {
        }
    }
}
