using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class Bronya:DefaultFighter
    {
        public Bronya(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Wind;
            Ability ability;
            //Karma Wind
            ability = new Ability(Parent) { AbilityType = Ability.AbilityTypeEnm.Technique, Name = "Banner of Command", Cost = 1, CostType = Resource.ResourceType.TP, Element = Element };
            ability.Events.Add(new Event(null) { OnStepType = Step.StepTypeEnm.ExecuteAbilityUse, Type = Event.EventType.CombatStartSkillQueue });
            //buff apply

            Event eventBuff = new(null)
                { OnStepType = Step.StepTypeEnm.ExecuteStartQueue, Type = Event.EventType.Mod, AbilityValue = ability };
            eventBuff.Mods.Add(new Mod(){Type=Mod.ModType.Buff,Modifier = Mod.ModifierType.AtkPrc,Value = 15,Duration=2,Dispellable = true,CalculateTargets = GetFriends});
            ability.Events.Add(eventBuff);
            //Dequeue
            ability.Events.Add(new Event(null) { OnStepType = Step.StepTypeEnm.ExecuteStartQueue, Type = Event.EventType.CombatStartSkillDeQueue});
            Abilities.Add(ability);

        }
    }
}
