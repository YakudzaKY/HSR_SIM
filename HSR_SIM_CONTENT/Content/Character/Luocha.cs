﻿using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_CONTENT.Content.Character;

public class Luocha : DefaultFighter
{
    private readonly double coLfAtk;
    private readonly double coLfFix;
    private readonly Ability cycleOfLife;
    private readonly int cycleOfLifeMaxCnt = 2;
    private readonly double deathWish;
    private readonly AppliedBuff e2ShieldAppliedBuff;
    private readonly double poAfAtk;
    private readonly double poAfFix;

    private readonly Ability? prayerOfAbyssFlower;
    private readonly Ability? prayerOfAbyssFlowerAuto;
    private readonly int totALvl;
    private readonly AppliedBuff triggerCdAppliedBuff;
    private readonly Ability ultimateAbility;
    private readonly AppliedBuff uniqueAppliedBuff;

    private List<KeyValuePair<Unit, Unit?>> trackedUnits = new();

    public Luocha(Unit parent) : base(parent)
    {
        var coLLvl = Parent.Skills.First(x => x.Name == "Cycle of Life")!.Level;
        var dWLvl = Parent.Skills.First(x => x.Name == "Death Wish")!.Level;
        totALvl = Parent.Skills.First(x => x.Name == "Thorns of the Abyss")!.Level;
        var poALvl = Parent.Skills.First(x => x.Name == "Prayer of Abyss Flower")!.Level;

        coLfAtk = FighterUtils.GetAbilityScaling(0.12, 0.18, coLLvl);
        coLfFix = FighterUtils.GetAbilityScaling(60, 240, coLLvl);
        deathWish = FighterUtils.GetAbilityScaling(1.20, 2, dWLvl);
        poAfAtk = FighterUtils.GetAbilityScaling(0.4, 0.6, poALvl);
        poAfFix = FighterUtils.GetAbilityScaling(200, 800, poALvl);

        uniqueAppliedBuff = new AppliedBuff(Parent, null, this)
        {
            Type = Buff.BuffType.Buff,
            BaseDuration = 2,
            MaxStack = 1,
            Dispellable = false,
            CustomIconName = "Icon_Abyss_Flower",
            Effects = []
        };

        triggerCdAppliedBuff = new AppliedBuff(Parent, null, this)
        {
            Type = Buff.BuffType.Debuff,
            BaseDuration = 2,
            MaxStack = 1,
            Dispellable = false,
            CustomIconName = "Abyss_Flower_CD",
            Effects = []
        };

        //=====================
        //Abilities
        //=====================
        //Cycle of life
        cycleOfLife = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.FollowUpAction,
            Name = "Cycle of life",
            Available = ColAvailable,
            FollowUpPriority = PriorityEnm.Medium,
            TargetType = TargetTypeEnm.Self
        };
        cycleOfLife.Events.Add(new MechanicValReset(null, this, Parent)
            { AbilityValue = cycleOfLife });
        cycleOfLife.Events.Add(new ApplyBuff(null, this, Parent)
        {
            TargetUnit = Parent,
            AppliedBuffToApply = uniqueAppliedBuff
        });
        Mechanics.AddVal(cycleOfLife);
        Abilities.Add(cycleOfLife);

