using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Fighters.Character;

public class Luocha : DefaultFighter
{
    private readonly double[]  coLfAtkMods = 
    {
         0.12 ,  0.1275, 0.135,  0.1425 ,  0.15 ,  0.156,  0.162 ,  0.168 ,
         0.174 ,  0.18 ,  0.186 , 0.192 
    };

    private readonly double[]    coLfFixMods =
    {
        60 ,  96 ,  123 ,  150,  168 , 186 ,  199.5, 213 ,
       226.5 , 240 ,  253.5 ,  267 
    };

    private readonly Ability cycleOfLife;
    private readonly double cycleOfLifeMaxCnt = 2;

    private readonly double[]  deathWishMods =
    {
         1.20 ,  1.28 ,  1.36 ,  1.44 ,  1.52 ,  1.60 ,  1.70,  1.80 ,
         1.90 ,  2 , 2.08 ,  2.16
    };

    private readonly double[]  poAfAtkMods = 
    {
         0.40 ,  0.425 ,  0.45 ,  0.475 ,  0.50 ,  0.52,  0.54 ,  0.56 ,
         0.58 ,  0.60 ,  0.62 , 0.64

    };

    private readonly double[]  poAfFixMods =
    {
         200 ,  320 ,  410 ,  500 ,  560 ,  620 ,  665 ,  710 ,
         755 , 800 ,  845 ,  890 
    };

    private readonly Ability prayerOfAbyssFlower;
    private readonly Ability prayerOfAbyssFlowerAuto;
    private readonly Buff triggerCdBuff;
    private readonly Buff e2ShieldBuff;
    private readonly Ability ultimateAbility;
    private readonly Buff uniqueBuff;

    private List<Unit> trackedUnits = new();

    private double coLfAtk;
    private double coLfFix;
    private double deathWish;
    private double poAfAtk;
    private double poAfFix;
    private int coLLvl;
    private int dWLvl;
    private int totALvl;
    private int poALvl;

    public Luocha(Unit parent) : base(parent)
    {
        Parent.Stats.BaseMaxEnergy = 100;

        coLLvl = Parent.Skills.First(x => x.Name == "Cycle of Life")!.Level;
        dWLvl= Parent.Skills.First(x => x.Name == "Death Wish")!.Level;
        totALvl=Parent.Skills.First(x => x.Name == "Thorns of the Abyss")!.Level;
        poALvl = Parent.Skills.First(x => x.Name == "Prayer of Abyss Flower")!.Level;

        coLfAtk = coLfAtkMods[coLLvl-1];
        coLfFix = coLfFixMods[coLLvl-1];
        deathWish = deathWishMods[dWLvl-1];
        poAfAtk= poAfAtkMods[poALvl-1];
        poAfFix= poAfFixMods[poALvl-1];

        uniqueBuff = new Buff(Parent)
        {
            Type = Buff.BuffType.Buff,
            BaseDuration = 2,
            MaxStack = 1,
            Dispellable = false,
            CustomIconName = "Icon_Abyss_Flower"
        };

        triggerCdBuff = new Buff(Parent)
        {
            Type = Buff.BuffType.Debuff,
            BaseDuration = 2,
            MaxStack = 1,
            Dispellable = false,
            CustomIconName = "Abyss_Flower_CD"
        };

        //=====================
        //Abilities
        //=====================
        //Cycle of life
        cycleOfLife = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.FollowUpAction,
            Name = "Cycle of life",
            Element = Element,
            Available = ColAvailable,
            Priority = PriorityEnm.Low,
            TargetType = TargetTypeEnm.Self
        };
        cycleOfLife.Events.Add(new MechanicValChg(null, this, Parent)
        { TargetUnit = Parent, AbilityValue = cycleOfLife, Val = -cycleOfLifeMaxCnt });
        cycleOfLife.Events.Add(new ApplyBuff(null, this, Parent)
        {
            AbilityValue = cycleOfLife,
            TargetUnit = Parent,
            BuffToApply = uniqueBuff
        });
        Mechanics.AddVal(cycleOfLife);
        Abilities.Add(cycleOfLife);

