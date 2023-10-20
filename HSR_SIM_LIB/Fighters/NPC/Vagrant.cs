using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.NPC
{
    public class Vagrant: DefaultNPCFighter
    {
        public Vagrant(Unit parent) : base(parent)
        {    
            //Elemenet
            Element = Unit.ElementEnm.Physical;

            Weaknesses.Add(Unit.ElementEnm.Fire);
            Weaknesses.Add(Unit.ElementEnm.Ice);
            Weaknesses.Add(Unit.ElementEnm.Imaginary);
            //resist
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Physical,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Lightning,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Wind,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Quantum,ResistVal=0.20});

        }
    }
}
