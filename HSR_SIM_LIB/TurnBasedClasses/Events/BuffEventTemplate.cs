using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    public  abstract class BuffEventTemplate: Event
    {
        public Buff BuffToApply { get; set; }
        protected BuffEventTemplate(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }
    }
}
