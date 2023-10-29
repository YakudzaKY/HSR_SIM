﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;


namespace HSR_SIM_LIB.Fighters.Character
{
    public class Blade : DefaultFighter
    {


        //ultimate
        //main
        private readonly Dictionary<int, double> dsMainAtkMods = new()
        {
            { 1, 0.24 }, { 2, 0.256 }, { 3, 0.272 }, { 4, 0.288 }, { 5, 0.304 },{ 6, 0.32 }
            ,{ 7, 0.34 }, { 8, 0.36 }, { 9, 0.38 }, { 10, 0.40 }, { 11, 0.416 },{ 12, 0.432 }
        };

        private readonly Dictionary<int, double> dsMainHpMods = new()
        {
            { 1, 0.60 }, { 2, 0.64 }, { 3, 0.68 }, { 4, 0.72 }, { 5, 0.76 },{ 6, 0.80 }
            ,{ 7, 0.85 }, { 8, 0.90 }, { 9, 0.95 }, { 10, 1.00 }, { 11, 1.04 },{ 12, 1.08 }
        };
        //adjacent
        private readonly Dictionary<int, double> dsAdjAtkMods = new()
        {
            { 1, 0.096 }, { 2, 0.102 }, { 3, 0.109 }, { 4, 0.115 }, { 5, 0.122 },{ 6, 0.128 }
            ,{ 7, 0.136 }, { 8, 0.144 }, { 9, 0.152 }, { 10, 0.16 }, { 11, 0.166 },{ 12, 0.173 }
        };

        private readonly Dictionary<int, double> dsAdjHpMods = new()
        {
            { 1, 0.24 }, { 2, 0.256 }, { 3, 0.272 }, { 4, 0.288 }, { 5, 0.304 },{ 6, 0.32 }
            ,{ 7, 0.34 }, { 8, 0.36 }, { 9, 0.38 }, { 10, 0.40 }, { 11, 0.416 },{ 12, 0.432 }
        };

        private readonly Dictionary<int, double> dsAdjHpLossMods = new()
        {
            { 1, 0.24 }, { 2, 0.26 }, { 3, 0.27 }, { 4, 0.29 }, { 5, 0.30 },{ 6, 0.32 }
            ,{ 7, 0.34 }, { 8, 0.36 }, { 9, 0.38 }, { 10, 0.40 }, { 11, 0.42 },{ 12, 0.43 }
        };


        //
        private readonly Dictionary<int, double> forestAdjAtkMods = new()
        {
            { 1, 0.08 }, { 2, 0.096 }, { 3, 0.112 }, { 4, 0.128 }, { 5, 0.144 }
            ,{ 6, 0.16 }, {7, 0.176 }
        };

        private readonly Dictionary<int, double> sgAtkMods = new()
        {
            { 1, 0.22 }, { 2, 0.242 }, { 3, 0.264 }, { 4, 0.286 }, { 5, 0.308 }
            ,{ 6, 0.330 }, {7, 0.358 },{ 8, 0.385}, {9, 0.431 },{ 10, 0.44 }
            , {11, 0.462 },{ 12, 0.484 }
        };
        private readonly Dictionary<int, double> sgHpMods = new()
        {
            { 1, 0.55 }, { 2, 0.605 }, { 3, 0.66 }, { 4, 0.715 }, { 5, 0.77}
            ,{ 6, 0.825}, {7, 0.894 },{ 8, 0.963 }, {9, 1.031 },{ 10, 1.10 }
            , {11, 1.155 },{ 12, 1.21 }
        };


        private readonly double shuHuMaxCnt;
        private readonly Ability shuhuGift;
        private readonly Ability deathSentence;
        private readonly Mod hellscapeBuff = null;
        private readonly Ability forestOfSwords;
        public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Destruction;
        public sealed override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Wind;

        public override Ability ChoseAbilityToCast(Step step)
        {
            Ability watAbility = null;

            return watAbility ?? base.ChoseAbilityToCast(step);
        }

