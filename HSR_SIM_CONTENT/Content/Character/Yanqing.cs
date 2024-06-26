﻿using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.Character;

public class Yanqing : DefaultFighter
{
    public Yanqing(Unit parent) : base(parent)
    {
    }

    public override double MaxEnergy { get; } = 0;
    public override Ability.ElementEnm Element { get; } = Ability.ElementEnm.Ice;
    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Hunt;
}