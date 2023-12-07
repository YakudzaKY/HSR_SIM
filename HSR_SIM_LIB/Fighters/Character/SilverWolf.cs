using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;
using HSR_SIM_LIB.Utils.Utils;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Fighters.Character;

public class SilverWolf : DefaultFighter
{
    private Buff allowChangesDebuff;
    private Buff ultDefDebuff;
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

    private readonly double[]  ultDmgMods = 
    {
        2.28,
        2.432,
        2.584,
        2.736,
        2.888,
        3.04,
        3.23,
        3.42,
        3.61,
        3.8,
        3.952,
        4.104,
        4.256,
        4.408,
        4.56,
    };

    private readonly double[] ultDefMods =
    {
        0.36,
        0.369,
        0.378,
        0.387,
        0.396,
        0.405,
        0.41625,
        0.4275,
        0.43875,
        0.45,
        0.459,
        0.468,
        0.477,
        0.486,
        0.495,
    };

    private readonly double[] ultChanceMods =
    {
        0.85,
        0.865,
        0.88,
        0.895,
        0.91,
        0.925,
        0.94375,
        0.9625,
        0.98125,
        1.0,
        1.015,
        1.03,
        1.045,
        1.06,
        1.075,
    };
    private readonly double alwChgChnc;
    private readonly double alwChgAtk;
    private readonly double alwChgVal;
    private readonly double ultDmg;
    private readonly double ultDef;
    private readonly double ultChance;
    private readonly int swSkillLvl;
    public SilverWolf(Unit parent) : base(parent)
    {
        Parent.Stats.BaseMaxEnergy = 110;
        alwChgChnc = alwChgChncMods[Parent.Skills.First(x => x.Name == "Allow Changes?").Level-1];
        alwChgAtk = alwChgAtkMods[Parent.Skills.First(x => x.Name == "Allow Changes?").Level-1];
        swSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "System Warning").Level;
        alwChgVal = alwChgValMods[Parent.Skills.First(x => x.Name == "Allow Changes?").Level-1];
        
        ultDmg = ultDmgMods[Parent.Skills.First(x => x.Name == "User Banned").Level-1];
        ultDef = ultDefMods[Parent.Skills.First(x => x.Name == "User Banned").Level-1];
        ultChance = ultChanceMods[Parent.Skills.First(x => x.Name == "User Banned").Level-1];

        allowChangesDebuff = new Buff(Parent, null)
        {  Type = Buff.BuffType.Debuff, BaseDuration = 2, CustomIconName = "Allow_Changes", Effects = new List<Effect>() { } ,EffectStackingType = Buff.EffectStackingTypeEnm.FullReplace};

        allowChangesDebuffAllDmgRes = new Buff(Parent, null)
            { Type = Buff.BuffType.Debuff, BaseDuration = 2, Effects = new List<Effect>() { new EffAllDamageResist() {Value = alwChgVal}} };

        ultDefDebuff    = new Buff(Parent, null)
            {  Type = Buff.BuffType.Debuff, BaseDuration = 3, Effects = new List<Effect>() { new EffDefPrc() {Value = -ultDef} }};
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
        //dmg events
        foreach (double proportion in new[] {  0.25,0.25, 0.5 })
        {
            SystemWarning.Events.Add(new DirectDamage(null, this, Parent)
                { CalculateValue = CalculateBasicDmg, AbilityValue = SystemWarning , CalculateProportion = proportion });
            SystemWarning.Events.Add(new ToughnessShred(null, this, Parent) { Val = 30, AbilityValue = SystemWarning, CalculateProportion = proportion  });
            SystemWarning.Events.Add(new EnergyGain(null, this, Parent)
                { Val = 20, TargetUnit = Parent, AbilityValue = SystemWarning, CalculateProportion = proportion  });
        }

        Abilities.Add(SystemWarning);


        //todo best target is who can be main dps target and does not have element
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

        //User Banned
        Ability UserBanned;
        UserBanned = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Ultimate,
            Name = "User Banned",
            AdjacentTargets = Ability.AdjacentTargetsEnm.None,
            CostType = Resource.ResourceType.Energy,
            Cost = Parent.Stats.BaseMaxEnergy,
            Priority = PriorityEnm.Ultimate,
            EndTheTurn = false,
            Available = UltimateAvailable,
        };
        UserBanned.Events.Add(new AttemptEffect(null, this, Parent) { BaseChance = ultChance, BuffToApply = ultDefDebuff });
        //dmg events
        UserBanned.Events.Add(new DirectDamage(null, this, Parent)
            { CalculateValue = CalculateUltimateDmg, AbilityValue = UserBanned });
        UserBanned.Events.Add(new ToughnessShred(null, this, Parent) { Val = 90, AbilityValue = UserBanned });
        UserBanned.Events.Add(new EnergyGain(null, this, Parent)
            { Val = 5, TargetUnit = Parent, AbilityValue = UserBanned });

        Abilities.Add(UserBanned);

        /* Talent + A4 debuff stacking
         * pick random bug not exists in target. else pick bugs with smallest duration(if 2 bugs with 2 turns and 1 bug with 3 turns then pick random from 2 bugs)         * 
         * Procs from all attack include technique
         *
         */


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
                        Dispellable = false,
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
                //first find elements not in native weakness
                bool reduceResists = true;
                var elemListToApply = this.GetAliveFriends().Select(x => x.Fighter.Element).Where(x=>!ent.TargetUnit.Fighter.NativeWeaknesses.Contains(x)).Distinct();
                if (!elemListToApply.Any())
                {
                    elemListToApply =
                        this.GetAliveFriends().Select(x => x.Fighter.Element).Distinct(); //else pick all elements
                    reduceResists = false;
                }

                Unit.ElementEnm elm = (Unit.ElementEnm)Utl.GetRandomObject(elemListToApply);
                ent.ChildEvents.Add(new ApplyBuffEffect(ent.ParentStep,this,Parent) {TargetUnit = ent.TargetUnit,BuffToApply =allowChangesDebuff,Eff =  new EffWeaknessImpair() { Element = elm } });
                if (reduceResists)
                    ent.ChildEvents.Add(new ApplyBuffEffect(ent.ParentStep,this,Parent) {TargetUnit = ent.TargetUnit,BuffToApply =allowChangesDebuff,Eff =  new EffElementalResist() { Element = elm,Value = -0.2} });




            }
        }
        //E4
        if (ent is ExecuteAbilityFinish && ent.SourceUnit == Parent&&ent.AbilityValue.Attack&&ent.AbilityValue.AbilityType==AbilityTypeEnm.Ultimate&& Parent.Rank >= 4)
        {
            double debuffs = 0;
            debuffs = ent.TargetUnit.Buffs.Count(x => x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);
            debuffs = Math.Min(debuffs, 5);
            for (int i = 0; i < debuffs; i++)
            {
                ent.ChildEvents.Add(new DirectDamage(ent.ParentStep, this, Parent)
                    { TargetUnit = ent.TargetUnit,CalculateValue = CalculateE4Dmg, AbilityValue = ent.AbilityValue });
            }

          
        }
        //TODO handle enemy break shield by any of party members(A6?)

        base.DefaultFighter_HandleEvent(ent);
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