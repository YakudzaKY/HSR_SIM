﻿using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

public class EffShock : EffDotTemplate
{
    public override Unit.ElementEnm Element { get; init; } = Unit.ElementEnm.Lightning;
}