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
using ImageMagick;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Fighters.Character;

public class SilverWolf : DefaultFighter
{
    private Buff allowChangesDebuff;
    private Buff ultDefDebuff;
    private Buff allowChangesDebuffAllDmgRes;
    private Event UltimateHitLastEvent;

    private Buff talentAtkDebuff;
    private Buff talentSpdDebuff;
    private Buff talentDefDebuff;
    private Buff[] bugArray;
    private int bugDuration;
    private int weaknessDuration;
    private TriggerEvent trgEnt;


    private readonly double[] alwChgAtkMods = {
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

    private readonly double[] alwChgAllTypeResMods =
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

    private readonly double[] ultDmgMods =
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

    private readonly double[] talentChanceMods =
    {
        0.6,
        0.612,
        0.624,
        0.636,
        0.648,
        0.66,
        0.675,
        0.69,
        0.705,
        0.72,
        0.732,
        0.744,
        0.756,
        0.768,
        0.78,
    };

    private readonly double[] talentATKMods =
    {
        0.05,
        0.055,
        0.06,
        0.065,
        0.07,
        0.075,
        0.08125,
        0.0875,
        0.09375,
        0.1,
        0.105,
        0.11,
        0.115,
        0.12,
        0.125,
    };

    private readonly double[] talentDEFMods =
    {
        0.04,
        0.044,
        0.048,
        0.052,
        0.056,
        0.06,
        0.065,
        0.07,
        0.075,
        0.08,
        0.084,
        0.088,
        0.092,
        0.096,
        0.1,
    };

    private readonly double[] talentSPDMods =
    {
        0.03,
        0.033,
        0.036,
        0.039,
        0.042,
        0.045,
        0.04875,
        0.0525,
        0.05625,
        0.06,
        0.063,
        0.066,
        0.069,
        0.072,
        0.075,
    };
    private readonly double alwChgChnc;
    private readonly double alwChgAtk;
    private readonly double allowChangesDebuffAllDmgVal;
    private readonly double talentDebuffChance;
    private readonly double ultDmg;
    private readonly double ultDef;
    private readonly double ultChance;
    private int talentLvl;
    private readonly int swSkillLvl;
    public SilverWolf(Unit parent) : base(parent)
    {
        trgEnt = new TriggerEvent(null, null, Parent);
        Parent.Stats.BaseMaxEnergy = 110;
        alwChgChnc = alwChgChncMods[Parent.Skills.First(x => x.Name == "Allow Changes?").Level - 1];
        alwChgAtk = alwChgAtkMods[Parent.Skills.First(x => x.Name == "Allow Changes?").Level - 1];
        swSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "System Warning").Level;
        allowChangesDebuffAllDmgVal = alwChgAllTypeResMods[Parent.Skills.First(x => x.Name == "Allow Changes?").Level - 1];

        ultDmg = ultDmgMods[Parent.Skills.First(x => x.Name == "User Banned").Level - 1];
        ultDef = ultDefMods[Parent.Skills.First(x => x.Name == "User Banned").Level - 1];
        ultChance = ultChanceMods[Parent.Skills.First(x => x.Name == "User Banned").Level - 1];

        talentLvl = Parent.Skills.First(x => x.Name == "Awaiting System Response...").Level;
        talentDebuffChance = talentChanceMods[talentLvl - 1];

        weaknessDuration = Atraces.HasFlag(ATracesEnm.A4) ? 3 : 2;

        allowChangesDebuff = new Buff(Parent, null)
        { Type = Buff.BuffType.Debuff, BaseDuration = weaknessDuration, CustomIconName = "Allow_Changes", Effects = new List<Effect>() { }, EffectStackingType = Buff.EffectStackingTypeEnm.FullReplace };

        allowChangesDebuffAllDmgRes = new Buff(Parent, null)
        { Type = Buff.BuffType.Debuff, BaseDuration = 2, Effects = new List<Effect>() { new EffAllDamageResist() { CalculateValue = calcSkillAllDmgRes } } };

        ultDefDebuff = new Buff(Parent, null)
        { Type = Buff.BuffType.Debuff, BaseDuration = 3, Effects = new List<Effect>() { new EffDefPrc() { Value = -ultDef } } };

        bugDuration = Atraces.HasFlag(ATracesEnm.A2) ? 4 : 3;
        talentAtkDebuff = new Buff(Parent, null)
        { Type = Buff.BuffType.Debuff, BaseDuration = bugDuration, Effects = new List<Effect>() { new EffAtkPrc() { Value = -talentATKMods[talentLvl - 1] } } };
        talentDefDebuff = new Buff(Parent, null)
        { Type = Buff.BuffType.Debuff, BaseDuration = bugDuration, Effects = new List<Effect>() { new EffDefPrc() { Value = -talentDEFMods[talentLvl - 1] } } };
        talentSpdDebuff = new Buff(Parent, null)
        { Type = Buff.BuffType.Debuff, BaseDuration = bugDuration, Effects = new List<Effect>() { new EffSpeedPrc() { Value = -talentSPDMods[talentLvl - 1] } } };
        bugArray = new[] { talentAtkDebuff, talentSpdDebuff, talentDefDebuff };
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
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "System Warning",
            Element = Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.None,
            SpGain = 1
        };
        //dmg events
        foreach (double proportion in new[] { 0.25, 0.25, 0.5 })
        {
            SystemWarning.Events.Add(new DirectDamage(null, this, Parent)
            { CalculateValue = CalculateBasicDmg, CalculateProportion = proportion });

            SystemWarning.Events.Add(new ToughnessShred(null, this, Parent) { Val = 30, CalculateProportion = proportion });
            SystemWarning.Events.Add(new EnergyGain(null, this, Parent)
            { Val = 20, TargetUnit = Parent, CalculateProportion = proportion });
        }
        SystemWarning.Events.Add(trgEnt);
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
        { CalculateValue = CalculateAbilityDmg});
        AllowChanges.Events.Add(new ToughnessShred(null, this, Parent) { Val = 60 });
        AllowChanges.Events.Add(trgEnt);
        AllowChanges.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 30, TargetUnit = Parent });
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
        { CalculateValue = CalculateUltimateDmg });
        UserBanned.Events.Add(new ToughnessShred(null, this, Parent) { Val = 90 });
        UserBanned.Events.Add(trgEnt);
        UltimateHitLastEvent = new EnergyGain(null, this, Parent)
        { Val = 5, TargetUnit = Parent };
        UserBanned.Events.Add(UltimateHitLastEvent);

        Abilities.Add(UserBanned);

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

    private double? calcSkillAllDmgRes(Event ent)
    {
        //get default from table
        double res = allowChangesDebuffAllDmgVal;
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
                bool reduceResists = true;
                var elemListToApply = this.GetAliveFriends().Select(x => x.Fighter.Element).Where(x => !ent.TargetUnit.Fighter.NativeWeaknesses.Contains(x)).Distinct();
                if (!elemListToApply.Any())
                {
                    elemListToApply =
                        this.GetAliveFriends().Select(x => x.Fighter.Element).Distinct(); //else pick all elements
                    reduceResists = false;
                }

                Unit.ElementEnm elm = (Unit.ElementEnm)Utl.GetRandomObject(elemListToApply, Parent.ParentTeam.ParentSim.Parent);
                ent.ChildEvents.Add(new ApplyBuffEffect(ent.ParentStep, this, Parent) { TargetUnit = ent.TargetUnit, BuffToApply = allowChangesDebuff, Eff = new EffWeaknessImpair() { Element = elm } });
                if (reduceResists)
                    ent.ChildEvents.Add(new ApplyBuffEffect(ent.ParentStep, this, Parent) { TargetUnit = ent.TargetUnit, BuffToApply = allowChangesDebuff, Eff = new EffElementalResist() { Element = elm, Value = -0.2 } });




            }
        }
        //E2-4
        else if (Parent.Rank >= 2 && ent.Reference == UltimateHitLastEvent)//EnergyGain event
        {
            double debuffs;
            Unit tarUnit = ent.ParentStep.Target;
            debuffs = tarUnit.Buffs.Count(x => x.Type == Buff.BuffType.Debuff || x.Type == Buff.BuffType.Dot);
            debuffs = Math.Min(debuffs, 5);
            if (Parent.Rank >= 4)
                for (int i = 0; i < debuffs; i++)
                {
                    ent.ChildEvents.Add(new DirectDamage(ent.ParentStep, this, Parent)
                    { TargetUnit = tarUnit, CalculateValue = CalculateE4Dmg });
                }
            ent.ChildEvents.Add(new EnergyGain(ent.ParentStep, this, Parent)
            { TargetUnit = Parent, Val = 7 * debuffs });


        }

        else if (ent.Reference==trgEnt && ent.SourceUnit == Parent )
        {
            ImplantBug(ent.TargetUnit, ent, talentDebuffChance);
        }
        //a2. shield got broken by teammate
        else if (ent is ToughnessBreak && Atraces.HasFlag(ATracesEnm.A2) && ent.SourceUnit.ParentTeam == Parent.ParentTeam)
        {
            ImplantBug(ent.TargetUnit, ent, 0.65);//fixed chance
        }
        base.DefaultFighter_HandleEvent(ent);
    }

    //apply talent Debuff
    private void ImplantBug(Unit target, Event ent, double chance)
    {


        var currentBugs = target.Buffs.Where(x => bugArray.Contains(x.Reference)).ToArray();
        //try search bugs to implant by lowest duration
        for (int i = 1; i <= bugDuration; i++)
        {
            //get current bugs on target
            var notExistsBugs = bugArray.Where(x => !currentBugs.Where(y => y.DurationLeft == i).Select(y => y.Reference).Contains(x)).ToArray();
            if (notExistsBugs.Length > 0 && notExistsBugs.Length < bugArray.Length)
            {
                Buff randomBug = (Buff)Utl.GetRandomObject(notExistsBugs, Parent.ParentTeam.ParentSim.Parent);
                ent.ChildEvents.Add(new AttemptEffect(ent.ParentStep, this, ent.SourceUnit) { BuffToApply = randomBug, BaseChance = chance, TargetUnit = target });
                break;
            }
            //if no debuff founded  then get random from start array
            if (i == bugDuration)
            {
                Buff randomBug = (Buff)Utl.GetRandomObject(bugArray, Parent.ParentTeam.ParentSim.Parent);
                ent.ChildEvents.Add(new AttemptEffect(ent.ParentStep, this, ent.SourceUnit) { BuffToApply = randomBug, BaseChance = chance, TargetUnit = target });
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