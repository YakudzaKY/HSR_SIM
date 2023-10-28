using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    // Unit made the attack. Good for triggers
    public class Attack:Event
    {
        public Attack(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
         return   AbilityValue.Parent.Parent.Name + " doing Attack";
        }
    }
}