        public override void DefaultFighter_HandleEvent(Event ent)
        {
            //if unit consume hp or got attack then apply buff
            if (Mechanics.Values[shuhuGift] < shuHuMaxCnt && ent.TargetUnit == Parent && ent is ResourceDrain && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal > 0)
            {

                MechanicValChg sgProc = new MechanicValChg(ent.ParentStep, this, Parent)
                { TargetUnit = Parent, Val = 1, AbilityValue = shuhuGift };
                ent.ChildEvents.Add(sgProc);
            }
            //buffering Lost hp pull
            if (ent.TargetUnit == Parent
                 && (
                     (ent is ResourceDrain && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal > 0)
                     || ent is DirectDamage or ShieldBreak or DoTDamage
                     ))
            {

                MechanicValChg dsValCharge = new MechanicValChg(ent.ParentStep, this, Parent)
                { TargetUnit = Parent, Val = ent.RealVal, AbilityValue = deathSentence };
                ent.ChildEvents.Add(dsValCharge);
            }


            if (Atraces.HasFlag(ATracesEnm.A2)&&ent.SourceUnit == Parent&&ent is DirectDamage&&ent.AbilityValue==forestOfSwords&&ent.TargetUnit.GetRes(Resource.ResourceType.Toughness).ResVal==0)
            {
                ent.ChildEvents.Add(new Healing(ent.ParentStep, this, this.Parent)
                    { TargetUnit = Parent, CalculateValue = CalculateForestHeal, AbilityValue = forestOfSwords });
            }


            base.DefaultFighter_HandleEvent(ent);
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

        public double? CalculateSGDmg(Event ent)
        {
            int skillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Shuhu's Gift").Level;
            double attackPart = Parent.GetAttack(null) * sgAtkMods[skillLvl];
            double maxHpPart = Parent.GetMaxHp(ent) * sgHpMods[skillLvl];
            return FighterUtils.CalculateDmgByBasicVal(attackPart + maxHpPart, ent);
        }


        public double? CalculateHellscapeSelfDmg(Event ent)
        {
            return Parent.GetMaxHp(ent) * 0.3;
        }

        public override string GetSpecialText()
        {
            return $"SG: {(int)Mechanics.Values[shuhuGift]:d}\\{(int)shuHuMaxCnt:d}  DS: {(int)Mechanics.Values[deathSentence]:d}\\{(int)getDsMaxLostHp(null):d}";
        }



        public bool SGAvailable()
        {
            return (Mechanics.Values[shuhuGift] == shuHuMaxCnt);
        }

        //50-110
        public double? CalculateBasicDmg(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(null) * (0.4 + (Parent.Skills.FirstOrDefault(x => x.Name == "Shard Sword").Level * 0.1)), ent);
        }

        public double getDsMaxLostHp(Event ent)
        {
            return Parent.GetMaxHp(ent) * 0.9;
        }

        //damage for ULTI main target
        public double? CalculateDsDmg(Event ent)
        {
            int skillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Death Sentence").Level;
            double attackPart = Parent.GetAttack(null) * dsMainAtkMods[skillLvl];
            double maxHpPart = Parent.GetMaxHp(ent) * dsMainHpMods[skillLvl];
            double hpLossPart = Math.Min(Mechanics.Values[deathSentence], getDsMaxLostHp(ent)) * dsMainHpMods[skillLvl];
            return FighterUtils.CalculateDmgByBasicVal(attackPart + maxHpPart + hpLossPart, ent);
        }

        //damage for ULTI adjacent target
        public double? CalculateDstDmgAdj(Event ent)
        {
            int skillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Death Sentence").Level;
            double attackPart = Parent.GetAttack(null) * dsAdjAtkMods[skillLvl];
            double maxHpPart = Parent.GetMaxHp(ent) * dsAdjHpMods[skillLvl];
            double hpLossPart = Math.Min(Mechanics.Values[deathSentence], getDsMaxLostHp(ent)) * dsAdjHpLossMods[skillLvl];
            return FighterUtils.CalculateDmgByBasicVal(attackPart + maxHpPart + hpLossPart, ent);
        }


        //damage for main target
        public double? CalculateForestDmg(Event ent)
        {
            int skillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Forest of Swords").Level;
            double attackPart = Parent.GetAttack(null) * (0.16 +
                (skillLvl * 0.04));
            double maxHpPart = Parent.GetMaxHp(ent) * (0.4 + (skillLvl * 0.1));
            return FighterUtils.CalculateDmgByBasicVal(attackPart + maxHpPart, ent);
        }

        //damage for adjacent target
        public double? CalculateForestDmgAdj(Event ent)
        {
            int skillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Forest of Swords").Level;
            double attackPart = Parent.GetAttack(null) * forestAdjAtkMods[skillLvl];
            double maxHpPart = Parent.GetMaxHp(ent) * (0.16 + (skillLvl * 0.04));
            return FighterUtils.CalculateDmgByBasicVal(attackPart + maxHpPart, ent);
        }
        //a4
        public double? CalculateForestHeal(Event ent)
        {
            return FighterUtils.CalculateHealByBasicVal(Parent.GetMaxHp(ent)*0.05+100, ent);

        }

        public double? CalculateDsSelfDmg(Event ent)
        {

            double setValue = 0;
            if (Parent.GetHpPrc(ent) > 0.5)
                setValue = Parent.GetRes(Resource.ResourceType.HP).ResVal - (Parent.GetMaxHp(ent) / 2);
            return setValue;
        }

