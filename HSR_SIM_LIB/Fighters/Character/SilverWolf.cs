using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class SilverWolf:DefaultFighter
    {
        public  override FighterUtils.PathType? Path { get; set; } = FighterUtils.PathType.Nihility;


        public override void DefaultFighter_HandleEvent(Event ent)
        {
            //if unit consume hp or got attack then apply buff
            if ( Parent.Rank>=2&& Parent.IsAlive&& ent.Type == Event.EventType.UnitEnteringBattle )
            {
                //if enemy enter combat need debuff
                if (Parent.Enemies.Any(x => x == ent.TargetUnit))
                {
                    Event newEvent = new(ent.ParentStep, this,Parent)
                    {
                        Type = Event.EventType.Mod
                        ,TargetUnit = ent.TargetUnit,
                        Modification = new Mod(Parent)
                        {
                            Type = Mod.ModType.Debuff, 
                            Effects= new (){new (){EffType = Effect.EffectType.EffectResPrc, Value = -0.20}}
                        }
                    };

                    ent.ParentStep.AddEvent(newEvent,true);
                }
            }
          
            //TODO handle enemy break shield by any of party members(A6?)

            base.DefaultFighter_HandleEvent(ent);
        }


        public double? CalculateFqpDmg(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.Stats.Attack* 0.8, ent);
        }
        //50-110
        public double? CalculateBasicDmg(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.Stats.Attack*(0.4 + (Parent.Skills.FirstOrDefault(x=>x.Name=="System Warning").Level*0.1)), ent);
        }

        //get 0.2 AllDmg per debuff  on enemy Team
        public static double? CalculateE6(Event ent)
        {
            double maxDebufs = 5;
            double debufs = 0;
        
            
                debufs += ent.TargetUnit.Mods.Count(x => x.Type == Mod.ModType.Debuff||x.Type == Mod.ModType.Dot);
                if (debufs > maxDebufs)
                {
                    debufs = maxDebufs;
                  
                }

            return debufs*0.2;
        }
        public SilverWolf(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Quantum;
            Parent.Stats.BaseMaxEnergy = 110;


            //=====================
            //Abilities
            //=====================

            Ability ability;
            //Force Quit Program
            ability = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Technique
                , Name = "Force Quit Program"
                , Cost = 1
                , CostType = Resource.ResourceType.TP
                , Element = Element
                , AdjacentTargets = Ability.AdjacentTargetsEnm.All
                , Attack=true
                , IgnoreWeakness=true
            };
            //dmg events
            ability.Events.Add(new Event(null, this, this.Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.DirectDamage, CalculateValue = CalculateFqpDmg,  AbilityValue = ability });
            //shield break in this case going after skill dmg
            ability.Events.Add(new Event(null, this, this.Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.ResourceDrain,ResType = Resource.ResourceType.Toughness, Val = 60, AbilityValue = ability });
      
            Abilities.Add(ability);


            Ability SystemWarning;
            //Force Quit Program
            SystemWarning = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Basic
                , Name = "System Warning"
                , CostType = Resource.ResourceType.TP
                , Element = Element
                , AdjacentTargets = Ability.AdjacentTargetsEnm.None
                , Attack=true
                , ToughnessShred = 30
            };
            //dmg events
            SystemWarning.Events.Add(new Event(null, this, this.Parent) {Type = Event.EventType.DirectDamage, CalculateValue = CalculateBasicDmg,  AbilityValue = SystemWarning });
            SystemWarning.Events.Add(new Event(null, this, this.Parent) {Type = Event.EventType.PartyResourceGain,ResType = Resource.ResourceType.SP,TargetUnit = Parent, Val = 1,  AbilityValue = SystemWarning });
      
            Abilities.Add(SystemWarning);


            if (Parent.Rank >= 6)
            {
                PassiveMods.Add(new PassiveMod(Parent)
                {
                    Mod = new Mod(Parent)
                        { Effects = new List<Effect>() { new Effect() { EffType = Effect.EffectType.AllDamageBoost, CalculateValue = CalculateE6 } } },
                    Target = Parent,
                    IsTargetCheck = true

                });
            }

          
        }


    }
}
