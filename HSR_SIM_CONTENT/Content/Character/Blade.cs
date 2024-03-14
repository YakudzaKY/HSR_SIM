﻿using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.Content.FighterUtils;


namespace HSR_SIM_CONTENT.Content.Character;

public class Blade : DefaultFighter
{
    private readonly Ability deathSentence; //ultimate

    //some modifiers


    private readonly Event forestMainTrgtHit; //last hit in main target by this ability(for event handlers)
    private readonly AppliedBuff hellscapeAppliedBuff; //E buff


    private readonly Ability? shuhuGift; //passive ability

    private readonly double shuHuMaxCnt; // passive max counter

    private Step lastDamageStep;

    public Blade(Unit? parent) : base(parent)
    {
        Parent.Stats.BaseMaxEnergy = 130;
        Parent.Element = Unit.ElementEnm.Wind;
        //load lvl
        var dsSkillLvl = Parent.Skills.First(x => x.Name == "Death Sentence").Level;
        var fsSkillLvl = Parent.Skills.First(x => x.Name == "Forest of Swords").Level;
        var sgSkillLvl = Parent.Skills.First(x => x.Name == "Shuhu's Gift").Level;
        var ssSkillLvl = Parent.Skills.First(x => x.Name == "Shard Sword").Level;

        //get modifier by level
        var dsMainAtk = FighterUtils.GetAbilityScaling(0.24, 0.4, dsSkillLvl);
        var dsMainHp = FighterUtils.GetAbilityScaling(0.60, 1, dsSkillLvl);

        var dsAdjAtk = FighterUtils.GetAbilityScaling(0.096, 0.16, dsSkillLvl);
        var dsAdjHp = FighterUtils.GetAbilityScaling(0.24, 0.4, dsSkillLvl);

        var forestAdjAtk = FighterUtils.GetBasicScaling(0.20, 0.40, fsSkillLvl);

        var sgAtk = FighterUtils.GetAbilityScaling(0.22, 0.44, sgSkillLvl);
        var sgHp = FighterUtils.GetAbilityScaling(0.55, 0.110, sgSkillLvl);

        hellscapeAppliedBuff = new AppliedBuff(Parent, null, this)
        {
            Type = Buff.BuffType.Buff,
            BaseDuration = 3,
            MaxStack = 1,
            CustomIconName = "Hellscape",
            Dispellable = false,
            Effects = new List<Effect> { new EffAllDamageBoost { Value = 0.4 } }
        };


        //=====================
        //Abilities
        //=====================


        //Passive
        shuhuGift = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.FollowUpAction,
            Name = "Shuhu's Gift",
            TargetType = TargetTypeEnm.Enemy,
            AdjacentTargets = AdjacentTargetsEnm.All,
            Available = SGAvailable,
            FollowUpPriority = PriorityEnm.Medium
        };
        shuHuMaxCnt = parent.Rank >= 6 ? 4 : 5; //4 stacks on 6 eidolon 
        var e6Mod = Parent.Rank >= 6
            ? $"+ (0.5 * {Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)})"
            : "";
        foreach (var proportion in new[] { 0.33000000030733645, 0.33000000030733645, 0.3400000003166497 })
        {
            shuhuGift.Events.Add(new DirectDamage(null, this, Parent)
            {
                CalculateValue = DamageFormula(new Formula()
                {
                    Expression =
                        $"(({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAttack)} * {sgAtk}) " +
                        $" + ({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)} * {sgHp} {e6Mod})) * {proportion} "
                })
            });
            shuhuGift.Events.Add(new ToughnessShred(null, this, Parent)
                { Value = 30 * proportion });

            shuhuGift.Events.Add(new EnergyGain(null, this, Parent)
                { Value = 10 * proportion, TargetUnit = Parent });
        }

        shuhuGift.Events.Add(new Healing(null, this, Parent)
            { TargetUnit = Parent, CalculateValue = CalculateSgHeal });
        shuhuGift.Events.Add(new MechanicValChg(null, this, Parent)
            { AbilityValue = shuhuGift, Value = -shuHuMaxCnt });
        Abilities.Add(shuhuGift);
        //Passive counter
        Mechanics.AddVal(shuhuGift);

        //basic

