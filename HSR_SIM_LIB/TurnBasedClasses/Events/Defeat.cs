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
        public List<Mod> RemovedMods { get; set; } = new List<Mod>();
        public Defeat(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return TargetUnit.Name + " get rekt (:";
        }

        public override void ProcEvent(bool revert)
        {
            
            //got defeated

            TargetUnit.IsAlive = revert;

            if (!revert)
                foreach (Mod mod in RemovedMods)
                {
                    TargetUnit.RemoveMod(mod);
                }
            else
                foreach (Mod mod in RemovedMods)
                {
                    TargetUnit.ApplyMod(mod);
                }

            base.ProcEvent(revert);

        }
    }
}
