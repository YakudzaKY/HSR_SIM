using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_CONTENT.Content.Special;

public class ForgottenHall : DefaultNPCFighter
{
    public ForgottenHall(Unit? parent) : base(parent)
    {
        Ability? IncreaseCycle;
        //Karma Wind
        IncreaseCycle = new Ability(this)
        {
            AbilityType = AbilityTypeEnm.Ability,
            Name = "Cycle Set",
            TargetType = TargetTypeEnm.Self,
            AdjacentTargets = AdjacentTargetsEnm.None,
            EndTheTurn = true
        };
        //dmg events
        IncreaseCycle.Events.Add(new IncreaseLevel(null, this, Parent)
            { Val = 1, TargetType = TargetTypeEnm.Self });

        IncreaseCycle.Events.Add(new ApplyBuff(null, this, Parent)
        {
            AppliedBuffToApply = new AppliedBuff(Parent)
            {
                Type = AppliedBuff.BuffType.Buff,
                Effects = new List<Effect> { new EffReduceBAV { Value = 50 } },
                BaseDuration = 1,
                Dispellable = false
            }
        });
        Abilities.Add(IncreaseCycle);
    }

    public override void DefaultFighter_HandleEvent(Event ent)
    {
        // wipe party if 1000+cycles
        if (ent.TargetUnit == Parent && ent is IncreaseLevel && Parent.Level >= 1000)
            foreach (var unit in Parent.ParentTeam.ParentSim.PartyTeam.Units)
            {
                ResourceDrain newEvent = new(ent.ParentStep, this, Parent)
                {
                    TargetUnit = unit,
                    CanSetToZero = true,
                    ResType = Resource.ResourceType.HP,
                    Val = unit.GetRes(Resource.ResourceType.HP).ResVal
                };

                ent.ChildEvents.Add(newEvent);
            }

        base.DefaultFighter_HandleEvent(ent);
    }
}