using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;


namespace HSR_SIM_LIB.Fighters.Character
{
    public class Blade : DefaultFighter
    {
        public override FighterUtils.PathType? Path { get; set; } = FighterUtils.PathType.Destruction;

        private readonly double ShuHuMaxCnt;
        private readonly Ability shuhuGift;
        public override Ability ChooseAbilityToCast(Step step)
        {
            Ability watAbility = null;

            return watAbility ?? base.ChooseAbilityToCast(step);
        }

        public override void DefaultFighter_HandleEvent(Event ent)
        {
            //if unit consume hp or got attack then apply buff
            if (ent.TargetUnit == Parent && ent.Type == Event.EventType.ResourceDrain && ent.ResType == Resource.ResourceType.HP && ent.RealVal > 0)
            {
                Mechanics.Values[shuhuGift] =  Math.Min( (double) Mechanics.Values[shuhuGift] +1,  (double)  ShuHuMaxCnt);
            }
           
            base.DefaultFighter_HandleEvent(ent);
        }



        public double? CalculateKarmaSelfDmg(Event ent)
        {
            return Parent.Stats.MaxHp * 0.2;
        }

        public double? CalculateKarmaDmg(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.Stats.MaxHp * 0.4, ent);
        }
        
        public double? CalculateHellscapeSelfDmg(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.Stats.MaxHp * 0.3, ent);
        }

        public override string GetSpecialText()
        {
            return $"SG: {(int)Mechanics.Values[shuhuGift]:d}\\{(int)ShuHuMaxCnt:d}";
        }

        public bool ICanUseHellscape(Step step)
        {
            return true;
        }
        //Blade constructor
        public Blade(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Wind;
            Parent.Stats.BaseMaxEnergy = 130;


            //=====================
            //Abilities
            //=====================

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
            KarmaWind.Events.Add(new Event(null, this,this.Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.ResourceDrain, ResType = Resource.ResourceType.HP, TargetType =TargetTypeEnm.Self, CanSetToZero = false, CalculateValue = CalculateKarmaSelfDmg, AbilityValue = KarmaWind ,CurentTargetType=AbilityCurrentTargetEnm.AbilityMain});
            KarmaWind.Events.Add(new Event(null, this,this.Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.DirectDamage, CalculateValue = CalculateKarmaDmg, AbilityValue = KarmaWind });

            Abilities.Add(KarmaWind);

            //Passive
            shuhuGift = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.FolowUpAttack
                , Name = "Shuhu's Gift"
                , Element = Element
                , ToughnessShred = 30
                , TargetType = TargetTypeEnm.Enemy
                , AdjacentTargets = AdjacentTargetsEnm.All
                , EnergyGain =10
                , Attack=true
            };
            Abilities.Add(shuhuGift);

            //Passive counter
            ShuHuMaxCnt = (parent.Rank == 6) ? 4 : 5;//4 stacks on 6 eidolon 
            Mechanics.AddVal(shuhuGift);
            

            
            Ability Hellscape;
            //Karma Wind
            Hellscape = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Ability
                , Name = "Hellscape"
                , Cost = 1
                , CostType = Resource.ResourceType.SP
                , ToughnessShred = 60
                , Attack=false
                , TargetType = Ability.TargetTypeEnm.Self
                , AdjacentTargets =AdjacentTargetsEnm.None
                , EndTheTurn= false
                , CanUsePrc=ICanUseHellscape
            };
            //dmg events
            Hellscape.Events.Add(new Event(null, this,this.Parent) { Type = Event.EventType.ResourceDrain, ResType = Resource.ResourceType.HP, TargetType =TargetTypeEnm.Self, CanSetToZero = false, CalculateValue = CalculateHellscapeSelfDmg, AbilityValue = Hellscape ,CurentTargetType=AbilityCurrentTargetEnm.AbilityMain});
            

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
                    { Effects =  new List<Effect>() { new Effect(){ EffType = Effect.EffectType.AbilityTypeBoost, Value = 0.20, AbilityTypes = new List<Ability.AbilityTypeEnm>(){ Ability.AbilityTypeEnm.FolowUpAttack} }, 
                       } },
                    Target = Parent
                   
                });
            }

          
            //TODO A4


    

        }
    }
}
