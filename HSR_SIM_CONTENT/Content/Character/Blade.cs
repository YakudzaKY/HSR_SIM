using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;


namespace HSR_SIM_CONTENT.Content.Character;

public class Blade : DefaultFighter
{
    private readonly Ability deathSentence; //ultimate


    private readonly Event forestMainTrgtHit; //last hit in main target by this ability(for event handlers)
    private readonly Ability forestOfSwords; //enchanced basic
    private readonly Buff hellscapeBuff; //E buff


    private readonly Ability shuhuGift; //passive ability

    private readonly double shuHuMaxCnt; // passive max counter

    //some modifiers
    private readonly double dsAdjAtk;
    private readonly double dsAdjHp;
    private readonly double dsMainAtk;
    private readonly double dsMainHp;

    private readonly int dsSkillLvl;
    private readonly double forestAdjAtk;
    private readonly int fsSkillLvl;
    private Step lastDamageStep;
    private readonly double sgAtk;
    private readonly double sgHp;
    private readonly int sgSkillLvl;

    private readonly int ssSkillLvl;

    public Blade(Unit parent) : base(parent)
    {
        Parent.Stats.BaseMaxEnergy = 130;

        //load lvl
        dsSkillLvl = Parent.Skills.First(x => x.Name == "Death Sentence").Level;
        fsSkillLvl = Parent.Skills.First(x => x.Name == "Forest of Swords").Level;
        sgSkillLvl = Parent.Skills.First(x => x.Name == "Shuhu's Gift").Level;
        ssSkillLvl = Parent.Skills.First(x => x.Name == "Shard Sword").Level;

        //get modifier by level
        dsMainAtk = FighterUtils.GetAbilityScaling(0.24, 0.4, dsSkillLvl);
        dsMainHp = FighterUtils.GetAbilityScaling(0.60, 1, dsSkillLvl);

        dsAdjAtk = FighterUtils.GetAbilityScaling(0.096, 0.16, dsSkillLvl);
        dsAdjHp = FighterUtils.GetAbilityScaling(0.24, 0.4, dsSkillLvl);

        forestAdjAtk = FighterUtils.GetBasicScaling(0.20, 0.40, fsSkillLvl);

        sgAtk = FighterUtils.GetAbilityScaling(0.22, 0.44, sgSkillLvl);
        sgHp = FighterUtils.GetAbilityScaling(0.55, 0.110, sgSkillLvl);

        hellscapeBuff = new Buff(Parent)
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
            Element = Element,
            TargetType = TargetTypeEnm.Enemy,
            AdjacentTargets = AdjacentTargetsEnm.All,
            Available = SGAvailable,
            FollowUpPriority = PriorityEnm.Medium
        };
        shuHuMaxCnt = parent.Rank >= 6 ? 4 : 5; //4 stacks on 6 eidolon 
        foreach (var proportion in new[] { 0.33000000030733645, 0.33000000030733645, 0.3400000003166497 })

        {
            shuhuGift.Events.Add(new DirectDamage(null, this, Parent)
            {
                CalculateValue = CalculateSGDmg,
                CalculateProportion = proportion
            });
            shuhuGift.Events.Add(new ToughnessShred(null, this, Parent)
                { Val = 30, CalculateProportion = proportion });

            shuhuGift.Events.Add(new EnergyGain(null, this, Parent)
                { Val = 10, TargetUnit = Parent, CalculateProportion = proportion });
        }

        shuhuGift.Events.Add(new Healing(null, this, Parent)
            { TargetUnit = Parent, CalculateValue = CalculateSgHeal });
        shuhuGift.Events.Add(new MechanicValChg(null, this, Parent)
            { AbilityValue = shuhuGift, Val = -shuHuMaxCnt });
        Abilities.Add(shuhuGift);
        //Passive counter
        Mechanics.AddVal(shuhuGift);

        //basic

