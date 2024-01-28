using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Fighters.FighterUtils;

namespace HSR_SIM_LIB.Fighters;

/// <summary>
///     fighting logic interface
/// </summary>
public interface IFighter : ICloneable
{
    public delegate void EventHandler(Event ent);

    public delegate void StepHandler(Step step);

    public List<ConditionBuff> ConditionBuffs { get; set; }

    public List<PassiveBuff> PassiveBuffs { get; set; }

    //need of unique effect on shield break
    public Buff ShieldBreakDebuff { get; set; }
    public Unit.ElementEnm Element { get; }
    public PathType? Path { get; }
    public List<Unit.ElementEnm> NativeWeaknesses { get; set; }
    public List<Resist> Resists { get; set; }
    public List<DebuffResist> DebuffResists { get; set; }
    public EventHandler EventHandlerProc { get; set; }

    public StepHandler StepHandlerProc { get; set; }

    //ability list
    public List<Ability> Abilities { get; set; }
    public Unit Parent { get; set; }
    public double Cost { get; } //unit cost in the squad
    public UnitRole? Role { get; set; }
    public Unit GetBestTarget(Ability ability);
    public string GetSpecialText(); //text for different triggers counters etc
    public void Reset();
    public Ability ChoseAbilityToCast(Step step);
}