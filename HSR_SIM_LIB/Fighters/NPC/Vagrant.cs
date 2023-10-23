using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.NPC
{
    public class Vagrant: DefaultNPCFighter
    {


        public Vagrant(Unit parent) : base(parent)
        {    
            //Elemenet
            Element = Unit.ElementEnm.Physical;

            Weaknesses.Add(Unit.ElementEnm.Fire);
            Weaknesses.Add(Unit.ElementEnm.Ice);
            Weaknesses.Add(Unit.ElementEnm.Imaginary);
            //resist
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Physical,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Lightning,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Wind,ResistVal=0.20});
            Resists.Add(new Resist(){ResistType=Unit.ElementEnm.Quantum,ResistVal=0.20});

            var ability = new Ability(Parent)
            {
                AbilityType = Ability.AbilityTypeEnm.Ability,
                TargetType = Ability.TargetTypeEnm.Friend,
                Name = "Test",
                Cost = 1,
                Cooldown=1,
                Element = Element,
                ToughnessShred = 60,
                Attack = true
            };
            //dmg events
            ability.Events.Add(new Event(null, this,Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.DirectDamage, CanSetToZero = false, Val=1, AbilityValue = ability });


            Abilities.Add(ability);


        }
    }
}
