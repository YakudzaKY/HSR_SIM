using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    // dispell buff or dot
    public class RemoveMod : ModEventTemplate
    {
        public RemoveMod(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"Remove modifications on {TargetUnit.Name}. Source: {Source?.GetType()?.ToString().Split(".").Last():s}";
        }

        public override void ProcEvent(bool revert)
        {
           
            //remove mod


            if (!revert)
                TargetUnit.RemoveMod(Modification);
            else
                TargetUnit.ApplyMod(Modification);

            base.ProcEvent(revert);
        }
    }
}
