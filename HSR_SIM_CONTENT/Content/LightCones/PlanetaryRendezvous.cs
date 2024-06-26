﻿using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;

namespace HSR_SIM_CONTENT.Content.LightCones;

internal class PlanetaryRendezvous : DefaultLightCone
{
    private readonly Dictionary<int, double> modifiers = new()
        { { 1, 0.12 }, { 2, 0.15 }, { 3, 0.18 }, { 4, 0.21 }, { 5, 0.24 } };

    public PlanetaryRendezvous(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
            Parent.Parent.PassiveBuffs.Add(new PassiveBuff(Parent.Parent, this)
            {
                Effects = [new EffElementalBoost { Element = parent.Element, Value = modifiers[rank] }],

                Target = Parent.Parent.ParentTeam
            });
    }

    public sealed override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Harmony;
}