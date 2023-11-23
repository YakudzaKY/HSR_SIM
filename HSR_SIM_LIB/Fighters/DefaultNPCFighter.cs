using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils.Utils;
using static HSR_SIM_LIB.Fighters.FighterUtils;

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

    public List<ConditionBuff> ConditionBuffs { get; set; } = new();

    public Unit GetBestTarget(Ability ability)
    {
        if (ability.TargetType == Ability.TargetTypeEnm.Friend)
            return Parent.Friends.Where(x => x.IsAlive).OrderBy(x => x.Fighter.Role).First();

        var totalEnemyAggro = Parent.EnemyTeam.TeamAggro;
        var aggroRandomed = new MersenneTwister().NextDouble();
        double counter = 1;
        if (totalEnemyAggro > 0)
        {
            foreach (var unit in GetAoeTargets())
            {
                counter -= unit.GetAggro(null) / Parent.EnemyTeam.TeamAggro;
                if (counter <= aggroRandomed)
                    return unit;
            }
        }
        else
        {
            //enemies have NPC? not typycal scenario  go random target then
            var enemyTargetList = GetAoeTargets().ToList();
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
    public Buff ShieldBreakMod { get; set; } = new(null);
    public PathType? Path { get; set; } = null;
    public Unit.ElementEnm Element { get; set; }
    public List<Unit.ElementEnm> Weaknesses { get; set; } = new();
    public List<Resist> Resists { get; set; } = new();
    public List<DebuffResist> DebuffResists { get; set; } = new();
    public Unit Parent { get; set; }

    public string GetSpecialText()
    {
        return null;
    }

    public virtual double Cost => Parent.GetAttack(null) /
                                  (Parent.Fighter.Abilities.Count(x => x.TargetType == Ability.TargetTypeEnm.Friend) +
                                   1);


    public UnitRole? Role
    {
        get
        {
            if (!Parent.IsAlive) return null;
            //special units have no role
            if (Parent.ParentTeam == Parent.ParentTeam.ParentSim.SpecialTeam)
                return null;
            var unitsToSearch = Parent.ParentTeam.Units.Where(x => x.IsAlive).OrderByDescending(x => x.Fighter.Cost)
                .ThenByDescending(x => x.GetAttack(null) * x.Stats.BaseCritChance * x.Stats.BaseCritDmg).ToList();
            if (Parent == unitsToSearch.First())
                return UnitRole.MainDPS;

            if (Parent == unitsToSearch.ElementAt(1))
                return UnitRole.SecondDPS;
            return UnitRole.Support;
        }
    }

    public void Reset()
    {
    }

    public Ability ChoseAbilityToCast(Step step)
    {
        Ability chosenAbility = null;
        Parent.ParentTeam.ParentSim?.Parent.LogDebug("========What i can cast=====");
        //TODO :cooldown
        chosenAbility = Abilities
            .Where(x => x.Available.Invoke() && x.CooldownTimer == 0 &&
                        x.AbilityType is Ability.AbilityTypeEnm.Basic or Ability.AbilityTypeEnm.Ability)
            .MaxBy(x => x.AbilityType);
        Parent.ParentTeam.ParentSim?.Parent.LogDebug($"Choose  {chosenAbility?.Name}");
        return chosenAbility;
    }


    public IFighter.EventHandler EventHandlerProc { get; set; }
    public IFighter.StepHandler StepHandlerProc { get; set; }
    public List<Ability> Abilities { get; set; } = new();

    public object Clone()
    {
        return MemberwiseClone();
    }

    public IEnumerable<Unit> GetAoeTargets()
    {
        return Parent.Enemies.Where(x => x.IsAlive);
    }

    public IEnumerable<Unit> GetAoeFriends()
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