using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones
{
    internal class PlanetaryRendezvous : DefaultLightCone
    {
        public sealed override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Harmony;
        private readonly Dictionary<int, double> modifiers = new() { { 1, 0.12 }, { 2, 0.15 }, { 3, 0.18 }, { 4, 0.21 }, { 5, 0.24 } };
        public PlanetaryRendezvous(IFighter parent, int rank) : base(parent, rank)
        {
            if (Path == Parent.Path)
                PassiveMods.Add(new PassiveMod(Parent.Parent)
                {
                    Mod = new Buff(Parent.Parent)
                    { Effects = new() { new EffElementalBoost() {  Element = parent.Element, Value = modifiers[rank] } } },
                    Target = Parent.Parent.ParentTeam

                });
        }
    }
}
