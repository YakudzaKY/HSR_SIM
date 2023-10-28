using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //starting the combat
    internal class StartCombat : Event
    {
        public StartCombat(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return "Combat was started";

        }

        public override void ProcEvent(bool revert)
        {
            
            //Loading combat

            if (!revert)
            {
                ParentStep.Parent.DoEnterCombat = false;
                ParentStep.Parent.CurrentFight = new CombatFight(ParentStep.Parent.CurrentScenario.Fights[ParentStep.Parent.CurrentFightStep], ParentStep.Parent);
                ParentStep.Parent.CurrentFightStep += 1;
            }
            else
            {
                ParentStep.Parent.DoEnterCombat = true;
                ParentStep.Parent.CurrentFight = null;
                ParentStep.Parent.CurrentFightStep -= 1;
            }
            base.ProcEvent(revert);
        }
    }
}