        Ability shardSword;
        shardSword = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Basic,
            Name = "Shard Sword",
            Element = Element,
            AdjacentTargets = AdjacentTargetsEnm.None,
            SpGain = 1,
            Available = HellscapeNotActive
        };
        //dmg events
        foreach (var proportion in new[] { 0.5, 0.5 })
        {
            shardSword.Events.Add(new DirectDamage(null, this, Parent)
                { CalculateValue = CalculateBasicDmg, CalculateProportion = proportion });
            shardSword.Events.Add(new ToughnessShred(null, this, Parent)
                { Val = 30, CalculateProportion = proportion });
            shardSword.Events.Add(new EnergyGain(null, this, Parent)
                { Val = 20, TargetUnit = Parent, CalculateProportion = proportion });
        }

        Abilities.Add(shardSword);

        //Forest of swords
        forestOfSwords = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Basic,
            Name = "Forest of Swords",
            Element = Element,
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
                CalculateValue = CalculateForestDmg,
                CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain,
                CalculateProportion = proportion
            });
            //main target thg
            forestMainTrgtHit = new ToughnessShred(null, this, Parent)
                { Val = 60, CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain, CalculateProportion = proportion };
            forestOfSwords.Events.Add(forestMainTrgtHit);
            //energy
            forestOfSwords.Events.Add(new EnergyGain(null, this, Parent)
                { Val = 30, TargetUnit = Parent, CalculateProportion = proportion });
        }

        //adjacent
        forestOfSwords.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = CalculateForestDmgAdj,
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent
        });
        forestOfSwords.Events.Add(new ToughnessShred(null, this, Parent)
            { Val = 30, CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });


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
            CalculateValue = CalculateDsDmg,
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain
        });
        deathSentence.Events.Add(new ToughnessShred(null, this, Parent)
            { Val = 60, CurrentTargetType = AbilityCurrentTargetEnm.AbilityMain });
        deathSentence.Events.Add(new EnergyGain(null, this, Parent)
            { Val = 5, TargetUnit = Parent });
        deathSentence.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = CalculateDstDmgAdj,
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent
        });
        deathSentence.Events.Add(new ToughnessShred(null, this, Parent)
            { Val = 60, CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
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
                Element = Element,
                TargetType = TargetTypeEnm.Enemy,
                AdjacentTargets = AdjacentTargetsEnm.All
            };
        //dmg events
        karmaWind.Events.Add(new ToughnessShred(null, this, Parent)
            { OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue, Val = 60 });
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
            CalculateValue = CalculateKarmaDmg
        });

        Abilities.Add(karmaWind);


        Ability Hellscape;
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
            BuffToApply = hellscapeBuff
        });

        Abilities.Add(Hellscape);

        //=====================


        //=====================
        //Ascended Traces
        //=====================
        if (Atraces.HasFlag(ATracesEnm.A2))
            ConditionBuffs.Add(new ConditionBuff(Parent)
            {
                AppliedBuff = new Buff(Parent)
                {
                    Effects = new List<Effect> { new EffIncomeHealingPrc { Value = 0.20 } },
                    CustomIconName = "Traces\\A2"
                },
                Target = Parent,
                Condition = new ConditionBuff.ConditionRec
                {
                    CondtionParam = ConditionBuff.ConditionCheckParam.HPPrc,
                    CondtionExpression = ConditionBuff.ConditionCheckExpression.EqualOrLess,
                    Value = 0.5
                }
            });
        if (Atraces.HasFlag(ATracesEnm.A6))
            PassiveBuffs.Add(new PassiveBuff(Parent)
            {
                AppliedBuff = new Buff(Parent)
                {
                    Effects = new List<Effect>
                    {
                        new EffAbilityTypeBoost { Value = 0.20, AbilityType = AbilityTypeEnm.FollowUpAction }
                    }
                },
                Target = Parent
            });

        e4Buff = new Buff(Parent)
            { MaxStack = 2, Dispellable = false, Effects = new List<Effect> { new EffMaxHpPrc { Value = 0.2 } } };
    }

    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Destruction;
    public sealed override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Wind;

    public Buff e4Buff { get; set; }
    //Blade constructor

    /*
     * if 2+ turns left we dont need SP.
     * duration 2= turn+ next from bronya turn or double turn between support turn
     */
    public override double WillSpend()
    {
        return (Parent.Buffs.FirstOrDefault(x => x.Reference == hellscapeBuff)?.DurationLeft ?? 0) > 2 ? 0 : 1;
    }



    public override void DefaultFighter_HandleEventAfter(Event ent)
    {
        //if unit consume hp or got attack 
        if (ent.TargetUnit == Parent
            && (
                (ent is ResourceDrain && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal > 0)
                || (ent is DamageEventTemplate && ent.RealVal > 0)
            ) && (ent is DoTDamage || ent.ParentStep != lastDamageStep)
           )
        {
            //once per attack
            if (ent is not DoTDamage)
                lastDamageStep = ent.ParentStep;
            if (Mechanics.Values[shuhuGift] < shuHuMaxCnt)
                ent.ChildEvents.Add(new MechanicValChg(ent.ParentStep, this, Parent)
                    {  Val = 1, AbilityValue = shuhuGift });

            var bladeHalfHp = Parent.GetMaxHp(ent) * 0.5;
            //if hp<=50% but was 50%+ then apply hp buff
            if (Parent.Rank >= 4 && Parent.GetRes(Resource.ResourceType.HP).ResVal <= bladeHalfHp &&
                Parent.GetRes(Resource.ResourceType.HP).ResVal + ent.RealVal > bladeHalfHp)
                ent.ChildEvents.Add(new ApplyBuff(ent.ParentStep, this, Parent)
                    { TargetUnit = Parent, BuffToApply = e4Buff });
        }

        if (ent.Reference == forestMainTrgtHit && Atraces.HasFlag(ATracesEnm.A4) &&
            ent.TargetUnit.GetRes(Resource.ResourceType.Toughness).ResVal == 0)
            ent.ChildEvents.Add(new Healing(ent.ParentStep, this, Parent)
                { TargetUnit = Parent, CalculateValue = CalculateForestHeal });
        //buffering Lost hp pull
        if (ent.TargetUnit == Parent
            && (
                (ent is ResourceDrain && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal > 0)
                || ent.IsDamageEvent
            ))
        {
            var dsValCharge = new MechanicValChg(ent.ParentStep, this, Parent)
                { Val = ent.RealVal, AbilityValue = deathSentence };
            ent.ChildEvents.Add(dsValCharge);
        }

        base.DefaultFighter_HandleEventAfter(ent);
    }


    public double? CalculateKarmaSelfDmg(Event ent)
    {
        return Parent.GetMaxHp(ent) * 0.2;
    }

    public double? CalculateForestSelfDmg(Event ent)
    {
        return Parent.GetMaxHp(ent) * 0.1;
    }


    public double? CalculateKarmaDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(Parent.GetMaxHp(ent) * 0.4, ent);
    }

    private double getSgBasicDmg(Event ent)
    {
        var attackPart = Parent.GetAttack(ent) * sgAtk;
        var maxHpPart = Parent.GetMaxHp(ent) * sgHp + (Parent.Rank >= 6 ? 0.5 * Parent.GetMaxHp(ent) : 0);
        return attackPart + maxHpPart;
    }

    public double? CalculateSGDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(getSgBasicDmg(ent), ent);
    }


    public double? CalculateHellscapeSelfDmg(Event ent)
    {
        return Parent.GetMaxHp(ent) * 0.3;
    }

    public override string GetSpecialText()
    {
        return
            $"SG: {(int)Mechanics.Values[shuhuGift]:d}\\{(int)shuHuMaxCnt:d}  DS: {(int)Mechanics.Values[deathSentence]:d}\\{(int)getDsMaxLostHp(null):d}";
    }


    public bool SGAvailable()
    {
        return Mechanics.Values[shuhuGift] == shuHuMaxCnt;
    }

    //50-110
    public double? CalculateBasicDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(
            Parent.GetAttack(null) * (0.4 + ssSkillLvl * 0.1),
            ent);
    }

    public double getDsMaxLostHp(Event ent)
    {
        return Parent.GetMaxHp(ent) * 0.9;
    }

    //damage for ULTI main target
    public double? CalculateDsDmg(Event ent)
    {
        var attackPart = Parent.GetAttack(ent) * dsMainAtk;
        var maxHpPart = Parent.GetMaxHp(ent) * dsMainHp;
        var hpLossPart = Math.Min(Mechanics.Values[deathSentence], getDsMaxLostHp(ent)) * dsMainHp;
        //E1 ultimate buff
        if (Parent.Rank >= 1)
            hpLossPart += Math.Min(Mechanics.Values[deathSentence], getDsMaxLostHp(ent)) * 1.5;
        return FighterUtils.CalculateDmgByBasicVal(attackPart + maxHpPart + hpLossPart, ent);
    }

    //damage for ULTI adjacent target
    public double? CalculateDstDmgAdj(Event ent)
    {
        var attackPart = Parent.GetAttack(ent) * dsAdjAtk;
        var maxHpPart = Parent.GetMaxHp(ent) * dsAdjHp;
        var hpLossPart = Math.Min(Mechanics.Values[deathSentence], getDsMaxLostHp(ent)) * dsAdjHp;
        return FighterUtils.CalculateDmgByBasicVal(attackPart + maxHpPart + hpLossPart, ent);
    }


    //damage for main target
    public double? CalculateForestDmg(Event ent)
    {
        var attackPart = Parent.GetAttack(ent) * (0.16 +
                                                  fsSkillLvl * 0.04);
        var maxHpPart = Parent.GetMaxHp(ent) * (0.4 + fsSkillLvl * 0.1);
        return FighterUtils.CalculateDmgByBasicVal(attackPart + maxHpPart, ent);
    }

    //damage for adjacent target
    public double? CalculateForestDmgAdj(Event ent)
    {
        var attackPart = Parent.GetAttack(ent) * forestAdjAtk;
        var maxHpPart = Parent.GetMaxHp(ent) * (0.16 + fsSkillLvl * 0.04);
        return FighterUtils.CalculateDmgByBasicVal(attackPart + maxHpPart, ent);
    }

    //a4 if main target have 0 toughness then heal
    public double? CalculateForestHeal(Event ent)
    {
        return FighterUtils.CalculateHealByBasicVal(Parent.GetMaxHp(ent) * 0.05 + 100, ent);
    }

    public double? CalculateDsSelfDmg(Event ent)
    {
        double setValue = 0;
        if (Parent.GetHpPrc(ent) > 0.5)
            setValue = Parent.GetRes(Resource.ResourceType.HP).ResVal - Parent.GetMaxHp(ent) / 2;
        return setValue;
    }

    public double? CalculateDsSelfHeal(Event ent)
    {
        double setValue = 0;
        if (Parent.GetHpPrc(ent) < 0.5)
            setValue = Parent.GetMaxHp(ent) / 2 - Parent.GetRes(Resource.ResourceType.HP).ResVal;
        return setValue;
    }

    public bool HellscapeActive()
    {
        return Parent.Buffs.Any(x => x.Reference == hellscapeBuff);
    }

    public bool HellscapeNotActive()
    {
        return Parent.Buffs.All(x => x.Reference != hellscapeBuff);
    }

    public double? CalculateSgHeal(Event ent)
    {
        return FighterUtils.CalculateHealByBasicVal(Parent.GetMaxHp(ent) * 0.25, ent);
    }

    private double? CalcResetDsCharge(Event ent)
    {
        return -Mechanics.Values[deathSentence];
    }
}