        Ability? shardSword;
        shardSword = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Basic,
            Name = "Shard Sword",
            AdjacentTargets = AdjacentTargetsEnm.None,
            SpGain = 1,
            Available = HellscapeNotActive
        };
        //dmg events
        foreach (var proportion in new[] { 0.5, 0.5 })
        {
            shardSword.Events.Add(new DirectDamage(null, this, Parent)
            {
                CalculateValue = DamageFormula(new Formula()
                {
                    Expression =
                        $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAttack)} * ({ssSkillLvl} * 0.1 + 0.4) * {proportion}"
                })
            });
            shardSword.Events.Add(new ToughnessShred(null, this, Parent)
                { Value = 30 * proportion });
            shardSword.Events.Add(new EnergyGain(null, this, Parent)
                { Value = 20 * proportion, TargetUnit = Parent });
        }

        Abilities.Add(shardSword);

        //Forest of swords
        var forestOfSwords = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Basic,
            Name = "Forest of Swords",
            AdjacentTargets = AdjacentTargetsEnm.Blast,
            Available = HellscapeActive
        };
        //dmg events
        forestOfSwords.Events.Add(new ResourceDrain(null, this, Parent)
        {
            ResType = Resource.ResourceType.HP,
            TargetType = TargetTypeEnm.Self,
            CanSetToZero = false,
            CalculateValue = CalculateForestSelfDmg,
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain
        });

        //main target 2 hits
        foreach (var proportion in new[] { 0.5, 0.5 })
        {
            //main target dmg
            forestOfSwords.Events.Add(new DirectDamage(null, this, Parent)
            {
                CalculateValue = DamageFormula(new Formula()
                {
                    Expression =
                        $"(({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAttack)} * ({fsSkillLvl} * 0.04 + 0.16 ) ) " +
                        $" + ({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)} * ({fsSkillLvl} * 0.1 + 0.4 )  ) ) * {proportion} "
                }),

                CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain
            });
            //main target thg
            forestMainTrgtHit = new ToughnessShred(null, this, Parent)
                { Value = 60 * proportion, CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain };
            forestOfSwords.Events.Add(forestMainTrgtHit);
            //energy
            forestOfSwords.Events.Add(new EnergyGain(null, this, Parent)
                { Value = 30 * proportion, TargetUnit = Parent });
        }

        //adjacent
        forestOfSwords.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = DamageFormula(new Formula()
            {
                Expression =
                    $"({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAttack)} * {forestAdjAtk} ) " +
                    $" + ({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)} * ({fsSkillLvl} *  0.04 + 0.16 )  )  "
            }),
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent
        });
        forestOfSwords.Events.Add(new ToughnessShred(null, this, Parent)
            { Value = 30, CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });


        Abilities.Add(forestOfSwords);


        deathSentence = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ultimate,
            Name = "Death Sentence",
            AdjacentTargets = AdjacentTargetsEnm.Blast,
            CostType = Resource.ResourceType.Energy,
            Cost = Parent.Stats.BaseMaxEnergy
        };
        //dmg events
        deathSentence.Events.Add(new ResourceDrain(null, this, Parent)
        {
            ResType = Resource.ResourceType.HP,
            TargetUnit = Parent,
            CanSetToZero = false,
            CalculateValue = CalculateDsSelfDmg,
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain
        });
        deathSentence.Events.Add(new ResourceGain(null, this, Parent)
        {
            ResType = Resource.ResourceType.HP,
            TargetUnit = Parent,
            CalculateValue = CalculateDsSelfHeal,
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain
        });
        deathSentence.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = DamageFormula(new Formula()
            {
                Expression =
                    $"({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAttack)} * {dsMainAtk}  ) " +
                    $" + ({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)} * {dsMainHp}  ) " +
                    $" + ({Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Fighter)}#{nameof(GetDsMechanic)} min  getDsMaxLostHp * {dsMainHp}  ) " +
                    ((Parent.Rank >= 1)
                        ? $" + ({Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Fighter)}#{nameof(GetDsMechanic)} min  getDsMaxLostHp * 1.5 ) "
                        : ""),
                Variables = new Dictionary<string, Formula.VarVal>()
                    { { nameof(getDsMaxLostHp), new Formula.VarVal() { ResFormula = getDsMaxLostHp() } } }
            }),

            CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain
        });
        deathSentence.Events.Add(new ToughnessShred(null, this, Parent)
            { Value = 60, CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain });
        deathSentence.Events.Add(new EnergyGain(null, this, Parent)
            { Value = 5, TargetUnit = Parent });
        deathSentence.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = DamageFormula(new Formula()
            {
                Expression =
                    $"({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAttack)} * {dsAdjAtk}  ) " +
                    $" + ({Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)} * {dsAdjHp}  ) " +
                    $" + ({Formula.DynamicTargetEnm.Attacker}#{nameof(Unit.Fighter)}#{nameof(GetDsMechanic)} min  getDsMaxLostHp * {dsAdjHp}  ) ",
                Variables = new Dictionary<string, Formula.VarVal>()
                    { { nameof(getDsMaxLostHp), new Formula.VarVal() { ResFormula = getDsMaxLostHp() } } }
            }),
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent
        });
        deathSentence.Events.Add(new ToughnessShred(null, this, Parent)
            { Value = 60, CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
        deathSentence.Events.Add(new MechanicValChg(null, this, Parent)
            { AbilityValue = deathSentence, CalculateValue = CalcResetDsCharge });

        Abilities.Add(deathSentence);
        Mechanics.AddVal(deathSentence);

        var karmaWind =
            //Karma Wind
            new Ability(this)
            {
                AbilityType = AbilityTypeEnm.Technique,
                Name = "Karma Wind",
                Cost = 1,
                CostType = Resource.ResourceType.TP,
                TargetType = TargetTypeEnm.Enemy,
                AdjacentTargets = AdjacentTargetsEnm.All
            };
        //dmg events
        karmaWind.Events.Add(new ToughnessShred(null, this, Parent)
            { OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue, Value = 60 });
        karmaWind.Events.Add(new ResourceDrain(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            ResType = Resource.ResourceType.HP,
            TargetType = TargetTypeEnm.Self,
            CanSetToZero = false,
            CalculateValue = CalculateKarmaSelfDmg,
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain
        });
        karmaWind.Events.Add(new DirectDamage(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            CalculateValue = FighterUtils.DamageFormula(new Formula
            {
                Expression = " BladeMaxHp * 0.4 ",
                Variables = new Dictionary<string, Formula.VarVal>
                {
                    {
                        "BladeMaxHp",
                        new()
                        {
                            ReplaceExpression =
                                $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)}"
                        }
                    }
                }
            })
        });

        Abilities.Add(karmaWind);


        Ability? Hellscape;

        //Hellscape
        Hellscape = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ability,
            Name = "Hellscape",
            Cost = 1,
            CostType = Resource.ResourceType.SP,
            TargetType = TargetTypeEnm.Self,
            AdjacentTargets = AdjacentTargetsEnm.None,
            EndTheTurn = false,
            Available = HellscapeNotActive
        };

        //dmg events
        Hellscape.Events.Add(new ResourceDrain(null, this, Parent)
        {
            ResType = Resource.ResourceType.HP,
            TargetType = TargetTypeEnm.Self,
            CanSetToZero = false,
            CalculateValue = CalculateHellscapeSelfDmg,
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain
        });

        Hellscape.Events.Add(new ApplyBuff(null, this, Parent)
        {
            TargetUnit = Parent,
            AppliedBuffToApply = hellscapeAppliedBuff
        });

        Abilities.Add(Hellscape);

