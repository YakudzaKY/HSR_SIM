using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static HSR_SIM_LIB.Ability;


namespace HSR_SIM_LIB.Fighters.Character
{
    public class Blade : DefaultFighter
    {
        public  override FighterUtils.PathType? Path { get; set; } = FighterUtils.PathType.Destruction;
        private int ShuHuCnt ;
        private readonly int ShuHuMaxCnt ;
        public override Ability ChooseAbilityToCast(Step step)
        {
            Ability watAbility = null;

            return watAbility ?? base.ChooseAbilityToCast(step);
        }


        public double? CalculateKarmaSelfDmg(Event ent)
        {
            return Parent.Stats.MaxHp*0.2;
        }

        public double? CalculateKarmaDmg(Event ent)
        {
            return  FighterUtils.CalculateBasicDmg(Parent.Stats.MaxHp*0.4,ent);
        }

        public override string GetSpecialText()
        {
            return $"SH: {ShuHuCnt:d}\\{ShuHuMaxCnt:d}";
        }

        //Blade constructor
        public Blade(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Wind;
            Ability ability;
            //Karma Wind
            ability = new Ability(Parent) { AbilityType = Ability.AbilityTypeEnm.Technique, Name = "Karma Wind", Cost = 1, CostType = Resource.ResourceType.TP, Element = Element,CalculateToughnessShred= DefaultFighter.CalculateOpeningThg };
            ability.Events.Add(new Event(null) { OnStepType = Step.StepTypeEnm.ExecuteAbilityUse, Type = Event.EventType.CombatStartSkillQueue });
            ability.Events.Add(new Event(null) { OnStepType = Step.StepTypeEnm.ExecuteAbilityUse, Type = Event.EventType.EnterCombat });
            //dmg events
            ability.Events.Add(new Event(null) { OnStepType = Step.StepTypeEnm.ExecuteStartQueue, Type = Event.EventType.ResourceDrain,ResType = Resource.ResourceType.HP, TargetUnit = Parent, CanSetToZero = false, CalculateValue = CalculateKarmaSelfDmg ,AbilityValue = ability});
            ability.Events.Add(new Event(null) { OnStepType = Step.StepTypeEnm.ExecuteStartQueue, Type = Event.EventType.DirectDamage,CalculateValue = CalculateKarmaDmg, CalculateTargets = GetAoeTargets ,AbilityValue = ability});
            //Dequeue
            ability.Events.Add(new Event(null) { OnStepType = Step.StepTypeEnm.ExecuteStartQueue, Type = Event.EventType.CombatStartSkillDeQueue});
            Abilities.Add(ability);
            ShuHuMaxCnt = (parent.Rank==6 )? 4 : 5;//4 stacks on 6 eidolon 

        }
    }
}
