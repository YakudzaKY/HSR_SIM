﻿namespace HSR_SIM_LIB.Skills.EffectList;

/// <summary>
///     shield buff
/// </summary>
public class EffShield : Effect
{
    /// <summary>
    ///     shield is always dynamic coz damage reduce the value
    /// </summary>
    public override bool DynamicValue => true;
}