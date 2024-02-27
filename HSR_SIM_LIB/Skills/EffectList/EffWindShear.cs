﻿using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     WindShear debuff
/// </summary>
public class EffWindShear : EffDotTemplate
{
    public override Unit.ElementEnm Element { get; init; } = Unit.ElementEnm.Wind;
}