        //Prayer of Abyss Flower
        prayerOfAbyssFlower = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ability,
            Name = "Prayer of Abyss Flower",
            TargetType = TargetTypeEnm.Friend,
            CostType = Resource.ResourceType.SP,
            Cost = 1
        };
        if (parent.ATraces.HasFlag(Unit.ATracesEnm.A2))
            prayerOfAbyssFlower.Events.Add(new DispelBad(null, this, Parent));

        prayerOfAbyssFlower.Events.Add(new Healing(null, this, Parent)
        {
            CalculateValue = FighterUtils.HealFormula(new Formula
                { Expression = $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Attack)} * {poAfAtk} + {poAfFix}" })
        });
        prayerOfAbyssFlower.Events.Add(new EnergyGain(null, this, Parent)
            { Value = 30, TargetUnit = Parent });
        Abilities.Add(prayerOfAbyssFlower);


        //Prayer of Abyss Flower(auto)
        prayerOfAbyssFlowerAuto = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.FollowUpAction,
            Name = "Prayer of Abyss Flower (auto)",
            Available = PoAfAvailable,
            FollowUpPriority = PriorityEnm.High,
            TargetType = TargetTypeEnm.Friend,
            FollowUpTargets = trackedUnits
        };
        if (parent.ATraces.HasFlag(Unit.ATracesEnm.A2))
            prayerOfAbyssFlowerAuto.Events.Add(new DispelBad(null, this, Parent));
        prayerOfAbyssFlowerAuto.Events.Add(new Healing(null, this, Parent)
        {
            CalculateValue = FighterUtils.HealFormula(new Formula
                { Expression = $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Attack)} * {poAfAtk} + {poAfFix}" })
        });
        prayerOfAbyssFlowerAuto.Events.Add(new EnergyGain(null, this, Parent)
            { Value = 30, TargetUnit = Parent });
        prayerOfAbyssFlowerAuto.Events.Add(new ApplyBuff(null, this, Parent)
        {
            TargetUnit = Parent,
            AppliedBuffToApply = triggerCdAppliedBuff
        });
        Abilities.Add(prayerOfAbyssFlowerAuto);

        e2ShieldAppliedBuff = new AppliedBuff(Parent, null, this)
        {
            Effects =
            [
                new EffShield
                {
                    CalculateValue = FighterUtils.ShieldFormula(new Formula
                    {
                        Expression =
                            $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Attack)} * 0.18 + 240"
                    })
                }
            ]
        };

        //basic attack
        var thornsOfTheAbyss = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Basic,
            Name = "Thorns of the Abyss",
            AdjacentTargets = AdjacentTargetsEnm.None,
            SpGain = 1
        };

        foreach (var proportion in new[] { 0.3, 0.3, 0.4 })
        {
            thornsOfTheAbyss.Events.Add(new DirectDamage(null, this, Parent)
            {
                CalculateValue = FighterUtils.DamageFormula(new Formula
                {
                    Expression =
                        $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Attack)} * (0.4 + {totALvl} * 0.1) * {proportion} "
                })
            });

            thornsOfTheAbyss.Events.Add(new ToughnessShred(null, this, Parent)
                { Value = 30 * proportion });
            thornsOfTheAbyss.Events.Add(new EnergyGain(null, this, Parent)
                { Value = 20 * proportion, TargetUnit = Parent });
        }


        Abilities.Add(thornsOfTheAbyss);


        // ULTIMATE
        ultimateAbility = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ultimate,
            Name = "Death Wish",
            AdjacentTargets = AdjacentTargetsEnm.All,
            CostType = Resource.ResourceType.Energy,
            Cost = MaxEnergy,
            IWannaUseIt = WannaUseDw
        };
        //dmg events
        //E6
        if (Parent.Rank >= 6)
            ultimateAbility.Events.Add(new ApplyBuff(null, this, Parent)
            {
                AppliedBuffToApply = new AppliedBuff(Parent, null, this)
                {
                    CustomIconName = "Ability_Death_Wish", BaseDuration = 2, Type = Buff.BuffType.Debuff,
                    Effects = [new EffAllDamageResist { Value = -0.2 }]
                },
                CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent
            });
        ultimateAbility.Events.Add(new DispelGood(null, this, Parent)
            { CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
        ultimateAbility.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = FighterUtils.DamageFormula(new Formula
            {
                Expression =
                    $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Attack)} * {deathWish} "
            }),
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent
        });
        ultimateAbility.Events.Add(new ToughnessShred(null, this, Parent)
            { Value = 60, CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
        ultimateAbility.Events.Add(new EnergyGain(null, this, Parent)
            { Value = 5, TargetUnit = Parent });
        Abilities.Add(ultimateAbility);


        //Mercy of a Fool
        var ability = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Technique,
            Name = "Mercy of a Fool",
            Cost = 1,
            CostType = Resource.ResourceType.TP
        };
        ability.Events.Add(new MechanicValChg(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            Value = cycleOfLifeMaxCnt,
            AbilityValue = cycleOfLife
        }); //AbilityValue for Cycle of life
        Abilities.Add(ability);

        //E1
        if (Parent.Rank >= 1)
            //CoL buffs
            Parent.PassiveBuffs.Add(new PassiveBuff(Parent, this)
            {
                Effects = [new EffAtkPrc { Value = 0.2 }],
                CustomIconName = uniqueAppliedBuff.CustomIconName,

                Target = Parent.ParentTeam,
                ApplyConditions =
                [
                    new Condition
                    {
                        AppliedBuffValue = uniqueAppliedBuff,
                        ConditionExpression = Condition.ConditionCheckExpression.Exists,
                        ConditionParam = Condition.ConditionCheckParam.Buff
                    }
                ]
            });


        //A6
        if (parent.ATraces.HasFlag(Unit.ATracesEnm.A6))
            Parent.NativeDebuffResists.Add(new DebuffResist { Debuff = typeof(EffCrowdControl), ResistVal = 0.7 });
        //E2
        if (Parent.Rank >= 2)
            Parent.PassiveBuffs.Add(new PassiveBuff(Parent, this)
            {
                Effects = [new EffOutgoingHealingPrc { CalculateValue = CalculateE2 }],
                Target = Parent,
                IsTargetCheck = true
            });

        //E4
        if (Parent.Rank >= 4)
            //CoL buffs
            Parent.PassiveBuffs.Add(new PassiveBuff(Parent, this)
            {
                Type = Buff.BuffType.Debuff,
                Effects = [new EffAllDamageBoost { Value = -0.12 }],
                CustomIconName = uniqueAppliedBuff.CustomIconName,

                Target = TargetTypeEnm.Enemy,
                ApplyConditions =
                [
                    new Condition
                    {
                        AppliedBuffValue = uniqueAppliedBuff,
                        ConditionExpression = Condition.ConditionCheckExpression.Exists,
                        ConditionParam = Condition.ConditionCheckParam.Buff
                    }
                ]
            });
    }

    public sealed override double MaxEnergy { get; } = 100;
    public override ElementEnm Element { get; } = ElementEnm.Imaginary;
    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Abundance;


    //default all abilities we want to cast
    private bool WannaUseDw()
    {
        //if we have no CoL or enemy have buff
        return ultimateAbility.DefaultAbilityWannaUse() && (!ColBuffAvailable() ||
                                                            GetAliveEnemies().Count(x =>
                                                                x.AppliedBuffs.Any(y =>
                                                                    y.Dispellable &&
                                                                    y.Type == Buff.BuffType.Buff)) >
                                                            0);
    }

    private Formula CalculateE2(Event? ent)
    {
        double res = 0;

        if (ent != null && (ent.ParentStep.ActorAbility == prayerOfAbyssFlowerAuto ||
                            ent.ParentStep.ActorAbility == prayerOfAbyssFlower) &&
            ent.TargetUnit.HpPrc(ent: ent).Result < 0.5) res = 0.3;
        return new Formula { Expression = $"{res}" };
    }

    //If unit hp<=50% for Luocha follow up heals
    private bool UnitAtLowHpForAuto(Unit unit, Event? ent)
    {
        return unit.GetRes(Resource.ResourceType.HP).ResVal / unit.MaxHp(ent: ent).Result <= 0.5;
    }


    protected override void DefaultFighter_HandleEvent(Event ent)
    {
        //if unit consume hp or got attack then apply buff

        if (ent is FinishCombat)
        {
            trackedUnits = new List<KeyValuePair<Unit, Unit?>>();
        }
        else if (ent is ResourceDrain or DamageEventTemplate or Healing or ResourceGain or Defeat)
        {
            CheckAndAddTarget(ent.TargetUnit, ent);
        }
        else if (ent is ExecuteAbilityFinish && Parent.Friends.Any(x => x == ent.SourceUnit) &&
                 ent.ParentStep.ActorAbility.Attack && ent.SourceUnit.IsAlive && ColBuffAvailable())
        {
            ent.ChildEvents.Add(
                new Healing(ent.ParentStep, this,
                        Parent) //will put source unit coz Output healing calc will be calculated by target unit
                    {
                        TargetUnit = ent.SourceUnit,
                        CalculateValue = FighterUtils.HealFormula(new Formula
                        {
                            Expression =
                                $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Attack)} * {coLfAtk} + {coLfFix}"
                        })
                    });

            if (Parent.ATraces.HasFlag(Unit.ATracesEnm.A4))
                //for all friends except attacker unit
                foreach (var unit in ent.SourceUnit.Friends.Where(x => x.IsAlive && x != ent.SourceUnit))
                    ent.ChildEvents.Add(
                        new Healing(ent.ParentStep, this,
                                Parent) //will put source unit coz Output healing calc will be calculated by target unit
                            {
                                TargetUnit = unit,
                                CalculateValue = FighterUtils.HealFormula(new Formula
                                {
                                    Expression =
                                        $"{Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Attack)} * 0.07 + 93"
                                })
                            });
        }

        if (Parent.Rank >= 2 && ent is ExecuteAbilityStart && ent.SourceUnit == Parent &&
            (ent.ParentStep.ActorAbility == prayerOfAbyssFlowerAuto ||
             ent.ParentStep.ActorAbility == prayerOfAbyssFlower))
            //E2 shield part
            if (Parent.Rank >= 2 && ent.TargetUnit.HpPrc(ent: ent).Result >= 0.5)
                ent.ChildEvents.Add(new ApplyBuff(ent.ParentStep, this, ent.SourceUnit)
                    { TargetUnit = ent.TargetUnit, AppliedBuffToApply = e2ShieldAppliedBuff });


        base.DefaultFighter_HandleEvent(ent);
    }


    //check unit alive and hp status and add/or remove
    private void CheckAndAddTarget(Unit entTargetUnit, Event? ent)
    {
        //if friend have low hp then add to track
        if (Parent.Friends.Any(x => x == entTargetUnit))
        {
            if (trackedUnits.All(x => x.Key != entTargetUnit) && entTargetUnit.IsAlive &&
                UnitAtLowHpForAuto(entTargetUnit, ent))
            {
                trackedUnits.Add(new KeyValuePair<Unit, Unit?>(entTargetUnit, Parent));
            }
            else if (trackedUnits.Any(x => x.Key == entTargetUnit) &&
                     (!UnitAtLowHpForAuto(entTargetUnit, ent) || !entTargetUnit.IsAlive))
            {
                //remove high hp unit from track
                var tarPair = trackedUnits.FirstOrDefault(x => x.Key == entTargetUnit);
                trackedUnits.Remove(tarPair);
            }
        }
    }


    protected override void DefaultFighter_HandleStep(Step step)
    {
        //check if friendly unit do action
        if (step.StepType is Step.StepTypeEnm.ExecuteAbilityFromQueue or Step.StepTypeEnm.UnitFollowUpAction
            or Step.StepTypeEnm.UnitUltimate
            or Step.StepTypeEnm.UnitTurnContinued or Step.StepTypeEnm.UnitTurnStarted)
        {
            CheckAndAddTarget(step.Actor, null);
            //also check self  stacks
            if (!ColBuffAvailable() & (step.Actor == Parent) && (step.ActorAbility == ultimateAbility ||
                                                                 step.ActorAbility == prayerOfAbyssFlowerAuto ||
                                                                 step.ActorAbility == prayerOfAbyssFlower))
                step.Events.Add(new MechanicValChg(step, this, Parent)
                    { Value = 1, AbilityValue = cycleOfLife });
        }


        base.DefaultFighter_HandleStep(step);
    }


    public override string GetSpecialText()
    {
        return $"CoL: {Utl.DoubleToIntRound(Mechanics.Values[cycleOfLife]):d}\\{cycleOfLifeMaxCnt:d}";
    }

    private bool ColAvailable()
    {
        return cycleOfLife.DefaultAbilityAvailable() &&
               Utl.DoubleToIntRound(Mechanics.Values[cycleOfLife]) >= cycleOfLifeMaxCnt;
    }

    private bool PoAfAvailable()
    {
        return Parent.AppliedBuffs.All(x => x.Reference != triggerCdAppliedBuff);
    }

    private bool ColBuffAvailable()
    {
        return Parent.AppliedBuffs.Any(x => x.Reference == uniqueAppliedBuff);
    }
}