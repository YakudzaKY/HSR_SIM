using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.NPC
{
    internal class AutomatonGrizzlyComplete:DefaultNPCBossFIghter
    {
        public AutomatonGrizzlyComplete(Unit parent) : base(parent)
        {  //Elemenet
            Element = Unit.ElementEnm.Physical;

            Weaknesses.Add(Unit.ElementEnm.Fire);
            Weaknesses.Add(Unit.ElementEnm.Lightning);
            Weaknesses.Add(Unit.ElementEnm.Ice);
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Physical,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Wind,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Quantum,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Imaginary,ResistVal=0.20});
            DebuffResists.Add(new DebuffResist(){Debuff =Effect.EffectType.Freeze,ResistVal = 0.5});
            DebuffResists.Add(new DebuffResist(){Debuff =Effect.EffectType.Imprisonment,ResistVal = 0.5});
            DebuffResists.Add(new DebuffResist(){Debuff =Effect.EffectType.Entanglement,ResistVal = 0.5});
        }
    }
}
