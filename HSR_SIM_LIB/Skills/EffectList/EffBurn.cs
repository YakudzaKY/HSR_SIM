﻿using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;
/// <summary>
/// burn DoT
/// </summary>
public class EffBurn : EffDotTemplate
{
    public override Unit.ElementEnm Element { get; init; } = Unit.ElementEnm.Fire;
}