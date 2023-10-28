using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Relics.Set
{
    public class PasserbyofWanderingCloud:DefaultRelicSet
    {
        public override  void DefaultRelicSet_HandleStep(Step step)
        {
            
            if (Num >= 4)
            {
                if (step.StepType==Step.StepTypeEnm.StartCombat)
                {
                    PartyResourceGain newEvent = new (step, this,Parent.Parent)
                    {
                        
                        ResType = Resource.ResourceType.SP
                        ,Val=1
                        ,TargetUnit = Parent.Parent
                    };
           
                    step.Events.Add(newEvent);
                }
            }


            base.DefaultRelicSet_HandleStep(step);
           
        }

        public PasserbyofWanderingCloud(IFighter parent,int num) : base(parent,num)
        {
        }
    }
}
