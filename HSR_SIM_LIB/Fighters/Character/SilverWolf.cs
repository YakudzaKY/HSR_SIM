using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character;

public class SilverWolf : DefaultFighter
{
    private Buff allowChangesDebuff;
    private Buff allowChangesDebuffAllDmgRes;


    private readonly double[]  alwChgAtkMods= {
        0.98,
        1.078,
        1.176,
        1.274,
        1.372,
        1.47,
        1.5925,
        1.715,
        1.8375,
        1.96,
        2.058,
        2.156,
        2.254,
        2.352,
        2.45,
    };

    private readonly double[] alwChgChncMods =
    {
        0.75,
        0.76,
        0.77,
        0.78,
        0.79,
        0.8,
        0.8125,
        0.825,
        0.8375,
        0.85,
        0.86,
        0.87,
        0.88,
        0.89,
        0.9,
    };

    private readonly double[]  alwChgValMods = 
    {
        0.075,
        0.0775,
        0.08,
        0.0825,
        0.085,
        0.0875,
        0.090625,
        0.09375,
        0.096875,
        0.1,
        0.1025,
        0.105,
        0.1075,
        0.11,
        0.1125,
    };

    private readonly double alwChgChnc;
    private readonly double alwChgAtk;
    private readonly double alwChgVal;
    private readonly int swSkillLvl;
    public SilverWolf(Unit parent) : base(parent)
    {
        Parent.Stats.BaseMaxEnergy = 110;
        alwChgChnc = alwChgChncMods[Parent.Skills.First(x => x.Name == "Allow Changes?").Level-1];
        alwChgAtk = alwChgAtkMods[Parent.Skills.First(x => x.Name == "Allow Changes?").Level-1];
        swSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "System Warning").Level;
        alwChgVal = alwChgValMods[Parent.Skills.First(x => x.Name == "Allow Changes?").Level-1];

        allowChangesDebuff = new Buff(Parent, null)
        {  Type = Buff.BuffType.Debuff, BaseDuration = 2, CustomIconName = "Allow_Changes ", Effects = new List<Effect>() { new EffWeaknessImpair() } ,EffectStackingType = Buff.EffectStackingTypeEnm.FullReplace};

        allowChangesDebuffAllDmgRes = new Buff(Parent, null)
            { Type = Buff.BuffType.Debuff, BaseDuration = 2, Effects = new List<Effect>() { new EffAllDamageResist() {Value = alwChgVal}} };
        //=====================
        //Abilities
        //=====================

        Ability ability;
        //Force Quit Program
        ability = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Technique,
            Name = "Force Quit Program",
            Cost = 1,
            CostType = Resource.ResourceType.TP,
            Element = Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.All,
            IgnoreWeakness = true
        };
        //dmg events
        ability.Events.Add(new DirectDamage(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            CalculateValue = CalculateFqpDmg,
            AbilityValue = ability
        });
        //shield break in this case going after skill dmg
        ability.Events.Add(new ResourceDrain(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            ResType = Resource.ResourceType.Toughness,
            Val = 60,
            AbilityValue = ability
        });

        Abilities.Add(ability);



        //System Warning
        Ability SystemWarning;
        SystemWarning = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "System Warning",
            Element = Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.None,
            SpGain = 1
        };
        //dmg events
        SystemWarning.Events.Add(new DirectDamage(null, this, Parent)
        { CalculateValue = CalculateBasicDmg, AbilityValue = SystemWarning });
        SystemWarning.Events.Add(new ToughnessShred(null, this, Parent) { Val = 30, AbilityValue = SystemWarning });
        SystemWarning.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 20, TargetUnit = Parent, AbilityValue = SystemWarning });

        Abilities.Add(SystemWarning);

        //Allow Changes?
        Ability AllowChanges;
        AllowChanges = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Ability,
            Name = "Allow Changes?",
            Element = Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.None,
            CostType = Resource.ResourceType.SP,
            Cost = 1
        };
        AllowChanges.Events.Add(new AttemptEffect(null, this, Parent) { BaseChance = alwChgChnc, BuffToApply = allowChangesDebuff });
        AllowChanges.Events.Add(new AttemptEffect(null, this, Parent) { BaseChance = 1, BuffToApply = allowChangesDebuffAllDmgRes });
        //dmg events
        AllowChanges.Events.Add(new DirectDamage(null, this, Parent)
        { CalculateValue = CalculateAbilityDmg, AbilityValue = AllowChanges });
        AllowChanges.Events.Add(new ToughnessShred(null, this, Parent) { Val = 60, AbilityValue = AllowChanges });
        AllowChanges.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 30, TargetUnit = Parent, AbilityValue = AllowChanges });
        Abilities.Add(AllowChanges);




        if (Parent.Rank >= 6)
            PassiveBuffs.Add(new PassiveBuff(Parent)
            {
                AppliedBuff = new Buff(Parent)
                { Effects = new List<Effect> { new EffAllDamageBoost { CalculateValue = CalculateE6 } } },
                Target = Parent,
                IsTargetCheck = true
            });
    }

    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Nihility;
    public sealed override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Quantum;
    public double? CalculateAbilityDmg(Event ent)
    {

        var attackPart = Parent.GetAttack(ent) * alwChgAtk;

        return FighterUtils.CalculateDmgByBasicVal(attackPart, ent);
    }

    public override void DefaultFighter_HandleEvent(Event ent)
    {
        //if unit consume hp or got attack then apply buff
        if (Parent.Rank >= 2 && Parent.IsAlive && ent is UnitEnteringBattle)
            //if enemy enter combat need debuff
            if (Parent.Enemies.Any(x => x == ent.TargetUnit))
            {
                ApplyBuff newEvent = new(ent.ParentStep, this, Parent)
                {
                    TargetUnit = ent.TargetUnit,
                    BuffToApply = new Buff(Parent)
                    {
                        Type = Buff.BuffType.Debuff,
                        Effects = new List<Effect> { new EffEffectResPrc { Value = -0.20 } }
                    }
                };

                ent.ChildEvents.Add(newEvent);
            }

        if (ent is ApplyBuff ab && ent.SourceUnit == Parent)
        {
            //if allow changes landed then choose element
            if (ab.BuffToApply == allowChangesDebuff)
            {
                
            }
        }
        //TODO handle enemy break shield by any of party members(A6?)

        base.DefaultFighter_HandleEvent(ent);
    }


    public double? CalculateFqpDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(ent) * 0.8, ent);
    }

    //50-110
    public double? CalculateBasicDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(
            Parent.GetAttack(ent) * (0.4 + swSkillLvl * 0.1),
            ent);
    }

    //get 0.2 AllDmg per debuff  on target
    public static double? CalculateE6(Event ent)
    {
        double maxDebuffs = 5;
        double debuffs = 0;


        debuffs += ent.TargetUnit.Buffs.Count(x => x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);
        if (debuffs > maxDebuffs) debuffs = maxDebuffs;

        return debuffs * 0.2;
    }
}