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
            ability = new Ability(Parent) { AbilityType = Ability.AbilityTypeEnm.Technique, Name = "Test", Cost = 0, CostType = Resource.ResourceType.TP, Element =Element,EnterCombat = true};
            ability.Events.Add(new Event(null) { OnStepType = Step.StepTypeEnm.ExecuteAbilityUse, Type = Event.EventType.CombatStartSkillQueue });
            Abilities.Add(ability);
        }
    }
}