        //Prayer of Abyss Flower
        prayerOfAbyssFlower = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ability,
            Name = "Prayer of Abyss Flower",
            Element = Element,
            TargetType = TargetTypeEnm.Friend,
            CostType = Resource.ResourceType.SP,
            Cost = 1
        };
        if (Atraces.HasFlag(ATracesEnm.A2))
            prayerOfAbyssFlower.Events.Add(new DispelShit(null, this, Parent)
            { AbilityValue = prayerOfAbyssFlower });

        prayerOfAbyssFlower.Events.Add(new Healing(null, this, Parent)
        { CalculateValue = CalculatePrayerOfAbyssFlower, AbilityValue = prayerOfAbyssFlower });
        prayerOfAbyssFlower.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 30, TargetUnit = Parent, AbilityValue = prayerOfAbyssFlower });
        Abilities.Add(prayerOfAbyssFlower);


        //Prayer of Abyss Flower(auto)
        prayerOfAbyssFlowerAuto = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.FollowUpAction,
            Name = "Prayer of Abyss Flower (auto)",
            Element = Element,
            Available = PoAFAvailable,
            Priority = PriorityEnm.High,
            TargetType = TargetTypeEnm.Friend,
        };
        if (Atraces.HasFlag(ATracesEnm.A2))
            prayerOfAbyssFlowerAuto.Events.Add(new DispelShit(null, this, Parent)
            { CalculateTargets = CalcFollowPoAFTarget, AbilityValue = prayerOfAbyssFlowerAuto });
        prayerOfAbyssFlowerAuto.Events.Add(new Healing(null, this, Parent)
        {
            CalculateTargets = CalcFollowPoAFTarget,
            CalculateValue = CalculatePrayerOfAbyssFlower,
            AbilityValue = prayerOfAbyssFlowerAuto
        });
        prayerOfAbyssFlowerAuto.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 30, TargetUnit = Parent, AbilityValue = prayerOfAbyssFlowerAuto });
        prayerOfAbyssFlowerAuto.Events.Add(new ApplyBuff(null, this, Parent)
        {
            AbilityValue = prayerOfAbyssFlowerAuto,
            TargetUnit = Parent,
            BuffToApply = triggerCdBuff
        });
        Abilities.Add(prayerOfAbyssFlowerAuto);

        e2ShieldBuff = new Buff(Parent, null)
        { Effects = new List<Effect>() { new EffShield() { CalculateValue = CalculateE2Shield } } };

        //basic attack
        Ability ThornsoftheAbyss;
        ThornsoftheAbyss = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Basic,
            Name = "Thorns of the Abyss",
            Element = Element,
            AdjacentTargets = AdjacentTargetsEnm.None,
            SpGain = 1
        };

        foreach (double proportion in new[] { 0.3, 0.3, 0.4 })
        {
            ThornsoftheAbyss.Events.Add(new DirectDamage(null, this, Parent)
            { CalculateValue = CalculateBasicDmg, AbilityValue = ThornsoftheAbyss, CalculateProportion = proportion });
            ThornsoftheAbyss.Events.Add(new ToughnessShred(null, this, Parent)
            { Val = 30, AbilityValue = ThornsoftheAbyss, CalculateProportion = proportion });
            ThornsoftheAbyss.Events.Add(new EnergyGain(null, this, Parent)
            { Val = 20, TargetUnit = Parent, AbilityValue = ThornsoftheAbyss, CalculateProportion = proportion });
        }


        Abilities.Add(ThornsoftheAbyss);


        // ULTIMATE
        ultimateAbility = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ultimate,
            Name = "Death Wish",
            AdjacentTargets = AdjacentTargetsEnm.All,
            Available = UltimateAvailable,
            Priority = PriorityEnm.Ultimate,
            EndTheTurn = false,
            CostType = Resource.ResourceType.Energy,
            Cost = Parent.Stats.BaseMaxEnergy
        };
        //dmg events
        //E6
        if (Parent.Rank >= 6)
            ultimateAbility.Events.Add(new ApplyBuff(null, this, Parent)
            { BuffToApply = new Buff(Parent) { CustomIconName = "Ability_Death_Wish", BaseDuration = 2, Type = Buff.BuffType.Debuff, Effects = new List<Effect>() { new EffAllDamageResist() { Value = 0.2 } } }, AbilityValue = ultimateAbility, CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
        ultimateAbility.Events.Add(new DispelGood(null, this, Parent)
        { AbilityValue = ultimateAbility, CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
        ultimateAbility.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = CalculateUltimateDmg,
            AbilityValue = ultimateAbility,
            CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent
        });
        ultimateAbility.Events.Add(new ToughnessShred(null, this, Parent)
        { Val = 60, AbilityValue = ultimateAbility, CurrentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
        ultimateAbility.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 5, TargetUnit = Parent, AbilityValue = ultimateAbility });
        Abilities.Add(ultimateAbility);


        //Mercy of a Fool
        var ability = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Technique,
            Name = "Mercy of a Fool",
            Cost = 1,
            CostType = Resource.ResourceType.TP,
            Element = Element
        };
        ability.Events.Add(new MechanicValChg(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            TargetUnit = Parent,
            Val = cycleOfLifeMaxCnt,
            AbilityValue = cycleOfLife
        }); //AbilityValue for Cycle of life
        Abilities.Add(ability);

        //E1
        if (Parent.Rank >= 1)
            //CoL buffs
            ConditionBuffs.Add(new ConditionBuff(Parent)
            {
                AppliedBuff = new Buff(Parent)
                {
                    Effects = new List<Effect> { new EffAtkPrc { Value = 0.2 } },
                    CustomIconName = uniqueBuff.CustomIconName
                },
                Target = Parent.ParentTeam,
                Condition = new ConditionBuff.ConditionRec { ConditionAvailable = ColBuffAvailable }
            });


        //A6
        if (Atraces.HasFlag(ATracesEnm.A6))
            DebuffResists.Add(new DebuffResist { Debuff = typeof(EffCrowControl), ResistVal = 0.7 });
        //E2
        if (Parent.Rank >= 2)
            PassiveBuffs.Add(new PassiveBuff(Parent)
            {
                AppliedBuff = new Buff(Parent)
                { Effects = new List<Effect> { new EffOutgoingHealingPrc() { CalculateValue = CalculateE2 } } },
                Target = Parent,
                IsTargetCheck = true
            });

        //E4
        if (Parent.Rank >= 4)
            //CoL buffs
            ConditionBuffs.Add(new ConditionBuff(Parent)
            {
                AppliedBuff = new Buff(Parent)
                {
                    Type = Buff.BuffType.Debuff,
                    Effects = new List<Effect> { new EffAllDamageBoost() { Value = -0.12 } },
                    CustomIconName = uniqueBuff.CustomIconName
                },
                Target = TargetTypeEnm.Enemy,
                Condition = new ConditionBuff.ConditionRec { ConditionAvailable = ColBuffAvailable }
            });
    }

    public double? CalculateE2Shield(Event ent)
    {

        return FighterUtils.CalculateShield(Parent.GetAttack(ent) * 0.18 + 240, ent, Parent);
    }

    public double? CalculateE2(Event ent)
    {
        double res = 0;

        if ((ent.AbilityValue == prayerOfAbyssFlowerAuto || ent.AbilityValue == prayerOfAbyssFlower) && ent.TargetUnit.GetHpPrc(ent) < 0.5)
        {
            res = 0.3;
        }
        return res;
    }

    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Abundance;
    public sealed override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Imaginary;

    //If unit hp<=50% for Luocha follow up heals
    private bool UnitAtLowHpForAuto(Unit unit)
    {
        return unit.GetRes(Resource.ResourceType.HP).ResVal / unit.GetMaxHp(null) <= 0.5;
    }

    public double? CalcCoLHealing(Event ent)
    {
        return FighterUtils.CalculateHealByBasicVal(Parent.GetAttack(ent) * coLfAtk + coLfFix,
            ent);
    }

    public double? CalcCoLHealingParty(Event ent)
    {
        return FighterUtils.CalculateHealByBasicVal(Parent.GetAttack(ent) * 0.07 + 93, ent);
    }


    public override void DefaultFighter_HandleEvent(Event ent)
    {
        //if unit consume hp or got attack then apply buff

        if (ent is FinishCombat)
        {
            trackedUnits = new List<Unit>();
        }
        else if (ent is ResourceDrain or DamageEventTemplate or Healing or ResourceGain)
        {
            CheckAndAddTarget(ent.TargetUnit);
        }
        else if (ent is ExecuteAbilityFinish && Parent.Friends.Any(x => x == ent.SourceUnit) &&
                 ent.AbilityValue.Attack && ent.SourceUnit.IsAlive && ColBuffAvailable())
        {
            ent.ChildEvents.Add(
                new Healing(ent.ParentStep, this,
                        Parent) //will put source unit coz Output healing calc will be calculated by target unit
                {
                    AbilityValue = ent.AbilityValue,
                    TargetUnit = ent.SourceUnit,
                    CalculateValue = CalcCoLHealing
                });

            if (Atraces.HasFlag(ATracesEnm.A4))
                //for all friends except attacker unit
                foreach (var unit in ent.SourceUnit.Friends.Where(x => x.IsAlive && x != ent.SourceUnit))
                    ent.ChildEvents.Add(
                        new Healing(ent.ParentStep, this,
                                Parent) //will put source unit coz Output healing calc will be calculated by target unit
                        {
                            AbilityValue = ent.AbilityValue,
                            TargetUnit = unit,
                            CalculateValue = CalcCoLHealingParty
                        });
        }

        if (Parent.Rank >= 2 && ent is ExecuteAbilityStart && ent.SourceUnit == Parent && (ent.AbilityValue == prayerOfAbyssFlowerAuto || ent.AbilityValue == prayerOfAbyssFlower))
        {
            //E2 shield part
            if (Parent.Rank >= 2 && ent.TargetUnit.GetHpPrc(ent) >= 0.5)
                ent.ChildEvents.Add(new ApplyBuff(ent.ParentStep, this, ent.SourceUnit)
                { TargetUnit = ent.TargetUnit, AbilityValue = ent.AbilityValue, BuffToApply = e2ShieldBuff });
        }


        base.DefaultFighter_HandleEvent(ent);
    }


    //check unit alive and hp status and add/or remove
    private void CheckAndAddTarget(Unit entTargetUnit)
    {
        //if friend have low hp then add to track
        if (Parent.Friends.Any(x => x == entTargetUnit))
        {
            if (trackedUnits.All(x => x != entTargetUnit) && entTargetUnit.IsAlive && UnitAtLowHpForAuto(entTargetUnit))
            {
                trackedUnits.Add(entTargetUnit);
                Parent.ParentTeam.ParentSim.Parent.LogDebug($"{Parent.Name} add {entTargetUnit.Name} to track list");
            }
            else if (trackedUnits.Any(x => x == entTargetUnit) &&
                     (!UnitAtLowHpForAuto(entTargetUnit) || !entTargetUnit.IsAlive))
            {
                //remove high hp unit from track
                trackedUnits.Remove(entTargetUnit);
                Parent.ParentTeam.ParentSim.Parent.LogDebug(
                    $"{Parent.Name} remove {entTargetUnit.Name} from track list");
            }
        }
    }


    public override void DefaultFighter_HandleStep(Step step)
    {
        //check if friendly unit do action
        if (step.StepType is Step.StepTypeEnm.ExecuteAbilityFromQueue or Step.StepTypeEnm.UnitFollowUpAction
            or Step.StepTypeEnm.UnitTurnContinued or Step.StepTypeEnm.UnitTurnStarted)
        {
            CheckAndAddTarget(step.Actor);
            //also check self  stacks
            if (!ColBuffAvailable() & (step.Actor == Parent) && (step.ActorAbility == ultimateAbility ||
                                                                 step.ActorAbility == prayerOfAbyssFlowerAuto ||
                                                                 step.ActorAbility == prayerOfAbyssFlower))
                step.Events.Add(new MechanicValChg(step, this, Parent)
                { TargetUnit = Parent, Val = 1, AbilityValue = cycleOfLife });
        }


        base.DefaultFighter_HandleStep(step);
    }


    public override string GetSpecialText()
    {
        return $"CoL: {(int)Mechanics.Values[cycleOfLife]:d}\\{(int)cycleOfLifeMaxCnt:d}";
    }

    public bool ColAvailable()
    {
        return Mechanics.Values[cycleOfLife] >= cycleOfLifeMaxCnt;
    }

    public bool PoAFAvailable()
    {
        return Parent.Buffs.All(x => x.Reference != triggerCdBuff) && (trackedUnits?.Any(x => x.IsAlive) ?? false);
    }

    public bool ColBuffAvailable()
    {
        return Parent.Buffs.Any(x => x.Reference == uniqueBuff);
    }

    public double? CalculateBasicDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(
            Parent.GetAttack(null) * (0.4 + totALvl * 0.1),
            ent);
    }


    //50-110
    public double? CalculatePrayerOfAbyssFlower(Event ent)
    {
        return FighterUtils.CalculateHealByBasicVal(Parent.GetAttack(null) * poAfAtk + poAfFix,
            ent);
    }


    public double? CalculateUltimateDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(null) * deathWish, ent);
    }

    //get targets for auto heal. One target for Luocha
    public IEnumerable<Unit> CalcFollowPoAFTarget()
    {
        IEnumerable<Unit> targets = new List<Unit> { trackedUnits.First() };

        return targets;
    }
}