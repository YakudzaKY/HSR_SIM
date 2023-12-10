using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones;

internal class PlanetaryRendezvous : DefaultLightCone
{
    private readonly Dictionary<int, double> modifiers = new()
        { { 1, 0.12 }, { 2, 0.15 }, { 3, 0.18 }, { 4, 0.21 }, { 5, 0.24 } };

    public PlanetaryRendezvous(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
            PassiveMods.Add(new PassiveBuff(Parent.Parent)
            {
                AppliedBuff = new Buff(Parent.Parent)
                {
                    Effects = new List<Effect>
                        { new EffElementalBoost { Element = parent.Element, Value = modifiers[rank] } }
                },
                Target = Parent.Parent.ParentTeam
            });
    }

    public sealed override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Harmony;
}