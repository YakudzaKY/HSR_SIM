using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Fighters.Special
{
    public class ForgottenHall:DefaultNPCFighter
    {
        public ForgottenHall(Unit parent) : base(parent)
        {
              
            Ability IncreaseCycle;
            //Karma Wind
            IncreaseCycle = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Ability
                , Name = "Cycle Set"
                , Attack=false
                , TargetType = Ability.TargetTypeEnm.Self
                , AdjacentTargets =AdjacentTargetsEnm.None
                , EndTheTurn= true
         
            };
            //dmg events
            IncreaseCycle.Events.Add(new Event(null, this,this.Parent) { Type = Event.EventType.IncreaseLevel, Val = 1, TargetType =TargetTypeEnm.Self,  AbilityValue = IncreaseCycle});

            IncreaseCycle.Events.Add(new(null, this, this.Parent)
            {   Type = Event.EventType.Mod, AbilityValue = IncreaseCycle,
                Modification = (new Mod(Parent)
                {
                    Type = Mod.ModType.Buff,
                    Effects = new List<Effect>() { new Effect() { EffType = Effect.EffectType.ReduceBAV ,Value = 50 } },
                    BaseDuration = 1, Dispellable = false
                })
            });
            Abilities.Add(IncreaseCycle);
        }
    }
}
