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
            return FighterUtils.CalculateBasicDmg(Parent.Stats.MaxHp * 0.4, ent);
        }

        public override string GetSpecialText()
        {
            return $"SG: {(int)Mechanics.Values[shuhuGift]:d}\\{(int)ShuHuMaxCnt:d}";
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

            Ability ability;
            //Karma Wind
            ability = new Ability(Parent) {   AbilityType = Ability.AbilityTypeEnm.Technique
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
            ability.Events.Add(new Event(null, this) { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.ResourceDrain, ResType = Resource.ResourceType.HP, TargetType =TargetTypeEnm.Self, CanSetToZero = false, CalculateValue = CalculateKarmaSelfDmg, AbilityValue = ability ,CurentTargetType=AbilityCurrentTargetEnm.AbilityMain});
            ability.Events.Add(new Event(null, this) { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.DirectDamage, CalculateValue = CalculateKarmaDmg, AbilityValue = ability });

            Abilities.Add(ability);


            shuhuGift = new Ability(Parent) {   AbilityType = Ability.AbilityTypeEnm.FolowUpAttack
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
            
            //=====================


            //=====================
            //Ascended Traces
            //=====================
            if (Atraces.HasFlag(ATracesEnm.A2))
            {
                ConditionMods.Add(new ConditionMod(Parent)
                {
                    Mod=new Mod(){Effects = new List<Effect>(){ new Effect(){ EffType = Effect.EffectType.IncomeHealingPrc,Value = 0.20}},CustomIconName = "Traces\\A2"}
                    , Target=Parent
                    ,Condition= new ConditionMod.ConditionRec(){CondtionParam = ConditionMod.ConditionCheckParam.HPPrc,CondtionExpression = ConditionMod.ConditionCheckExpression.EqualOrLess,Value = 0.5}
                    
                });
            }
            if (Atraces.HasFlag(ATracesEnm.A6))
            {
                PassiveMods.Add(new PassiveMod(Parent)
                {
                    Mod = new Mod()
                    { Effects =  new List<Effect>() { new Effect(){ EffType = Effect.EffectType.AbilityTypeBoost, Value = 0.20, AbilityTypes = new List<Ability.AbilityTypeEnm>(){ Ability.AbilityTypeEnm.FolowUpAttack} }, 
                       } },
                    Target = Parent
                   
                });
            }

          
            //TODO A4


    

        }
    }
}
