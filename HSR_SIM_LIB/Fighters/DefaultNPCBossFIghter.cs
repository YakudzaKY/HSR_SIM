using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters
{
    //routine for Elite
    internal class DefaultNPCBossFIghter:DefaultNPCFighter
    {
        public DefaultNPCBossFIghter(Unit parent) : base(parent)
        {
        }

        public override double Cost
        {
            get => Parent.GetAttack(null) * 1.5;
        }
    }
}
