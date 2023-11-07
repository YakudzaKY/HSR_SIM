using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    // delete skill from queue(when techniqe skill executed in battle)
    public class CombatStartSkillDeQueue:Event
    {
        public CombatStartSkillDeQueue(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return "Remove ability from start skill queue";
        }

        public override void ProcEvent(bool revert)
        {
            
            //DEQUEUE party buffs or opening
            if (!revert)
                Parent.Parent.BeforeStartQueue.Remove(AbilityValue);
            else
                Parent.Parent.BeforeStartQueue.Add(AbilityValue);
            base.ProcEvent(revert);
        }
    }
}
