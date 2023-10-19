using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class Luocha:DefaultFighter
    {
        public  override FighterUtils.PathType? Path { get; set; } = FighterUtils.PathType.Abundance;
        public Luocha(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Imaginary;
            Ability ability;
            //Karma Wind
            ability = new Ability(Parent) { AbilityType = Ability.AbilityTypeEnm.Technique, Name = "Test", Cost = 0, CostType = Resource.ResourceType.TP, Element =Element,Attack = true};
            ability.Events.Add(new Event(null,this) { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.CombatStartSkillQueue });
            Abilities.Add(ability);
        }
    }
}
