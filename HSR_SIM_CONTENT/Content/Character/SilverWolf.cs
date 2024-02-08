﻿using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_CONTENT.Content.Character;

public class SilverWolf : DefaultFighter
{
    private readonly Buff allowChangesDebuff;
    private readonly Buff allowChangesDebuffAllDmgRes;
    private readonly double allowChangesDebuffAllDmgVal;
    private readonly double alwChgAtk;


    private readonly double alwChgChnc;
    private readonly Buff[] bugArray;
    private readonly int bugDuration;
    private readonly int swSkillLvl;

    private readonly Buff talentAtkDebuff;
    private readonly double talentDebuffChance;
    private readonly Buff talentDefDebuff;
    private readonly Buff talentSpdDebuff;
    private readonly TriggerEvent trgEnt;
    private readonly double ultChance;
    private readonly double ultDef;
    private readonly Buff ultDefDebuff;
    private readonly double ultDmg;
    private readonly Event ultimateHitLastEvent;
    private readonly int weaknessDuration;
    private readonly int talentLvl;

    public SilverWolf(Unit parent) : base(parent)
    {
        trgEnt = new TriggerEvent(null, null, Parent);
        Parent.Stats.BaseMaxEnergy = 110;
        alwChgChnc =
            FighterUtils.GetAbilityScaling(0.75, 0.85, Parent.Skills.First(x => x.Name == "Allow Changes?").Level);
        alwChgAtk = FighterUtils.GetAbilityScaling(0.98, 1.96,
            Parent.Skills.First(x => x.Name == "Allow Changes?").Level);
        swSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "System Warning").Level;
        allowChangesDebuffAllDmgVal =
            FighterUtils.GetAbilityScaling(0.075, 0.10, Parent.Skills.First(x => x.Name == "Allow Changes?").Level);

        ultDmg = FighterUtils.GetAbilityScaling(2.28, 3.80, Parent.Skills.First(x => x.Name == "User Banned").Level);
        ultDef = FighterUtils.GetAbilityScaling(0.36, 0.45, Parent.Skills.First(x => x.Name == "User Banned").Level);
        ultChance = FighterUtils.GetAbilityScaling(0.85, 100, Parent.Skills.First(x => x.Name == "User Banned").Level);

        talentLvl = Parent.Skills.First(x => x.Name == "Awaiting System Response...").Level;
        talentDebuffChance = FighterUtils.GetAbilityScaling(0.60, 0.72, talentLvl);

        weaknessDuration = Atraces.HasFlag(ATracesEnm.A4) ? 3 : 2;

        allowChangesDebuff = new Buff(Parent)
        {
            Type = Buff.BuffType.Debuff, BaseDuration = weaknessDuration, CustomIconName = "Allow_Changes",
            Effects = new List<Effect>(), EffectStackingType = Buff.EffectStackingTypeEnm.FullReplace
        };

        allowChangesDebuffAllDmgRes = new Buff(Parent)
        {
            Type = Buff.BuffType.Debuff, BaseDuration = 2,
            Effects = new List<Effect> { new EffAllDamageResist { CalculateValue = calcSkillAllDmgRes } }
        };

        ultDefDebuff = new Buff(Parent)
        {
            Type = Buff.BuffType.Debuff, BaseDuration = 3,
            Effects = new List<Effect> { new EffDefPrc { Value = -ultDef } }
        };

        bugDuration = Atraces.HasFlag(ATracesEnm.A2) ? 4 : 3;
        talentAtkDebuff = new Buff(Parent)
        {
            Type = Buff.BuffType.Debuff, BaseDuration = bugDuration,
            Effects = new List<Effect>
                { new EffAtkPrc { Value = -FighterUtils.GetAbilityScaling(0.05, 0.1, talentLvl) } }
        };
        talentDefDebuff = new Buff(Parent)
        {
            Type = Buff.BuffType.Debuff, BaseDuration = bugDuration,
            Effects = new List<Effect>
                { new EffDefPrc { Value = -FighterUtils.GetAbilityScaling(0.04, 0.08, talentLvl) } }
        };
        talentSpdDebuff = new Buff(Parent)
        {
            Type = Buff.BuffType.Debuff, BaseDuration = bugDuration,
            Effects = new List<Effect>
                { new EffSpeedPrc { Value = -FighterUtils.GetAbilityScaling(0.03, 0.06, talentLvl) } }
        };
        bugArray = new[] { talentAtkDebuff, talentSpdDebuff, talentDefDebuff };
        //=====================
        //Abilities
        //=====================

