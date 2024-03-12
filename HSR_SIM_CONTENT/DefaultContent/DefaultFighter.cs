using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.ReadyBuffs;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Content.FighterUtils;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.Content.IFighter;

namespace HSR_SIM_CONTENT.DefaultContent;

/// <summary>
///     abstract class for Playable characters
/// </summary>
public abstract class DefaultFighter : IFighter
{
    private ILightCone? lightCone;

    private List<IRelicSet>? relics;

    private UnitRole? role;
    private AppliedBuff? weaknessBreakDebuff;


    //Blade constructor
    protected DefaultFighter(Unit parent)
    {
        Mechanics = new MechDictionary();
        Mechanics.Reset();
        Parent = parent;
        EventHandlerProc += DefaultFighter_HandleEvent;
        StepHandlerProc += DefaultFighter_HandleStep;
        //no way to get ascend traces from api :/
        ATraces = ATracesEnm.A2 | ATracesEnm.A4 | ATracesEnm.A6;

        var defOpener =
            //Typical left click opener
            new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Technique,
                Name = "Default opener",
                AdjacentTargets = Ability.AdjacentTargetsEnm.All
            };
        defOpener.Events.Add(new ToughnessShred(null, this, Parent)
            { OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue, Value = 30 });
        Abilities.Add(defOpener);
    }

    private List<IRelicSet> Relics
    {
        get
        {
            if (relics != null) return relics;
            relics = new List<IRelicSet>();
            foreach (var relicSet in Parent.RelicsClasses.Select(keyValRelic =>
                         (IRelicSet)Activator.CreateInstance(Type.GetType(keyValRelic.Key)!, this, keyValRelic.Value)!))
                relics.Add(relicSet);

            return relics;
        }
    }


    protected ATracesEnm ATraces { get; set; }

    public ILightCone? LightCone
    {
        get
        {
            if (!string.IsNullOrEmpty(Parent.LightConeStringPath))
                lightCone ??=
                    (ILightCone)Activator.CreateInstance(Type.GetType(Parent.LightConeStringPath)!, this,
                        Parent.LightConeInitRank)!;
            return lightCone;
        }

        set => lightCone = value;
    }

    public MechDictionary Mechanics { get; set; } // dictionary for save mechanics value in combat
    public bool IsEliteUnit => false;
    public bool IsNpcUnit => false;

    //we need this debuff to track and correctly apply debuff stacks
    public AppliedBuff WeaknessBreakDebuff
    {
        get
        {
            return weaknessBreakDebuff ??= Parent.Element switch
            {
                Unit.ElementEnm.Physical => new AppliedBuffBleedWb(Parent),
                Unit.ElementEnm.Fire => new AppliedBuffBurnWb(Parent),

                Unit.ElementEnm.Ice => new AppliedBuffFreezeWb(Parent),

                Unit.ElementEnm.Lightning => new AppliedBuffShockWb(Parent),

                Unit.ElementEnm.Wind => new AppliedBuffWindShearWb(Parent) { CalculateStacks = CalcStacksByTarget },
                Unit.ElementEnm.Quantum => new AppliedBuffEntanglementWb(Parent),
                Unit.ElementEnm.Imaginary => new AppliedBuffImprisonmentWb(Parent),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        set => weaknessBreakDebuff = value;
    }


    public abstract PathType? Path { get; }

    public List<Unit.ElementEnm> NativeWeaknesses { get; set; } = [];
    public List<DebuffResist> DebuffResists { get; set; } = [];
    public List<Resist> Resists { get; set; } = [];
    public Unit Parent { get; set; }

    /// <summary>
    ///     return Unit cost to determine unit role on the battlefield
    /// </summary>
    public double Cost
    {
        get
        {
            var totalCost = Path switch
            {
                PathType.Hunt => 7 * (GetAliveEnemies().Count() == 1 ? 2 : 1) //x2 if 1 target on battlefield
                ,
                PathType.Destruction =>
                    6 * (GetAliveEnemies().Count() >= 2 ? 2 : 1) //x2 if 2+ targets on battlefield
                ,
                PathType.Erudition => 5 * (GetAliveEnemies().Count() > 3 ? 3 : 1) //x3 if 3+ targets on battlefield
                ,
                PathType.Nihility => 4,
                PathType.Harmony => 3,
                PathType.Preservation => 2,
                PathType.Abundance => 1,
                _ => 1
            };
            return totalCost;
        }
    }

    public UnitRole? Role
    {
        get
        {
            if (role == null && Parent.IsAlive)
            {
                var unitsToSearch = Parent.ParentTeam.Units.Where(x => x.IsAlive)
                    .OrderByDescending(x => x.Fighter.Cost)
                    .ThenByDescending(x =>
                        (x.Stats.BaseAttack * (1 + x.Stats.AttackPrc) + x.Stats.AttackFix) * x.Stats.BaseCritChance *
                        x.Stats.BaseCritDmg).ToList();
                if (Parent == unitsToSearch.First())
                    role = UnitRole.MainDps;
                //if second on list then second dps
                else if (new List<PathType?>
                                 { PathType.Hunt, PathType.Destruction, PathType.Erudition, PathType.Nihility }
                             .Contains(Path) && Parent == unitsToSearch.ElementAt(1))
                    role = UnitRole.SecondDps;
                else if (new List<PathType?>
                             { PathType.Hunt, PathType.Destruction, PathType.Erudition }.Contains(Path) &&
                         Parent == unitsToSearch.ElementAt(2))
                    role = UnitRole.ThirdDps;
                //healer here
                else if (Path == PathType.Abundance)
                    role = UnitRole.Healer;
                else
                    role = UnitRole.Support;
            }

            return role;
        }
        set => role = value;
    }

    //Try to choose some good ability to cast
    public virtual Ability? ChoseAbilityToCast(Step step)
    {
        //Technique before fight
        if (step.Parent.CurrentFight == null)
        {
            var abilities = Abilities
                .Where(x => x.Available() && x.IWannaUseIt() && x.AbilityType == Ability.AbilityTypeEnm.Technique)
                .OrderBy(x => x.Attack)
                .ThenByDescending(x => x.Cost);
            if (step.Parent.Parent.DevMode)
                return DevModeUtils.ChooseAbilityToCast(this, abilities, true);

            //sort by combat then cost. available for casting by cost
            foreach (var ability in abilities)
                //enter combat skills
                if (ability.Attack)
                {
                    /*
                     *Cast ability if ability not in queue
                     * and we can penetrate weakness
                     * or others cant do that.
                     * So basic opener through weakness>>  combat technique that not penetrate
                     */
                    if (Parent.ParentTeam.ParentSim.BeforeStartQueue.IndexOf(ability) ==
                        -1 //check for existing in queue 
                        && (GetWeaknessTargets().Any() || ability.IgnoreWeakness //We can penetrate shield 
                                                       || !GetAliveFriends().Any(x => x != Parent
                                                           && ((((DefaultFighter)x.Fighter).GetWeaknessTargets().Any()
                                                                && x.Fighter.Abilities.Any(y =>
                                                                    y.AbilityType == Ability.AbilityTypeEnm.Technique
                                                                    && y.Attack)) || x.Fighter.Abilities.Any(y =>
                                                               y.AbilityType == Ability.AbilityTypeEnm.Technique
                                                               && y is { Attack: true, IgnoreWeakness: true }))
                                                       ) //or others cant penetrate  or others can ignore weakness
                        )
                        && !(Parent.ParentTeam.GetRes(ResourceType.TP).ResVal >= ability.Cost + 1
                             && GetAliveFriends().Any(x => x.Fighter.Abilities.Any(y =>
                                 y.AbilityType == Ability.AbilityTypeEnm.Technique && !y.Attack &&
                                 x.ParentTeam.ParentSim.BeforeStartQueue.IndexOf(y) <
                                 0))) // no unused buffers here when 2tp+
                       )
                        return ability;
                }
                else
                {
                    //if no skill in queue
                    if (Parent.ParentTeam.ParentSim.BeforeStartQueue.All(x => x != ability))
                        /*if have 2+ tp or
                         we have NOT friend who can penetrate weakness through  cost=1 ability
                        */
                        if (Parent.ParentTeam.GetRes(ResourceType.TP).ResVal >= ability.Cost + 1
                            || !GetAliveFriends().Any(x => GetWeaknessTargets().Any() && x.Fighter.Abilities.Any(y =>
                                y.AbilityType == Ability.AbilityTypeEnm.Technique && y is { Cost: > 0, Attack: true }))
                           )
                            return ability;
                }
        }
        else
        {
            //if dev mode then give All available sp else get from function
            var freeSp = step.Parent.Parent.DevMode
                ? Parent.ParentTeam.GetRes(ResourceType.SP).ResVal
                : HowManySpICanSpend();
            var abilities = Abilities
                .Where(x => x.Available() && x.IWannaUseIt() && (x.Cost <= freeSp || x.CostType != ResourceType.SP) &&
                            (x.AbilityType == Ability.AbilityTypeEnm.Basic ||
                             x.AbilityType == Ability.AbilityTypeEnm.Ability));
            var chosenAbility = step.Parent.Parent.DevMode
                ? DevModeUtils.ChooseAbilityToCast(this, abilities)
                : abilities.MaxBy(x => x.AbilityType);
            return chosenAbility;
        }

        return null;
    }

    public virtual string GetSpecialText()
    {
        return "";
    }


    public IFighter.EventHandler EventHandlerProc { get; set; }
    public StepHandler StepHandlerProc { get; set; }
    public List<Ability> Abilities { get; set; } = [];

    public virtual void Reset()
    {
        Mechanics.Reset();
        LightCone?.Reset();
        foreach (var relic in Relics) relic.Reset();
    }


    public virtual Unit? GetBestTarget(Ability ability)
    {
        var leader = Parent.ParentTeam.Units.FirstOrDefault(x => x.Fighter.Role == UnitRole.MainDps);
        if (ability.TargetType == Ability.TargetTypeEnm.Self) return Parent;
        if (ability.AdjacentTargets == Ability.AdjacentTargetsEnm.All) return null;

        if (ability.TargetType == Ability.TargetTypeEnm.Enemy)
        {
            if (Parent.ParentTeam.ParentSim.Parent.DevMode)
                return DevModeUtils.GetTarget(this, Parent.GetTargetsForUnit(ability.TargetType), ability);

            /*
             * MainDD,SecondDD focus on massive dmg, pref max weakness targets
             * Others- shield breaking
             *
             */
            if (ability.AdjacentTargets == Ability.AdjacentTargetsEnm.None)
            {
                //Support,Healer focus on Shield shred. 
                if (Role is UnitRole.Support or UnitRole.Healer or UnitRole.SecondDps)
                    return Parent.GetTargetsForUnit(ability.TargetType)
                        .OrderByDescending(x => x.GetWeaknesses(null).Contains(Parent.Element))
                        .ThenBy(x =>
                            x.GetRes(ResourceType.Toughness).ResVal *
                            (leader?.Fighter.Path is PathType.Destruction or PathType.Erudition ? -1 : 1)).ThenBy(
                            x =>
                                x.GetRes(ResourceType.HP).ResVal *
                                (leader?.Fighter.Path is PathType.Destruction or PathType.Erudition ? -1 : 1))
                        .FirstOrDefault();

                // focus on High hp if main dps Destruction,Erudition. Other- low hp
                return Parent.GetTargetsForUnit(ability.TargetType)
                    .OrderByDescending(x => x.GetWeaknesses(null).Contains(Parent.Element)).ThenBy(x =>
                        x.GetRes(ResourceType.HP).ResVal *
                        (leader?.Fighter.Path is PathType.Destruction or PathType.Erudition ? -1 : 1))
                    .FirstOrDefault();
            }

            if (ability.AdjacentTargets == Ability.AdjacentTargetsEnm.Blast)
            {
                Unit? bestTarget = null;
                double bestScore = -1;
                foreach (var unit in Parent.GetTargetsForUnit(ability.TargetType))
                {
                    var targets = ability.GetAffectedTargets(unit);
                    double score = 10 * targets.Count;
                    score += 5 * targets.Count(x => x == unit && x.GetRes(ResourceType.Toughness).ResVal == 0);
                    score += 3 * targets.Count(x => x == unit && x.GetWeaknesses(null).Any(z => z == Parent.Element));
                    score += 2 * targets.Count(x => x.Fighter is DefaultNPCBossFIghter);
                    //if equal but hp diff go focus big target
                    if (score > bestScore ||
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        (score == bestScore && unit.GetHpPrc(null) > bestTarget?.GetHpPrc(null)))
                    {
                        bestTarget = unit;
                        bestScore = score;
                    }
                }

                return bestTarget;
            }


            throw new NotImplementedException();
        }

        if (ability.TargetType == Ability.TargetTypeEnm.Friend)
            return Parent.ParentTeam.ParentSim.Parent.DevMode
                ? DevModeUtils.GetTarget(this, GetAliveFriends(), ability)
                : GetAliveFriends().OrderBy(x => x.Fighter.Role).First();
        throw new NotImplementedException();

        // return null;
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

    private int CalcStacksByTarget(Event ent)
    {
        return ent.TargetUnit.Fighter.IsEliteUnit ? 3 : 1;
    }


    //all alive enemies
    protected IEnumerable<Unit> GetAliveEnemies()
    {
        return Parent.Enemies.Where(x => x.LivingStatus == Unit.LivingStatusEnm.Alive);
    }

    //enemies with weakness to ability and without shield
    private IEnumerable<Unit> GetWeaknessTargets()
    {
        return Parent.Enemies.Where(x => x.IsAlive
                                         && x.GetWeaknesses(null).Any(y => y == Parent.Element));
    }

    //alive friends. select only who not defeated or waiting for rez
    public IEnumerable<Unit> GetAliveFriends()
    {
        return Parent.Friends.Where(x => x.LivingStatus == Unit.LivingStatusEnm.Alive);
    }


    /// <summary>
    ///     I will spend in next turn
    /// </summary>
    /// <returns></returns>
    protected virtual double WillSpend()
    {
        return 1;
    }

    protected DefaultFighter? GetFriendByRole(UnitRole unitRole)
    {
        var res = GetAliveFriends().FirstOrDefault(x => x.Fighter.Role == unitRole)?.Fighter;
        if (res != null)
            return (DefaultFighter)res;
        return null;
    }

    protected double GetFriendSpender(UnitRole unitRole)
    {
        var defaultFighter = GetFriendByRole(unitRole);
        if (defaultFighter != null)
            return defaultFighter.WillSpend();
        return 0;
    }

    /// <summary>
    ///     How many SP Fighter can spend on cast
    /// </summary>
    /// <returns></returns>
    private double HowManySpICanSpend()
    {
        var totalRes = Parent.ParentTeam.GetRes(ResourceType.SP).ResVal;
        var res = totalRes;
        double reservedSp = 0;
        var myReserve = HowManySpIReserve();
        //get friends reserved SP
        foreach (var friend in GetAliveFriends().Where(x => x != Parent))
            reservedSp += ((DefaultFighter)friend.Fighter).HowManySpIReserve();

        if (Role is UnitRole.MainDps)
        {
            //Cut Free  res to total-reserve
            res = Math.Min(res, totalRes - reservedSp);
        }
        else if (Role is UnitRole.Support)
        {
            var addSpenders = GetFriendSpender(UnitRole.MainDps);
            res -= addSpenders;
            //Cut Free  res to total-reserve
            res = Math.Min(res, totalRes - reservedSp);
        }
        else if (Role is UnitRole.SecondDps or UnitRole.ThirdDps)
        {
            var addSpenders = GetFriendSpender(UnitRole.MainDps) + GetFriendSpender(UnitRole.Support);
            res -= addSpenders;
            //Cut Free  res to total-reserve
            res = Math.Min(res, totalRes - reservedSp);
        }
        else if (Role is UnitRole.Healer)
        {
            res = myReserve;
        }


        //retake SP if not enough by role but have reserved
        if (res < myReserve) res = Math.Min(myReserve, totalRes);


        return Math.Max(res, 0);
    }

    /// <summary>
    ///     How many SP i reserve for next turn VERY IMPORTANT n1 ABILITY!
    /// </summary>
    /// <returns></returns>
    protected virtual double HowManySpIReserve()
    {
        if (Role == UnitRole.Healer)
            //if hp <=50% or hp<=70% and <=2500(at 80 lvl)
            if (GetAliveFriends().Any(x => x.GetHpPrc(null) <= 0.5 ||
                                           (x.GetHpPrc(null) <= 0.7 &&
                                            x.GetRes(ResourceType.HP).ResVal <= x.Level * 31.25)))
                return 1;
        return 0;
    }

    protected virtual void DefaultFighter_HandleEvent(Event ent)
    {
        LightCone?.EventHandlerProc?.Invoke(ent);
        foreach (var relic in Relics) relic.EventHandlerProc?.Invoke(ent);
    }

    protected virtual void DefaultFighter_HandleStep(Step step)
    {
        LightCone?.StepHandlerProc?.Invoke(step);
        foreach (var relic in Relics) relic.StepHandlerProc?.Invoke(step);
    }

    [Flags]
    protected enum ATracesEnm
    {
        A2 = 1,
        A4 = 2,
        A6 = 4
    }
}