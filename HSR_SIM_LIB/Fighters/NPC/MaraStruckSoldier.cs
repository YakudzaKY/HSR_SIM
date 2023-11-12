using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.NPC;

public class MaraStruckSoldier : DefaultNPCFighter
{
    public MaraStruckSoldier(Unit parent) : base(parent)
    {
        //Elemenet
        Element = Unit.ElementEnm.Wind;

        Weaknesses.Add(Unit.ElementEnm.Fire);
        Weaknesses.Add(Unit.ElementEnm.Ice);
        Weaknesses.Add(Unit.ElementEnm.Quantum);
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Physical, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Lightning, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Wind, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Imaginary, ResistVal = 0.20 });
    }
}