using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones
{
    internal class PlanetaryRendezvous:DefaultLightCone
    {
        private readonly Dictionary<int, double> modifiers = new Dictionary<int, double>() { { 1, 0.12 }, { 2, 0.15 }, { 3, 0.18 }, { 4, 0.21 }, { 5, 0.24 } };
        public PlanetaryRendezvous(IFighter parent, int rank) : base(parent, rank)
        {
            PassiveMods.Add(new PassiveMod(Parent.Parent)
            {
                Mod = new Mod()
                { Effects =  new () { new(){EffType = Effect.EffectType.ElementalBoost, Element = parent.Element  , Value = modifiers[rank]}  } },
                Target = Parent.Parent.ParentTeam
                   
            });
        }
    }
}
