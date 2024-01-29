using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    internal class UnbindUnit:Event

    {
        private int ndx;
        private Team lastTeam;
        public UnbindUnit(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
        {
            
        }

        public override void ProcEvent(bool revert)
        {
            if (!revert)
            {
                lastTeam = TargetUnit.ParentTeam;
                ndx = lastTeam.UnBindUnit(TargetUnit);
            }
            else
                lastTeam.BindUnit(TargetUnit,ndx);

            base.ProcEvent(revert);
        }

        public override string GetDescription()
        {
            return $"unbind {TargetUnit.Name} from team";
        }
    }
}
