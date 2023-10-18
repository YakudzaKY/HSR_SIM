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
    public class Bronya:DefaultFighter
    {
        public  override FighterUtils.PathType? Path { get; set; } = FighterUtils.PathType.Harmony;
        public Bronya(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Wind;
            Ability ability;
            //buff tech
            ability = new Ability(Parent) { AbilityType = Ability.AbilityTypeEnm.Technique, Name = "Banner of Command", Cost = 1, CostType = Resource.ResourceType.TP, Element = Element };
            //buff apply

            Event eventBuff = new(null)
                { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.Mod, AbilityValue = ability };
            eventBuff.Mods.Add(new Mod(null){Type=Mod.ModType.Buff,Modifier = Mod.ModifierType.AtkPrc,Value = 0.15,BaseDuration= 2,Dispellable = true,CalculateTargets = GetFriends});
            ability.Events.Add(eventBuff);

            Abilities.Add(ability);

        }
    }
}
