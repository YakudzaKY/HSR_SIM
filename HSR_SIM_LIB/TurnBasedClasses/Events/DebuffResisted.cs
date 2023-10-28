using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //Notify that debuff got resisted
    public class DebuffResisted:ModEventTemplate
    {
        public DebuffResisted(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"{TargetUnit.Name} debuff resisted: {Modification.Effects.First().EffType}";
            
        }
    }
}
