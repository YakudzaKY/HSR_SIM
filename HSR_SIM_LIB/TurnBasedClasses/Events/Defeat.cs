using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    // unit defeated( usefull for geppard etc)
    public class Defeat : Event
    {
        public List<Buff> RemovedMods { get; set; } = new List<Buff>();
        public Defeat(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return TargetUnit.Name + " get rekt (:";
        }

        public override void ProcEvent(bool revert)
        {
            //attacker got 10 energy
            if( !TriggersHandled)
                this.ChildEvents.Add(new EnergyGain(this.ParentStep, null, AbilityValue.Parent.Parent) { Val = 10 , TargetUnit = AbilityValue.Parent.Parent, AbilityValue = AbilityValue});
            //got defeated

            TargetUnit.IsAlive = revert;

            if (!revert)
                foreach (Buff mod in RemovedMods)
                {
                    TargetUnit.RemoveBuff(this,mod);
                }
            else
                foreach (Buff mod in RemovedMods)
                {
                    TargetUnit.ApplyBuff(this, mod);
                }

            base.ProcEvent(revert);

        }
    }
}
