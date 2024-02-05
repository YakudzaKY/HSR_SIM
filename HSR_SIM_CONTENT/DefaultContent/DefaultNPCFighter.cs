using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils.Utils;
using static HSR_SIM_LIB.Fighters.FighterUtils;
using static HSR_SIM_LIB.TurnBasedClasses.CombatFight;

namespace HSR_SIM_LIB.Fighters;

/// <summary>
///     default npc fighter logics
/// </summary>
public class DefaultNPCFighter : IFighter
{
    public DefaultNPCFighter(Unit parent)
    {
        Parent = parent;

        EventHandlerProc += DefaultFighter_HandleEvent;
        StepHandlerProc += DefaultFighter_HandleStep;
    }
    public virtual bool IsEliteUnit => false;
    public bool IsNpcUnit => true;
    public List<ConditionBuff> ConditionBuffs { get; set; } = new();

    public Unit GetBestTarget(Ability ability)
    {
        if (ability.TargetType == Ability.TargetTypeEnm.Self) return Parent;

        if (Parent.ParentTeam.ParentSim.Parent.DevMode)
        {
            return DevModeUtils.GetTarget(this, Parent.GetTargetsForUnit(ability.TargetType), ability);
        }

        if (ability.TargetType == Ability.TargetTypeEnm.Friend)
            //Pick friend in order that have no buff by ability then order by role
            return  GetAliveFriends().OrderByDescending(x=>x.Buffs.All(y=> ability.Events.Count(z =>( z is ApplyBuff ab) &&  ab.BuffToApply==y.Reference)==0 )).ThenBy(x => x.Fighter.Role).First();

        var totalEnemyAggro = Parent.EnemyTeam.TeamAggro;
        var aggroRandomed = new MersenneTwister().NextDouble();
        double counter = 1;
        if (totalEnemyAggro > 0)
        {
            foreach (var unit in GetAliveEnemies())
            {
                counter -= unit.GetAggro(null) / Parent.EnemyTeam.TeamAggro;
                if (counter <= aggroRandomed)
                    return unit;
            }
        }
        else
        {
            //enemies have NPC? not typycal scenario  go random target then
            var enemyTargetList = GetAliveEnemies().ToList();
            foreach (var unit in enemyTargetList)
            {
                // ReSharper disable once PossibleLossOfFraction
                counter -= 1 / enemyTargetList.Count;
                if (counter <= aggroRandomed)
                    return unit;
            }
        }

        throw new Exception($"no enemy will be chosen by AGGRO counter={counter}");
    }

    public List<PassiveBuff> PassiveBuffs { get; set; } = new();
    public Buff ShieldBreakDebuff { get; set; } = new(null);
    public PathType? Path { get; set; } = null;
    public Unit.ElementEnm Element { get; set; }
    public List<Unit.ElementEnm> NativeWeaknesses { get; set; } = new();
    public List<Resist> Resists { get; set; } = new();
    public List<DebuffResist> DebuffResists { get; set; } = new();
    public Unit Parent { get; set; }

    public string GetSpecialText()
    {
        return null;
    }

    public virtual double Cost =>  ((Parent.Stats.BaseAttack * (1 + Parent.Stats.AttackPrc) + Parent.Stats.AttackFix) )/
                                  (Parent.Fighter.Abilities.Count(x => x.TargetType == Ability.TargetTypeEnm.Friend) +
                                   1);

    private UnitRole? role;
    public UnitRole? Role
    {
        get
        {
            if (role == null&&Parent.IsAlive && Parent.ParentTeam != Parent.ParentTeam.ParentSim.SpecialTeam)
            {
                
                    var unitsToSearch = Parent.ParentTeam.Units.Where(x => x.IsAlive)
                        .OrderByDescending(x => x.Fighter.Cost)
                        
                        .ToList();
                    if (Parent == unitsToSearch.First())
                        role = UnitRole.MainDPS;

                    else if (Parent == unitsToSearch.ElementAt(1))
                        role = UnitRole.SecondDPS;
                    else
                        role = UnitRole.Support;
                
                
            }
            return role;
        }
        set => role = value;

    }

    public void Reset()
    {
    }

    public Ability ChoseAbilityToCast(Step step)
    {
        Ability chosenAbility = null;
        Parent.ParentTeam.ParentSim?.Parent.LogDebug("========What i can cast=====");
        var abilities = Abilities
            .Where(x => x.Available()&& x.CooldownTimer == 0 &&
                        x.AbilityType is Ability.AbilityTypeEnm.Basic or Ability.AbilityTypeEnm.Ability);
        chosenAbility = step.Parent.Parent.DevMode ? DevModeUtils.ChooseAbilityToCast(this, abilities) :abilities.MaxBy(x => x.AbilityType);
        Parent.ParentTeam.ParentSim?.Parent.LogDebug($"Choose  {chosenAbility?.Name}");
        return chosenAbility;
    }

    public MechDictionary Mechanics { get; set; }


    public IFighter.EventHandler EventHandlerProc { get; set; }
    public IFighter.StepHandler StepHandlerProc { get; set; }
    public List<Ability> Abilities { get; set; } = new();

    public object Clone()
    {
        return MemberwiseClone();
    }

    public IEnumerable<Unit> GetAliveEnemies()
    {
        return Parent.Enemies.Where(x => x.IsAlive);
    }

    public IEnumerable<Unit> GetAliveFriends()
    {
        return Parent.Friends.Where(x => x.IsAlive);
    }

    public virtual void DefaultFighter_HandleEvent(Event ent)
    {

    }

    public virtual void DefaultFighter_HandleStep(Step step)
    {
    }
}