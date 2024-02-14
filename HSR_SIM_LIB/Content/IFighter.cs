using System;
using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Fighters.FighterUtils;

namespace HSR_SIM_LIB.Fighters;

/// <summary>
///     Fighter logic interface
/// </summary>
public interface IFighter : ICloneable
{
    public delegate void EventHandler(Event ent);

    public delegate void StepHandler(Step step);
    /// <summary>
    /// Buff by character working under certain conditions
    /// </summary>
    public List<ConditionBuff> ConditionBuffs { get; set; }

    /// <summary>
    /// buffs that work passively
    /// </summary>
    public List<PassiveBuff> PassiveBuffs { get; set; }

    /// <summary>
    /// A debuff that is applied when a vulnerability is broken.
    /// </summary>
    public Buff WeaknessBreakDebuff { get; set; }
    public Unit.ElementEnm Element { get; }
    public PathType? Path { get; }
    /// <summary>
    /// native weaknesses defined by profile
    /// </summary>
    public List<Unit.ElementEnm> NativeWeaknesses { get; set; }
    /// <summary>
    /// native resists defined by profile
    /// </summary>
    public List<Resist> Resists { get; set; }
    /// <summary>
    /// native debuff resists defined by profile
    /// </summary>
    public List<DebuffResist> DebuffResists { get; set; }
    public EventHandler EventHandlerProc { get; set; }

    public StepHandler StepHandlerProc { get; set; }

    /// <summary>
    /// ability list
    /// </summary>
    public List<Ability> Abilities { get; set; }
    /// <summary>
    /// reference to Unit. i.e. Character = Unit who have Resources, Figher, buffs etc
    /// </summary>
    public Unit Parent { get; set; }
    /// <summary>
    /// Value to determine unit role in squad
    /// </summary>
    public double Cost { get; } //unit cost in the squad
    /// <summary>
    /// unit role in the battlefield
    /// </summary>
    public UnitRole? Role { get; set; }
    /// <summary>
    /// Get best target for selected ability
    /// </summary>
    /// <param name="ability"></param>
    /// <returns></returns>
    public Unit GetBestTarget(Ability ability);
    /// <summary>
    /// Unique text for this character meaning unique mechanics
    /// </summary>
    /// <returns></returns>
    public string GetSpecialText(); //text for different triggers counters etc
    public void Reset();
    /// <summary>
    /// Chose best and available ability to use in current situation
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    public Ability ChoseAbilityToCast(Step step);
    /// <summary>
    /// Unique mechanics for this character
    /// </summary>
    public MechDictionary Mechanics { get; set; }
    /// <summary>
    /// Is Elite flag. Need for some weakness break calculations
    /// </summary>
    public bool IsEliteUnit { get;  }
    /// <summary>
    /// flag that unit is NPC
    /// </summary>
    public bool IsNpcUnit { get;  }
}