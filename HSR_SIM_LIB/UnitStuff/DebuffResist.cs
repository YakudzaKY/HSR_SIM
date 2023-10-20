using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using static HSR_SIM_LIB.Skills.Effect;

namespace HSR_SIM_LIB.UnitStuff
{
    public class DebuffResist
    {
        public EffectType Debuff { get; init; }
        public double ResistVal { get; init; }
    }
}
