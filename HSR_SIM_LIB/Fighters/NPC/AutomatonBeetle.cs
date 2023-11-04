using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Effect;

namespace HSR_SIM_LIB.Fighters.NPC
{
    public class AutomatonBeetle:DefaultNPCFighter
    {     
        
    /*   dobule shield for testing shields
        public override void DefaultFighter_HandleEvent(Event ent)
        {
            //if unit consume hp or got attack then apply buff
            if (ent.Type==Event.EventType.UnitEnteringBattle&&ent.TargetUnit==this.Parent)
            {
                Event newEvent = new (ent.ParentStep,ent, Parent)
                {
                    Type = Event.EventType.Mod
                    
                    ,TargetUnit = this.Parent
                    ,Modification = new Mod(Parent)
                    {
                        BaseDuration = 2,
                        Type = Mod.ModType.Buff,
                        Effects = new List<Effect>(){new Effect(){Value = FighterUtils.CalculateShield(4000,ent,Parent),EffType = Effect.EffectType.Shield}}
                        
                        
                    }
                    
                };


                newEvent = new (ent.ParentStep,ent, Parent)
                {
                    Type = Event.EventType.Mod
                    
                    ,TargetUnit = this.Parent
                    ,Modification = new Mod(Parent)
                    {
                        BaseDuration = 2,
                        Type = Mod.ModType.Buff,
                        Effects = new List<Effect>(){new Effect(){Value = FighterUtils.CalculateShield(16000,ent,Parent),EffType = Effect.EffectType.Shield}}
                        
                        
                    }
                    
                };
 
            }
            base.DefaultFighter_HandleEvent(ent);
        }*/

        public AutomatonBeetle(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Physical;

            Weaknesses.Add(Unit.ElementEnm.Wind);
            Weaknesses.Add(Unit.ElementEnm.Lightning);
            Weaknesses.Add(Unit.ElementEnm.Imaginary);
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Lightning,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Physical,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Ice,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Quantum,ResistVal=0.20});

            Ability SystemWarning;
            //System Warning
            SystemWarning = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Basic
                , Name = "FIX THIS SHIT!!!"
                , Element = Element
                , AdjacentTargets = Ability.AdjacentTargetsEnm.None
                , Attack=true
                , EnergyGain = 20
                , SpGain = 1
            };
            //dmg events
            SystemWarning.Events.Add(new DirectDamage(null, this, this.Parent) { Val=1,  AbilityValue = SystemWarning });
            Abilities.Add(SystemWarning);


        }
    }
}