//=====================


//=====================
//Ascended Traces
//=====================
        if (ATraces.HasFlag(ATracesEnm.A2))
            Parent.PassiveBuffs.Add(new PassiveBuff(Parent, this)
            {
                Effects = new List<Effect> { new EffIncomeHealingPrc { Value = 0.20 } },
                CustomIconName = "Traces\\A2",

                Target = Parent,
                ApplyConditions =
                [
                    new Condition
                    {
                        ConditionParam = Condition.ConditionCheckParam.HpPrc,
                        ConditionExpression = Condition.ConditionCheckExpression.EqualOrLess,
                        Value = 0.5
                    }
                ]
            });
        if (ATraces.HasFlag(ATracesEnm.A6))
            Parent.PassiveBuffs.Add(new PassiveBuff(Parent, this)
            {
                Effects = new List<Effect>
                {
                    new EffAbilityTypeBoost { Value = 0.20, AbilityType = AbilityTypeEnm.FollowUpAction }
                },
                Target = Parent
            });

        E4AppliedBuff = new AppliedBuff(Parent, null, this)
        {
            MaxStack = 2,
            Dispellable = false,
            Effects =
            [
                new EffMaxHpPrc
                {
                    Value = 0.2
                }
            ]
        };
    }

    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Destruction;

    /// <summary>
    /// forumla will use this
    /// </summary>
    /// <returns></returns>
    public double GetDsMechanic()
    {
        return Mechanics.Values[deathSentence];
    }
    private AppliedBuff E4AppliedBuff { get; }
//Blade constructor

