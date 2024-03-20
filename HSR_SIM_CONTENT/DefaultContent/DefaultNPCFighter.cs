using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils.Utils;
using static HSR_SIM_LIB.Content.FighterUtils;

namespace HSR_SIM_LIB.Fighters;

/// <summary>
///     default npc fighter logics
/// </summary>
public class DefaultNPCFighter : IFighter
{
    private UnitRole? role;

    protected DefaultNPCFighter(Unit parent)
    {
        Parent = parent;
        Parent.IsNpcUnit = true;

        EventHandlerProc += DefaultFighter_HandleEvent;
        StepHandlerProc += DefaultFighter_HandleStep;
    }

    

    public Unit? GetBestTarget(Ability ability)
    {
        if (ability.TargetType == Ability.TargetTypeEnm.Self) return Parent;

        if (Parent.ParentTeam.ParentSim.Parent.DevMode)
            return DevModeUtils.GetTarget(this, Parent.GetTargetsForUnit(ability.TargetType), ability);

        if (ability.TargetType == Ability.TargetTypeEnm.Friend)
            //Pick friend in order that have no buff by ability then order by role
            return GetAliveFriends().OrderByDescending(x => x.AppliedBuffs.All(y =>
                    ability.Events.Count(z => z is ApplyBuff ab && ab.AppliedBuffToApply == y.Reference) == 0))
                .ThenBy(x => x.Fighter.Role).First();

        var totalEnemyAggro = Parent.EnemyTeam.TeamAggro;
        var aggroRandomed = new MersenneTwister().NextDouble();
        double counter = 1;
        if (totalEnemyAggro > 0)
        {
            foreach (var unit in GetAliveEnemies())
            {
                counter -= unit.GetAggro(ent:null).Result / Parent.EnemyTeam.TeamAggro;
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

    public Ability.ElementEnm Element { get; set; }
    public AppliedBuff WeaknessBreakDebuff { get; set; } = new(null,null,typeof(DefaultNPCFighter)) { Effects = [] };
    public PathType? Path { get; set; } = null;


    public Unit Parent { get; set; }

    public string GetSpecialText()
    {
        return string.Empty;
    }

    public virtual double Cost => (Parent.Stats.BaseAttack * (1 + Parent.Stats.AttackPrc) + Parent.Stats.AttackFix) /
                                  (Parent.Fighter.Abilities.Count(x => x.TargetType == Ability.TargetTypeEnm.Friend) +
                                   1);

    public UnitRole? Role
    {
        get
        {
            if (role == null && Parent.IsAlive && Parent.ParentTeam != Parent.ParentTeam.ParentSim.SpecialTeam)
            {
                var unitsToSearch = Parent.ParentTeam.Units.Where(x => x.IsAlive)
                    .OrderByDescending(x => x.Fighter.Cost)
                    .ToList();
                if (Parent == unitsToSearch.First())
                    role = UnitRole.MainDps;

                else if (Parent == unitsToSearch.ElementAt(1))
                    role = UnitRole.SecondDps;
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

    public Ability? ChoseAbilityToCast(Step step)
    {
        Ability? chosenAbility = null;
        var abilities = Abilities
            .Where(x => x.Available() && x.CooldownTimer == 0 &&
                        x.AbilityType is Ability.AbilityTypeEnm.Basic or Ability.AbilityTypeEnm.Ability);
        chosenAbility = step.Parent.Parent.DevMode
            ? DevModeUtils.ChooseAbilityToCast(this, abilities)
            : abilities.MaxBy(x => x.AbilityType);
        return chosenAbility;
    }

    public MechDictionary Mechanics { get; set; } = new MechDictionary();


    public IFighter.EventHandler EventHandlerProc { get; set; }
    public IFighter.StepHandler StepHandlerProc { get; set; }
    public List<Ability?> Abilities { get; set; } = new();

    public object Clone()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Unit?> GetAliveEnemies()
    {
        return Parent.Enemies.Where(x => x.IsAlive);
    }

    public IEnumerable<Unit?> GetAliveFriends()
    {
        return Parent.Friends.Where(x => x.IsAlive);
    }

    protected virtual void DefaultFighter_HandleEvent(Event ent)
    {
    }

    protected virtual void DefaultFighter_HandleStep(Step step)
    {
    }
}