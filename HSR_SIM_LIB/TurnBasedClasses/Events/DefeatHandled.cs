using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    internal class DefeatHandled : Event
    {
        public List<Buff> RemovedMods { get; set; } = new();
        public DefeatHandled(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"{TargetUnit.Name} avoid the defeat";
        }

        public override void ProcEvent(bool revert)
        {

            if (!TriggersHandled)
            {

                RemovedMods.AddRange(TargetUnit.Buffs.Where(x => x.Dispellable));
                ChildEvents.Add(new SetLiveStatus(ParentStep, SourceUnit, SourceUnit)
                { ToState = Unit.LivingStatusEnm.WaitingForFollowUp, TargetUnit = TargetUnit });

            }

            if (!revert)
            {

                foreach (var mod in RemovedMods)
                    TargetUnit.RemoveBuff(this, mod);
            }
            else
            {
                List<Buff> rmods = new();
                rmods.AddRange(RemovedMods);
                rmods.Reverse();
                foreach (var mod in rmods)
                    TargetUnit.RestoreBuff(this, mod);
            }


            base.ProcEvent(revert);
        }
    }
}
