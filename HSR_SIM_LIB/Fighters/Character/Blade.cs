using System;
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
 
        private readonly Dictionary<int, double> forestAdjAtkMods = new()
        {
            { 1, 0.08 }, { 2, 0.096 }, { 3, 0.112 }, { 4, 0.128 }, { 5, 0.144 }
            ,{ 6, 0.16 }, {7, 0.176 }
        };


        private readonly double ShuHuMaxCnt;
        private readonly Ability shuhuGift;
        private readonly Mod hellscapeBuff = null;
        public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Destruction;
        public sealed override Unit.ElementEnm Element { get;  } =Unit.ElementEnm.Wind;

        public override Ability ChoseAbilityToCast(Step step)
        {
            Ability watAbility = null;

            return watAbility ?? base.ChoseAbilityToCast(step);
        }

        public override void DefaultFighter_HandleEvent(Event ent)
        {
            //if unit consume hp or got attack then apply buff
            if (ent.TargetUnit == Parent && ent is ResourceDrain && ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal > 0)
            {
                MechanicValChg sgProc = new MechanicValChg(ent.ParentStep, this, Parent)
                    { TargetUnit = Parent, Val = 1, AbilityValue = shuhuGift };
                ent.ChildEvents.Add(sgProc);
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
        
        public double? CalculateHellscapeSelfDmg(Event ent)
        {
            return Parent.GetMaxHp(ent) * 0.3;
        }

        public override string GetSpecialText()
        {
            return $"SG: {(int)Mechanics.Values[shuhuGift]:d}\\{(int)ShuHuMaxCnt:d}";
        }



        public bool SGAvailable()
        {
            return (Mechanics.Values[shuhuGift] == ShuHuMaxCnt);
        }

        //50-110
        public double? CalculateBasicDmg(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.Stats.Attack*(0.4 + (Parent.Skills.FirstOrDefault(x=>x.Name=="Shard Sword").Level*0.1)), ent);
        }

        //damage for main target
        public double? CalculateForestDmg(Event ent)
        {
            int skillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Forest of Swords").Level;
            double attackPart =Parent.Stats.Attack *(0.16 +
                (skillLvl * 0.04));
            double maxHpPart =Parent.GetMaxHp(ent) *(0.4 +( skillLvl* 0.1));
            return FighterUtils.CalculateDmgByBasicVal(attackPart+maxHpPart, ent);
        }

        //damage for adjacent target
        public double? CalculateForestDmgAdj(Event ent)
        {
            int skillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Forest of Swords").Level;
            double attackPart = Parent.Stats.Attack * forestAdjAtkMods[skillLvl];
            double maxHpPart =Parent.GetMaxHp(ent) *(0.16 + (skillLvl * 0.04));
            return FighterUtils.CalculateDmgByBasicVal(attackPart+maxHpPart, ent);
        }

        public bool HellscapeActive()
        {
            return Parent.Mods.Any(x => x.RefMod == hellscapeBuff);
        }
        public bool HellscapeNotActive()
        {
            return Parent.Mods.All(x => x.RefMod != hellscapeBuff);
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

            //basic

            Ability shardSword;
            shardSword = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Basic
                , Name = "Shard Sword"
                , Element = Element
                , AdjacentTargets = Ability.AdjacentTargetsEnm.None
                , Attack=true
                , ToughnessShred = 30
                , EnergyGain = 20
                , SPgain = 1
                , Available = HellscapeNotActive
            };
            //dmg events
            shardSword.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateBasicDmg,  AbilityValue = shardSword });
            Abilities.Add(shardSword);

            Ability ForestofSwords;
            ForestofSwords = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Basic
                , Name = "Forest of Swords"
                , Element = Element
                , AdjacentTargets = Ability.AdjacentTargetsEnm.Blast
                , Attack=true
                , ToughnessShred = 60
                , AdjacentToughnessShred =30
                , EnergyGain = 30
                , Available = HellscapeActive
            };
            //dmg events
            ForestofSwords.Events.Add(new ResourceDrain(null, this,this.Parent) { ResType = Resource.ResourceType.HP, TargetType =TargetTypeEnm.Self, CanSetToZero = false, CalculateValue = CalculateForestSelfDmg, AbilityValue = ForestofSwords ,CurentTargetType=AbilityCurrentTargetEnm.AbilityMain});
            ForestofSwords.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateForestDmg,  AbilityValue = ForestofSwords ,CurentTargetType = AbilityCurrentTargetEnm.AbilityMain});
            ForestofSwords.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateForestDmgAdj,  AbilityValue = ForestofSwords ,CurentTargetType = AbilityCurrentTargetEnm.AbilityAdjacent});
            Abilities.Add(ForestofSwords);


            Ability KarmaWind;
            //Karma Wind
            KarmaWind = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Technique
                                            , Name = "Karma Wind"
                                            , Cost = 1
                                            , CostType = Resource.ResourceType.TP
                                            , Element = Element
                                            , ToughnessShred = 60
                                            , Attack=true
                                            , TargetType = Ability.TargetTypeEnm.Enemy
                                            , AdjacentTargets = AdjacentTargetsEnm.All
            };
            //dmg events
            KarmaWind.Events.Add(new ResourceDrain(null, this,this.Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, ResType = Resource.ResourceType.HP, TargetType =TargetTypeEnm.Self, CanSetToZero = false, CalculateValue = CalculateKarmaSelfDmg, AbilityValue = KarmaWind ,CurentTargetType=AbilityCurrentTargetEnm.AbilityMain});
            KarmaWind.Events.Add(new DirectDamage(null, this,this.Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility,  CalculateValue = CalculateKarmaDmg, AbilityValue = KarmaWind });

            Abilities.Add(KarmaWind);

            //Passive
            shuhuGift = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.FollowUpAction
                , Name = "Shuhu's Gift"
                , Element = Element
                , ToughnessShred = 30
                , TargetType = TargetTypeEnm.Enemy
                , AdjacentTargets = AdjacentTargetsEnm.All
                , EnergyGain =10
                , Attack=true
                , Available=SGAvailable
                , Priority = PriorityEnm.Medium
            };
            Abilities.Add(shuhuGift);

            //Passive counter
            ShuHuMaxCnt = (parent.Rank == 6) ? 4 : 5;//4 stacks on 6 eidolon 
            Mechanics.AddVal(shuhuGift);
            
            Ability Hellscape;
            //Hellscape
            Hellscape = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Ability
                , Name = "Hellscape"
                , Cost = 1
                , CostType = Resource.ResourceType.SP
                , ToughnessShred = 60
                , Attack=false
                , TargetType = Ability.TargetTypeEnm.Self
                , AdjacentTargets =AdjacentTargetsEnm.None
                , EndTheTurn= false
                , Available=HellscapeNotActive
            };
            //dmg events
            Hellscape.Events.Add(new ResourceDrain(null, this,this.Parent) {  ResType = Resource.ResourceType.HP, TargetType =TargetTypeEnm.Self, CanSetToZero = false, CalculateValue = CalculateHellscapeSelfDmg, AbilityValue = Hellscape ,CurentTargetType=AbilityCurrentTargetEnm.AbilityMain});
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
                    Mod=new Mod(Parent){Effects = new List<Effect>(){ new Effect(){ EffType = Effect.EffectType.IncomeHealingPrc,Value = 0.20}},CustomIconName = "Traces\\A2"}
                    , Target=Parent
                    ,Condition= new ConditionMod.ConditionRec(){CondtionParam = ConditionMod.ConditionCheckParam.HPPrc,CondtionExpression = ConditionMod.ConditionCheckExpression.EqualOrLess,Value = 0.5}
                    
                });
            }
            if (Atraces.HasFlag(ATracesEnm.A6))
            {
                PassiveMods.Add(new PassiveMod(Parent)
                {
                    Mod = new Mod(Parent)
                    { Effects =  new List<Effect>() { new Effect(){ EffType = Effect.EffectType.AbilityTypeBoost, Value = 0.20, AbilityType =  Ability.AbilityTypeEnm.FollowUpAction }, 
                       } },
                    Target = Parent
                   
                });
            }

          
            //TODO A4


    

        }
    }
}