        public double? CalculateDsSelfHeal(Event ent)
        {

            double setValue = 0;
            if (Parent.GetHpPrc(ent) < 0.5)
                setValue = (Parent.GetMaxHp(ent) / 2) - Parent.GetRes(Resource.ResourceType.HP).ResVal;
            return setValue;
        }

        public bool HellscapeActive()
        {
            return Parent.Mods.Any(x => x.RefMod == hellscapeBuff);
        }
        public bool HellscapeNotActive()
        {
            return Parent.Mods.All(x => x.RefMod != hellscapeBuff);
        }

        public double? CalculateSgHeal(Event ent)
        {

            return FighterUtils.CalculateHealByBasicVal(Parent.GetMaxHp(ent) * 0.25, ent);
        }

        //Blade constructor
        public Blade(Unit parent) : base(parent)
        {

            Parent.Stats.BaseMaxEnergy = 130;

            hellscapeBuff = new Mod(Parent)
            {
                Type = Mod.ModType.Buff,
                BaseDuration = 3,
                MaxStack = 1,
                CustomIconName = "Hellscape"
            };



            //=====================
            //Abilities
            //=====================



            //Passive
            shuhuGift = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.FollowUpAction
                ,
                Name = "Shuhu's Gift"
                ,
                Element = Element
                ,
                ToughnessShred = 30
                ,
                TargetType = TargetTypeEnm.Enemy
                ,
                AdjacentTargets = AdjacentTargetsEnm.All
                ,
                EnergyGain = 10
                ,
                Attack = true
                ,
                Available = SGAvailable
                ,
                Priority = PriorityEnm.Medium
            };
            shuHuMaxCnt = (parent.Rank == 6) ? 4 : 5;//4 stacks on 6 eidolon 
            shuhuGift.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateSGDmg, AbilityValue = shuhuGift });
            shuhuGift.Events.Add(new Healing(null, this, this.Parent) { TargetUnit = Parent, CalculateValue = CalculateSgHeal, AbilityValue = shuhuGift });
            shuhuGift.Events.Add(new MechanicValChg(null, this, this.Parent) { TargetUnit = Parent, AbilityValue = shuhuGift, Val = -shuHuMaxCnt });
            Abilities.Add(shuhuGift);
            //Passive counter
            Mechanics.AddVal(shuhuGift);

            //basic

            Ability shardSword;
            shardSword = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Basic
                ,
                Name = "Shard Sword"
                ,
                Element = Element
                ,
                AdjacentTargets = Ability.AdjacentTargetsEnm.None
                ,
                Attack = true
                ,
                ToughnessShred = 30
                ,
                EnergyGain = 20
                ,
                SpGain = 1
                ,
                Available = HellscapeNotActive
            };
            //dmg events
            shardSword.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateBasicDmg, AbilityValue = shardSword });
            Abilities.Add(shardSword);

            
            forestOfSwords = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Basic
                ,
                Name = "Forest of Swords"
                ,
                Element = Element
                ,
                AdjacentTargets = Ability.AdjacentTargetsEnm.Blast
                ,
                Attack = true
                ,
                ToughnessShred = 60
                ,
                AdjacentToughnessShred = 30
                ,
                EnergyGain = 30
                ,
                Available = HellscapeActive
            };
            //dmg events
            forestOfSwords.Events.Add(new ResourceDrain(null, this, this.Parent) { ResType = Resource.ResourceType.HP, TargetType = TargetTypeEnm.Self, CanSetToZero = false, CalculateValue = CalculateForestSelfDmg, AbilityValue = forestOfSwords, CurentTargetType = AbilityCurrentTargetEnm.AbilityMain });
            forestOfSwords.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateForestDmg, AbilityValue = forestOfSwords, CurentTargetType = AbilityCurrentTargetEnm.AbilityMain });
            forestOfSwords.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateForestDmgAdj, AbilityValue = forestOfSwords, CurentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
           

            Abilities.Add(forestOfSwords);


            deathSentence = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Ultimate
                ,
                Name = "Death Sentence"
                ,
                AdjacentTargets = Ability.AdjacentTargetsEnm.Blast
                ,
                
                Attack = true
                ,
                ToughnessShred = 60
                ,
                AdjacentToughnessShred = 60
                ,
                EnergyGain = 5
                ,
                Available = UltimateAvailable
                ,
                Priority = PriorityEnm.Ultimate
                ,
                EndTheTurn = false
                ,
                CostType = Resource.ResourceType.Energy
                ,
                Cost = Parent.Stats.BaseMaxEnergy

            };
            //dmg events
            deathSentence.Events.Add(new ResourceDrain(null, this, this.Parent) { ResType = Resource.ResourceType.HP,TargetUnit = Parent, CanSetToZero = false, CalculateValue = CalculateDsSelfDmg, AbilityValue = deathSentence, CurentTargetType = AbilityCurrentTargetEnm.AbilityMain });
            deathSentence.Events.Add(new ResourceGain(null, this, this.Parent) { ResType = Resource.ResourceType.HP, TargetUnit = Parent, CalculateValue = CalculateDsSelfHeal, AbilityValue = deathSentence, CurentTargetType = AbilityCurrentTargetEnm.AbilityMain });
            deathSentence.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateDsDmg, AbilityValue = deathSentence, CurentTargetType = AbilityCurrentTargetEnm.AbilityMain });
            deathSentence.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateDstDmgAdj, AbilityValue = deathSentence, CurentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent });
            deathSentence.Events.Add(new MechanicValChg(null, this, this.Parent) { TargetUnit = Parent, AbilityValue = deathSentence, CalculateValue = CalcResetDsCharge });
            Abilities.Add(deathSentence);
            Mechanics.AddVal(deathSentence);

            var karmaWind =
                //Karma Wind
                new Ability(this)
                {
                    AbilityType = Ability.AbilityTypeEnm.Technique
                ,
                    Name = "Karma Wind"
                ,
                    Cost = 1
                ,
                    CostType = Resource.ResourceType.TP
                ,
                    Element = Element
                ,
                    ToughnessShred = 60
                ,
                    Attack = true
                ,
                    TargetType = Ability.TargetTypeEnm.Enemy
                ,
                    AdjacentTargets = AdjacentTargetsEnm.All
                };
            //dmg events
            karmaWind.Events.Add(new ResourceDrain(null, this, this.Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, ResType = Resource.ResourceType.HP, TargetType = TargetTypeEnm.Self, CanSetToZero = false, CalculateValue = CalculateKarmaSelfDmg, AbilityValue = karmaWind, CurentTargetType = AbilityCurrentTargetEnm.AbilityMain });
            karmaWind.Events.Add(new DirectDamage(null, this, this.Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, CalculateValue = CalculateKarmaDmg, AbilityValue = karmaWind });

            Abilities.Add(karmaWind);


            Ability Hellscape;
            //Hellscape
            Hellscape = new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Ability
                ,
                Name = "Hellscape"
                ,
                Cost = 1
                ,
                CostType = Resource.ResourceType.SP
                ,
                ToughnessShred = 60
                ,
                Attack = false
                ,
                TargetType = Ability.TargetTypeEnm.Self
                ,
                AdjacentTargets = AdjacentTargetsEnm.None
                ,
                EndTheTurn = false
                ,
                Available = HellscapeNotActive
            };
            //dmg events
            Hellscape.Events.Add(new ResourceDrain(null, this, this.Parent) { ResType = Resource.ResourceType.HP, TargetType = TargetTypeEnm.Self, CanSetToZero = false, CalculateValue = CalculateHellscapeSelfDmg, AbilityValue = Hellscape, CurentTargetType = AbilityCurrentTargetEnm.AbilityMain });
            Hellscape.Events.Add(new ApplyMod(null, this, Parent)
            {
                AbilityValue = Hellscape,
                TargetUnit = Parent,
                Modification = hellscapeBuff
            });

            Abilities.Add(Hellscape);

            //=====================


            //=====================
            //Ascended Traces
            //=====================
            if (Atraces.HasFlag(ATracesEnm.A2))
            {
                ConditionMods.Add(new ConditionMod(Parent)
                {
                    Mod = new Mod(Parent) { Effects = new List<Effect>() { new Effect() { EffType = Effect.EffectType.IncomeHealingPrc, Value = 0.20 } }, CustomIconName = "Traces\\A2" }
                    ,
                    Target = Parent
                    ,
                    Condition = new ConditionMod.ConditionRec() { CondtionParam = ConditionMod.ConditionCheckParam.HPPrc, CondtionExpression = ConditionMod.ConditionCheckExpression.EqualOrLess, Value = 0.5 }

                });
            }
            if (Atraces.HasFlag(ATracesEnm.A6))
            {
                PassiveMods.Add(new PassiveMod(Parent)
                {
                    Mod = new Mod(Parent)
                    {
                        Effects = new List<Effect>() { new Effect(){ EffType = Effect.EffectType.AbilityTypeBoost, Value = 0.20, AbilityType =  Ability.AbilityTypeEnm.FollowUpAction },
                       }
                    },
                    Target = Parent

                });
            }


            //TODO A4




        }

        private double? CalcResetDsCharge(Event ent)
        {
            return -Mechanics.Values[deathSentence];
        }
    }
}
