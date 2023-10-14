using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Physical,ResistVal=20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Lightning,ResistVal=20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Wind,ResistVal=20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Quantum,ResistVal=20});

        }
    }
}
