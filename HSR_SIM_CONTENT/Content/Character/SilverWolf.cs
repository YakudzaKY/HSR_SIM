using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
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
    private readonly AppliedBuff allowChangesDebuff;

    private readonly double allowChangesDebuffAllDmgVal;
    private readonly double alwChgAtk;
    private readonly AppliedBuff[] bugArray;

    private readonly int bugDuration;
    private readonly int swSkillLvl;


    private readonly double talentDebuffChance;

    private readonly TriggerEvent trgEnt;

    private readonly double ultDmg;
    private readonly Event ultimateHitLastEvent;

    public SilverWolf(Unit parent) : base(parent)
    {
        //on this event we will place debuff
        trgEnt = new TriggerEvent(null, null, Parent);

        var allowChangeChance =
            FighterUtils.GetAbilityScaling(0.75, 0.85, Parent.Skills.First(x => x.Name == "Allow Changes?").Level);
        alwChgAtk = FighterUtils.GetAbilityScaling(0.98, 1.96,
            Parent.Skills.First(x => x.Name == "Allow Changes?").Level);
        swSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "System Warning")!.Level;
        allowChangesDebuffAllDmgVal =
            FighterUtils.GetAbilityScaling(0.075, 0.10, Parent.Skills.First(x => x.Name == "Allow Changes?").Level);

        ultDmg = FighterUtils.GetAbilityScaling(2.28, 3.80, Parent.Skills.First(x => x.Name == "User Banned").Level);
        var ultDef =
            FighterUtils.GetAbilityScaling(0.36, 0.45, Parent.Skills.First(x => x.Name == "User Banned").Level);
        var ultChance =
            FighterUtils.GetAbilityScaling(0.85, 100, Parent.Skills.First(x => x.Name == "User Banned").Level);

        var talentLvl = Parent.Skills.First(x => x.Name == "Awaiting System Response...").Level;
        talentDebuffChance = FighterUtils.GetAbilityScaling(0.60, 0.72, talentLvl);

        //weakness duration depend on A4 traits
        var weaknessDuration = ATraces.HasFlag(ATracesEnm.A4) ? 3 : 2;

        allowChangesDebuff = new AppliedBuff(Parent, null, this)
        {
            Type = Buff.BuffType.Debuff,
            BaseDuration = weaknessDuration,
            CustomIconName = "Allow_Changes",
            Effects = new List<Effect>(),
            EffectStackingType = AppliedBuff.EffectStackingTypeEnm.FullReplace
        };

        var allowChangesDebuffAllDmgRes = new AppliedBuff(Parent, null, this)
        {
            Type = Buff.BuffType.Debuff,
            BaseDuration = 2,
            Effects = [new EffAllDamageResist { CalculateValue = CalcSkillAllDmgRes }]
        };

        var ultDefDebuff = new AppliedBuff(Parent, null, this)
        {
            Type = Buff.BuffType.Debuff,
            BaseDuration = 3,
            Effects = [new EffDefPrc { Value = -ultDef }]
        };

        bugDuration = ATraces.HasFlag(ATracesEnm.A2) ? 4 : 3;
        var talentAtkDebuff = new AppliedBuff(Parent, null, this)
        {
            Type = Buff.BuffType.Debuff,
            BaseDuration = bugDuration,
            Effects = [new EffAtkPrc { Value = -FighterUtils.GetAbilityScaling(0.05, 0.1, talentLvl) }]
        };
        var talentDefDebuff = new AppliedBuff(Parent, null, this)
        {
            Type = Buff.BuffType.Debuff,
            BaseDuration = bugDuration,
            Effects = [new EffDefPrc { Value = -FighterUtils.GetAbilityScaling(0.04, 0.08, talentLvl) }]
        };
        var talentSpdDebuff = new AppliedBuff(Parent, null, this)
        {
            Type = Buff.BuffType.Debuff,
            BaseDuration = bugDuration,
            Effects = [new EffSpeedPrc { Value = -FighterUtils.GetAbilityScaling(0.03, 0.06, talentLvl) }]
        };
        bugArray = new[] { talentAtkDebuff, talentSpdDebuff, talentDefDebuff };
        //=====================
        //Abilities
        //=====================

        var ability =
            //Force Quit Program
            new Ability(this)
            {
                AbilityType = AbilityTypeEnm.Technique,
                Name = "Force Quit Program",
                Cost = 1,
                CostType = Resource.ResourceType.TP,
                AdjacentTargets = AdjacentTargetsEnm.All,
                IgnoreWeakness = true
            };
        //dmg events
        ability.Events.Add(new DirectDamage(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            CalculateValue = FighterUtils.DamageFormula(new Formula()
            {
                Expression =
                    $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas.Attack)} * 0.8"
            })
        });
        ability.Events.Add(trgEnt);
        //shield break in this case going after skill dmg
        ability.Events.Add(new ResourceDrain(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            ResType = Resource.ResourceType.Toughness,
            Value = 60
        });

        Abilities.Add(ability);


        //System Warning
        var systemWarning = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Basic,
            Name = "System Warning",
            AdjacentTargets = AdjacentTargetsEnm.None,
            SpGain = 1
        };
        //dmg events
        foreach (var proportion in new[] { 0.25, 0.25, 0.5 })
        {
            systemWarning.Events.Add(new DirectDamage(null, this, Parent)
            {
                CalculateValue = FighterUtils.DamageFormula(new Formula()
                {
                    Expression =
                        $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas.Attack)} * (0.4 + {swSkillLvl} * 0.1) * {proportion}"
                })
            });
            systemWarning.Events.Add(new ToughnessShred(null, this, Parent)
                { Value = 30 * proportion });
            systemWarning.Events.Add(new EnergyGain(null, this, Parent)
                { Value = 20 * proportion, TargetUnit = Parent });
        }

        systemWarning.Events.Add(trgEnt);
        Abilities.Add(systemWarning);

        //Allow Changes?
        var allowChanges = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ability,
            Name = "Allow Changes?",
            AdjacentTargets = AdjacentTargetsEnm.None,
            CostType = Resource.ResourceType.SP,
            Cost = 1
        };
        allowChanges.Events.Add(new AttemptEffect(null, this, Parent)
            { BaseChance = allowChangeChance, AppliedBuffToApply = allowChangesDebuff });
        allowChanges.Events.Add(new AttemptEffect(null, this, Parent)
            { BaseChance = 1, AppliedBuffToApply = allowChangesDebuffAllDmgRes });
        //dmg events
        allowChanges.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = FighterUtils.DamageFormula(new Formula()
            {
                Expression =
                    $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas.Attack)} * {alwChgAtk}"
            })
        });

        allowChanges.Events.Add(new ToughnessShred(null, this, Parent) { Value = 60 });
        allowChanges.Events.Add(trgEnt);
        allowChanges.Events.Add(new EnergyGain(null, this, Parent)
            { Value = 30, TargetUnit = Parent });
        Abilities.Add(allowChanges);

        //User Banned
        var userBanned = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ultimate,
            Name = "User Banned",
            AdjacentTargets = AdjacentTargetsEnm.None,
            CostType = Resource.ResourceType.Energy,
            Cost =MaxEnergy
        };
        userBanned.Events.Add(new AttemptEffect(null, this, Parent)
            { BaseChance = ultChance, AppliedBuffToApply = ultDefDebuff });
        //dmg events
        userBanned.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = FighterUtils.DamageFormula(new Formula()
            {
                Expression =
                    $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas.Attack)} * {ultDmg}"
            })
        });
        userBanned.Events.Add(new ToughnessShred(null, this, Parent) { Value = 90 });
        userBanned.Events.Add(trgEnt);
        ultimateHitLastEvent = new EnergyGain(null, this, Parent)
            { Value = 5, TargetUnit = Parent };
        userBanned.Events.Add(ultimateHitLastEvent);
        //for E4
        Abilities.Add(userBanned);

        if (Parent.Rank >= 6)
            Parent.PassiveBuffs.Add(new PassiveBuff(Parent, this)
            {
                Effects = [new EffAllDamageBoost { CalculateValue = CalculateE6 }],
                Target = Parent,
                IsTargetCheck = true
            });
    }

    public sealed override double MaxEnergy { get; set; } = 110;
    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Nihility;


    private Formula CalcSkillAllDmgRes(Event ent)
    {
        //get default from table
        var res = allowChangesDebuffAllDmgVal;
        double debuffs =
            ent.TargetUnit.AppliedBuffs.Count(x => x.Type is Buff.BuffType.Debuff or Buff.BuffType.Dot);

        //depend on debuffs add 3%
        if (debuffs >= 3) res += 0.03;


        return new Formula() { Expression = $"- {res}"};
    }

    protected override void DefaultFighter_HandleEvent(Event ent)
    {
        //if unit consume hp or got attack then apply buff
        if (Parent is { Rank: >= 2, IsAlive: true } && ent is UnitEnteringBattle)
        {
            //if enemy enter combat need debuff
            if (Parent.Enemies.Any(x => x == ent.TargetUnit))
            {
                ApplyBuff newEvent = new(ent.ParentStep, this, Parent)
                {
                    TargetUnit = ent.TargetUnit,
                    AppliedBuffToApply = new AppliedBuff(Parent, null, this)
                    {
                        Dispellable = false,
                        Type = Buff.BuffType.Debuff,
                        Effects = [new EffEffectResPrc { Value = -0.20 }]
                    }
                };

                ent.ChildEvents.Add(newEvent);
            }
        }
        //E1-4
        else if (Parent.Rank >= 1 && ent.Reference == ultimateHitLastEvent) //EnergyGain event
        {
            var tarUnit = ent.ParentStep.Target;
            double debuffs = tarUnit.AppliedBuffs.Count(x =>
                x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);
            debuffs = Math.Min(debuffs, 5);
            if (Parent.Rank >= 1)
                for (var i = 0; i < debuffs; i++)
                    ent.ChildEvents.Add(new EnergyGain(ent.ParentStep, this, Parent)
                        { TargetUnit = Parent, Value = 7 });
            if (Parent.Rank >= 4)
                for (var i = 0; i < debuffs; i++)
                    ent.ChildEvents.Add(new DirectDamage(ent.ParentStep, this, Parent)
                    {
                        TargetUnit = ent.ParentStep.Target, CalculateValue = FighterUtils.DamageFormula(new Formula()
                        {
                            Expression =
                                $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas.Attack)} * 0.2"
                        })
                    });
        }

        else if (ent is ApplyBuff ab && ent.SourceUnit == Parent)
        {
            //if allow changes landed then choose element
            if (ab.AppliedBuffToApply == allowChangesDebuff)
            {
                //first find elements not in native weakness
                var reduceResists = true;
                var elemListToApply = GetAliveFriends().Select(x => x.AttackElement)
                    .Where(x => !ent.TargetUnit.NativeWeaknesses.Contains(x)).Distinct();
                if (!elemListToApply.Any())
                {
                    elemListToApply =
                        GetAliveFriends().Select(x => x.AttackElement).Distinct(); //else pick all elements
                    reduceResists = false;
                }

                var elm = (ElementEnm)Utl.GetRandomObject(elemListToApply, Parent.ParentTeam.ParentSim.Parent);
                ent.ChildEvents.Add(new ApplyBuffEffect(ent.ParentStep, this, Parent)
                {
                    TargetUnit = ent.TargetUnit,
                    AppliedBuffToApply = allowChangesDebuff,
                    Eff = new EffWeaknessImpair { Element = elm }
                });
                if (reduceResists)
                    ent.ChildEvents.Add(new ApplyBuffEffect(ent.ParentStep, this, Parent)
                    {
                        TargetUnit = ent.TargetUnit,
                        AppliedBuffToApply = allowChangesDebuff,
                        Eff = new EffElementalResist { Element = elm, Value = -0.2 }
                    });
            }
        }
        else if (ent.Reference == trgEnt && ent.SourceUnit == Parent)
        {
            ImplantBug(ent.TargetUnit, ent, talentDebuffChance);
        }
        //a2. shield got broken by teammate
        else if (ent is ToughnessBreak && ATraces.HasFlag(ATracesEnm.A2) &&
                 ent.SourceUnit.ParentTeam == Parent.ParentTeam)
        {
            ImplantBug(ent.TargetUnit, ent, 0.65); //fixed chance
        }

        base.DefaultFighter_HandleEvent(ent);
    }


    //apply talent Debuff
    private void ImplantBug(Unit target, Event ent, double chance)
    {
        var currentBugs = target.AppliedBuffs.Where(x => bugArray.Contains(x.Reference)).ToArray();
        //try search bugs to implant by lowest duration
        for (var i = 1; i <= bugDuration; i++)
        {
            //get current bugs on target
            var i1 = i;
            var notExistsBugs = bugArray
                .Where(x => !currentBugs.Where(y => y.DurationLeft == i1).Select(y => y.Reference).Contains(x))
                .ToArray();
            if (notExistsBugs.Length > 0 && notExistsBugs.Length < bugArray.Length)
            {
                ent.ChildEvents.Add(new AttemptEffect(ent.ParentStep, this, Parent)
                {
                    AppliedBuffToApply =
                        (AppliedBuff)Utl.GetRandomObject(notExistsBugs, Parent.ParentTeam.ParentSim.Parent),
                    BaseChance = chance, TargetUnit = target
                });
                break;
            }

            //if no debuff founded  then get random from start array
            if (i == bugDuration)
                ent.ChildEvents.Add(new AttemptEffect(ent.ParentStep, this, Parent)
                {
                    AppliedBuffToApply = (AppliedBuff)Utl.GetRandomObject(bugArray, Parent.ParentTeam.ParentSim.Parent),
                    BaseChance = chance, TargetUnit = target
                });
        }
    }


    //get 0.2 AllDmg per debuff  on target
    private static Formula CalculateE6(Event ent)
    {
        const double maxDebuffs = 5;
        double debuffs = ent.TargetUnit.AppliedBuffs.Count(x =>
            x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);
        if (debuffs > maxDebuffs) debuffs = maxDebuffs;
        return new Formula() { Expression = $"{debuffs} * 0.2"};
    }
}