using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList
{
    public class EffElementalTemplate:Effect
    {
        public Unit.ElementEnm Element { get; init; }
    }
}
