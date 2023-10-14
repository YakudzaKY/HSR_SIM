﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HSR_SIM_LIB.Fighters.Character
{
    public class Luocha:DefaultFighter
    {
        public Luocha(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Imaginary;
            Ability ability;
            //Karma Wind
            ability = new Ability(Parent) { AbilityType = Ability.AbilityTypeEnm.Technique, Name = "Test", Cost = 1, CostType = Resource.ResourceType.TP, Element =Element};
            ability.Events.Add(new Event() { OnStepType = Step.StepTypeEnm.ExecuteAbilityUse, Type = Event.EventType.CombatStartSkillQueue });
            ability.Events.Add(new Event() { OnStepType = Step.StepTypeEnm.ExecuteAbilityUse, Type = Event.EventType.EnterCombat });
            Abilities.Add(ability);
        }
    }
}