/*
 * if 2+ turns left we dont need SP.
 * duration 2= turn+ next from bronya turn or double turn between support turn
 */
    protected override double WillSpend()
    {
        return (Parent.AppliedBuffs.FirstOrDefault(x => x.Reference == hellscapeAppliedBuff)?.DurationLeft ?? 0) > 2
            ? 0
            : 1;
    }


    protected override void DefaultFighter_HandleEvent(Event ent)
    {
        //if unit consume hp or got attack 
        if (ent.TargetUnit == Parent
            && (
                (ent is ResourceDrain && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealValue > 0)
                || (ent is DamageEventTemplate && ent.RealValue > 0)
            ) && (ent is DoTDamage || ent.ParentStep != lastDamageStep)
           )
        {
            //once per attack
            if (ent is not DoTDamage)
                lastDamageStep = ent.ParentStep;
            if (Mechanics.Values[shuhuGift] < shuHuMaxCnt)
                ent.ChildEvents.Add(new MechanicValChg(ent.ParentStep, this, Parent)
                    { Value = 1, AbilityValue = shuhuGift });

            var bladeHalfHp = Parent.GetMaxHp(ent) * 0.5;
            //if hp<=50% but was 50%+ then apply hp buff
            if (Parent.Rank >= 4 && Parent.GetRes(Resource.ResourceType.HP).ResVal <= bladeHalfHp &&
                Parent.GetRes(Resource.ResourceType.HP).ResVal + ent.RealValue > bladeHalfHp)
                ent.ChildEvents.Add(new ApplyBuff(ent.ParentStep, this, Parent)
                    { TargetUnit = Parent, AppliedBuffToApply = E4AppliedBuff });
        }

        if (ent.Reference == forestMainTrgtHit && ATraces.HasFlag(ATracesEnm.A4) &&
            ent.TargetUnit.GetRes(Resource.ResourceType.Toughness).ResVal == 0)
            ent.ChildEvents.Add(new Healing(ent.ParentStep, this, Parent)
                { TargetUnit = Parent, CalculateValue = CalculateForestHeal });
        //buffering Lost hp pull
        if (ent.TargetUnit == Parent
            && (
                (ent is ResourceDrain && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealValue > 0)
                || ent.IsDamageEvent
            ))
        {
            var dsValCharge = new MechanicValChg(ent.ParentStep, this, Parent)
                { Value = ent.RealValue, AbilityValue = deathSentence };
            ent.ChildEvents.Add(dsValCharge);
        }

        base.DefaultFighter_HandleEvent(ent);
    }


    private double? CalculateKarmaSelfDmg(Event ent)
    {
        return Parent.GetMaxHp(ent) * 0.2;
    }

    public double? CalculateForestSelfDmg(Event ent)
    {
        return Parent.GetMaxHp(ent) * 0.1;
    }


    private double? CalculateHellscapeSelfDmg(Event ent)
    {
        return Parent.GetMaxHp(ent) * 0.3;
    }

    public override string GetSpecialText()
    {
        return
            $"SG: {(int)Mechanics.Values[shuhuGift]:d}\\{(int)shuHuMaxCnt:d}  DS: {(int)Mechanics.Values[deathSentence]:d}\\{(int)getDsMaxLostHpForText():d}";
    }


    private bool SGAvailable()
    {
        return Mechanics.Values[shuhuGift] == shuHuMaxCnt;
    }


    private Formula getDsMaxLostHp()
    {
        return new Formula()
        {
            Expression =
                $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)} * 0.9"
        };
    }

    private double getDsMaxLostHpForText()
    {
        return Parent.GetMaxHp(null) * 0.9;
    }

//a4 if main target have 0 toughness then heal
    private double? CalculateForestHeal(Event ent)
    {
        return CalculateHealByBasicVal(Parent.GetMaxHp(ent) * 0.05 + 100, ent);
    }

    private double? CalculateDsSelfDmg(Event ent)
    {
        double setValue = 0;
        if (Parent.GetHpPrc(ent) > 0.5)
            setValue = Parent.GetRes(Resource.ResourceType.HP).ResVal - Parent.GetMaxHp(ent) / 2;
        return setValue;
    }

    private double? CalculateDsSelfHeal(Event ent)
    {
        double setValue = 0;
        if (Parent.GetHpPrc(ent) < 0.5)
            setValue = Parent.GetMaxHp(ent) / 2 - Parent.GetRes(Resource.ResourceType.HP).ResVal;
        return setValue;
    }

    private bool HellscapeActive()
    {
        return Parent.AppliedBuffs.Any(x => x.Reference == hellscapeAppliedBuff);
    }

    private bool HellscapeNotActive()
    {
        return Parent.AppliedBuffs.All(x => x.Reference != hellscapeAppliedBuff);
    }

    private double? CalculateSgHeal(Event ent)
    {
        return FighterUtils.CalculateHealByBasicVal(Parent.GetMaxHp(ent) * 0.25, ent);
    }

    private double? CalcResetDsCharge(Event ent)
    {
        return -Mechanics.Values[deathSentence];
    }
}