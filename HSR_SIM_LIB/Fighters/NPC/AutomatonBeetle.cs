using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Effect;

namespace HSR_SIM_LIB.Fighters.NPC
{
    public class AutomatonBeetle:DefaultNPCFighter
    {
        public AutomatonBeetle(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Physical;

            Weaknesses.Add(Unit.ElementEnm.Wind);
            Weaknesses.Add(Unit.ElementEnm.Lightning);
            Weaknesses.Add(Unit.ElementEnm.Imaginary);
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Lightning,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Physical,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Ice,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Quantum,ResistVal=0.20});


        }
    }
}