        Ability ability;
        //Force Quit Program
        ability = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Technique,
            Name = "Force Quit Program",
            Cost = 1,
            CostType = Resource.ResourceType.TP,
            Element = Element,
            AdjacentTargets = AdjacentTargetsEnm.All,
            IgnoreWeakness = true
        };
        //dmg events
        ability.Events.Add(new DirectDamage(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            CalculateValue = CalculateFqpDmg
        });
        ability.Events.Add(trgEnt);
        //shield break in this case going after skill dmg
        ability.Events.Add(new ResourceDrain(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            ResType = Resource.ResourceType.Toughness,
            Val = 60
        });

        Abilities.Add(ability);


        //System Warning
        Ability SystemWarning;
        SystemWarning = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Basic,
            Name = "System Warning",
            AdjacentTargets = AdjacentTargetsEnm.None,
            SpGain = 1
        };
        //dmg events
        foreach (var proportion in new[] { 0.25, 0.25, 0.5 })
        {
            SystemWarning.Events.Add(new DirectDamage(null, this, Parent)
                { CalculateValue = CalculateBasicDmg, CalculateProportion = proportion });

            SystemWarning.Events.Add(new ToughnessShred(null, this, Parent)
                { Val = 30, CalculateProportion = proportion });
            SystemWarning.Events.Add(new EnergyGain(null, this, Parent)
                { Val = 20, TargetUnit = Parent, CalculateProportion = proportion });
        }

        SystemWarning.Events.Add(trgEnt);
        Abilities.Add(SystemWarning);

        //Allow Changes?
        Ability AllowChanges;
        AllowChanges = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ability,
            Name = "Allow Changes?",
            AdjacentTargets = AdjacentTargetsEnm.None,
            CostType = Resource.ResourceType.SP,
            Cost = 1
        };
        AllowChanges.Events.Add(new AttemptEffect(null, this, Parent)
            { BaseChance = alwChgChnc, BuffToApply = allowChangesDebuff });
        AllowChanges.Events.Add(new AttemptEffect(null, this, Parent)
            { BaseChance = 1, BuffToApply = allowChangesDebuffAllDmgRes });
        //dmg events
        AllowChanges.Events.Add(new DirectDamage(null, this, Parent)
            { CalculateValue = CalculateAbilityDmg });
        AllowChanges.Events.Add(new ToughnessShred(null, this, Parent) { Val = 60 });
        AllowChanges.Events.Add(trgEnt);
        AllowChanges.Events.Add(new EnergyGain(null, this, Parent)
            { Val = 30, TargetUnit = Parent });
        Abilities.Add(AllowChanges);

        //User Banned
        Ability UserBanned;
        UserBanned = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ultimate,
            Name = "User Banned",
            AdjacentTargets = AdjacentTargetsEnm.None,
            CostType = Resource.ResourceType.Energy,
            Cost = Parent.Stats.BaseMaxEnergy
        };
        UserBanned.Events.Add(new AttemptEffect(null, this, Parent)
            { BaseChance = ultChance, BuffToApply = ultDefDebuff });
        //dmg events
        UserBanned.Events.Add(new DirectDamage(null, this, Parent)
            { CalculateValue = CalculateUltimateDmg });
        UserBanned.Events.Add(new ToughnessShred(null, this, Parent) { Val = 90 });
        UserBanned.Events.Add(trgEnt);
        ultimateHitLastEvent = new EnergyGain(null, this, Parent)
            { Val = 5, TargetUnit = Parent };
        UserBanned.Events.Add(ultimateHitLastEvent);

        Abilities.Add(UserBanned);

        if (Parent.Rank >= 6)
            PassiveBuffs.Add(new PassiveBuff(Parent)
            {
                AppliedBuff = new Buff(Parent)
                {
                    Effects = new List<Effect>
                        { new EffAllDamageBoost { CalculateValue = CalculateE6, RealTimeRecalculateValue = true } }
                },
                Target = Parent,
                IsTargetCheck = true
            });
    }

    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Nihility;
    public sealed override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Quantum;

    private double? CalculateAbilityDmg(Event ent)
    {
        var attackPart = Parent.GetAttack(ent) * alwChgAtk;

        return FighterUtils.CalculateDmgByBasicVal(attackPart, ent);
    }

    private double? CalculateUltimateDmg(Event ent)
    {
        var attackPart = Parent.GetAttack(ent) * ultDmg;

        return FighterUtils.CalculateDmgByBasicVal(attackPart, ent);
    }

    private double? CalculateE4Dmg(Event ent)
    {
        var attackPart = Parent.GetAttack(ent) * 0.2;

        return FighterUtils.CalculateDmgByBasicVal(attackPart, ent);
    }

    private double? calcSkillAllDmgRes(Event ent)
    {
        //get default from table
        var res = allowChangesDebuffAllDmgVal;
        double debuffs = 0;


        debuffs = ent.TargetUnit.Buffs.Count(x => x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);
        //depend on debuffs add 3%
        if (debuffs >= 3) res += 0.03;

        return -res;
    }

    public override void DefaultFighter_HandleEvent(Event ent)
    {
        //if unit consume hp or got attack then apply buff
        if (Parent.Rank >= 2 && Parent.IsAlive && ent is UnitEnteringBattle)
        {
            //if enemy enter combat need debuff
            if (Parent.Enemies.Any(x => x == ent.TargetUnit))
            {
                ApplyBuff newEvent = new(ent.ParentStep, this, Parent)
                {
                    TargetUnit = ent.TargetUnit,
                    BuffToApply = new Buff(Parent)
                    {
                        Dispellable = false,
                        Type = Buff.BuffType.Debuff,
                        Effects = new List<Effect> { new EffEffectResPrc { Value = -0.20 } }
                    }
                };

                ent.ChildEvents.Add(newEvent);
            }
        }

        else if (ent is ApplyBuff ab && ent.SourceUnit == Parent)
        {
            //if allow changes landed then choose element
            if (ab.BuffToApply == allowChangesDebuff)
            {
                //first find elements not in native weakness
                var reduceResists = true;
                var elemListToApply = GetAliveFriends().Select(x => x.Fighter.Element)
                    .Where(x => !ent.TargetUnit.Fighter.NativeWeaknesses.Contains(x)).Distinct();
                if (!elemListToApply.Any())
                {
                    elemListToApply =
                        GetAliveFriends().Select(x => x.Fighter.Element).Distinct(); //else pick all elements
                    reduceResists = false;
                }

                var elm = (Unit.ElementEnm)Utl.GetRandomObject(elemListToApply, Parent.ParentTeam.ParentSim.Parent);
                ent.ChildEvents.Add(new ApplyBuffEffect(ent.ParentStep, this, Parent)
                {
                    TargetUnit = ent.TargetUnit, BuffToApply = allowChangesDebuff,
                    Eff = new EffWeaknessImpair { Element = elm }
                });
                if (reduceResists)
                    ent.ChildEvents.Add(new ApplyBuffEffect(ent.ParentStep, this, Parent)
                    {
                        TargetUnit = ent.TargetUnit, BuffToApply = allowChangesDebuff,
                        Eff = new EffElementalResist { Element = elm, Value = -0.2 }
                    });
            }
        }
        //E2-4
        else if (Parent.Rank >= 2 && ent.Reference == ultimateHitLastEvent) //EnergyGain event
        {
            double debuffs;
            var tarUnit = ent.ParentStep.Target;
            debuffs = tarUnit.Buffs.Count(x => x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);
            debuffs = Math.Min(debuffs, 5);
            if (Parent.Rank >= 4)
                for (var i = 0; i < debuffs; i++)
                    ent.ChildEvents.Add(new DirectDamage(ent.ParentStep, this, Parent)
                        { TargetUnit = tarUnit, CalculateValue = CalculateE4Dmg });
            ent.ChildEvents.Add(new EnergyGain(ent.ParentStep, this, Parent)
                { TargetUnit = Parent, Val = 7 * debuffs });
        }

        else if (ent.Reference == trgEnt && ent.SourceUnit == Parent)
        {
            ImplantBug(ent.TargetUnit, ent, talentDebuffChance);
        }
        //a2. shield got broken by teammate
        else if (ent is ToughnessBreak && Atraces.HasFlag(ATracesEnm.A2) &&
                 ent.SourceUnit.ParentTeam == Parent.ParentTeam)
        {
            ImplantBug(ent.TargetUnit, ent, 0.65); //fixed chance
        }

        base.DefaultFighter_HandleEvent(ent);
    }

    //apply talent Debuff
    private void ImplantBug(Unit target, Event ent, double chance)
    {
        var currentBugs = target.Buffs.Where(x => bugArray.Contains(x.Reference)).ToArray();
        //try search bugs to implant by lowest duration
        for (var i = 1; i <= bugDuration; i++)
        {
            //get current bugs on target
            var notExistsBugs = bugArray
                .Where(x => !currentBugs.Where(y => y.DurationLeft == i).Select(y => y.Reference).Contains(x))
                .ToArray();
            if (notExistsBugs.Length > 0 && notExistsBugs.Length < bugArray.Length)
            {
                var randomBug = (Buff)Utl.GetRandomObject(notExistsBugs, Parent.ParentTeam.ParentSim.Parent);
                ent.ChildEvents.Add(new AttemptEffect(ent.ParentStep, this, ent.SourceUnit)
                    { BuffToApply = randomBug, BaseChance = chance, TargetUnit = target });
                break;
            }

            //if no debuff founded  then get random from start array
            if (i == bugDuration)
            {
                var randomBug = (Buff)Utl.GetRandomObject(bugArray, Parent.ParentTeam.ParentSim.Parent);
                ent.ChildEvents.Add(new AttemptEffect(ent.ParentStep, this, ent.SourceUnit)
                    { BuffToApply = randomBug, BaseChance = chance, TargetUnit = target });
            }
        }
    }


    private double? CalculateFqpDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(ent) * 0.8, ent);
    }

    //50-110
    private double? CalculateBasicDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(
            Parent.GetAttack(ent) * (0.4 + swSkillLvl * 0.1),
            ent);
    }

    //get 0.2 AllDmg per debuff  on target
    private static double? CalculateE6(Event ent)
    {
        double maxDebuffs = 5;
        double debuffs = 0;


        debuffs = ent.TargetUnit.Buffs.Count(x => x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);
        if (debuffs > maxDebuffs) debuffs = maxDebuffs;

        return debuffs * 0.2;
    }
}