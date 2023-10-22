using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
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
                            Event eventBuff = new(null, this)
                            {
                                OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.Mod,
                                AbilityValue = step.ActorAbility,
                                ParentStep = step,
                                TargetUnit = unit,
                                Modification = (new Mod()
                                {
                                    UniqueStr=this.GetType().ToString(),
                                    Type = Mod.ModType.Buff,
                                    Effects = new List<Effect>()
                                        { new Effect() { EffType = Effect.EffectType.SpeedPrc, Value = 0.12 }  },
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
