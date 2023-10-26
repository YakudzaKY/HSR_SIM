using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class Luocha:DefaultFighter
    {
        public  override FighterUtils.PathType? Path { get; set; } = FighterUtils.PathType.Abundance;
        private Ability cycleOfLife;
        private readonly double cycleOfLifeMaxCnt = 2;
        public override string GetSpecialText()
        {
            return $"CoL: {(int)Mechanics.Values[cycleOfLife]:d}\\{(int)cycleOfLifeMaxCnt:d}";
        }
        public Luocha(Unit parent) : base(parent)
        {
            //Elemenet
            Element = Unit.ElementEnm.Imaginary;
            Ability ability;
            

            //=====================
            //Abilities
            //=====================
            //Cycle of life
            cycleOfLife = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.FollowUpAction
                , Name = "Cycle of life"
                , Element = Element
            };
            Mechanics.AddVal(cycleOfLife);
            Abilities.Add(cycleOfLife);

            //Mercy of a Fool
            ability = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Technique
                , Name = "Mercy of a Fool"
                , Cost = 1
                , CostType = Resource.ResourceType.TP
                , Element = Element
            };
            ability.Events.Add(new Event(null, this,Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, TargetUnit = Parent,Type = Event.EventType.MechanicValChg, Val = 2, AbilityValue = cycleOfLife });
            Abilities.Add(ability);

        }
    }
}
