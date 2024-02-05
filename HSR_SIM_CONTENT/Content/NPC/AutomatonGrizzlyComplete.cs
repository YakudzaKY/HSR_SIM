using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.NPC;

internal class AutomatonGrizzlyComplete : DefaultNPCBossFIghter
{
    public AutomatonGrizzlyComplete(Unit parent) : base(parent)
    {
        //Elemenet
        Element = Unit.ElementEnm.Physical;

        NativeWeaknesses.Add(Unit.ElementEnm.Fire);
        NativeWeaknesses.Add(Unit.ElementEnm.Lightning);
        NativeWeaknesses.Add(Unit.ElementEnm.Ice);
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Physical, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Wind, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Quantum, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Imaginary, ResistVal = 0.20 });
        DebuffResists.Add(new DebuffResist { Debuff = typeof(EffFreeze), ResistVal = 0.5 });
        DebuffResists.Add(new DebuffResist { Debuff = typeof(EffImprisonment), ResistVal = 0.5 });
        DebuffResists.Add(new DebuffResist { Debuff = typeof(EffEntanglement), ResistVal = 0.5 });

        Ability myAttackAbility;
        //Deals minor Physical DMG (250% ATK) to a single target.
        myAttackAbility = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "Shovel Attack",
            Element = Element
        };
        //dmg events
        myAttackAbility.Events.Add(new DirectDamage(null, this, Parent)
            { CalculateValue = CalcMyAttack });
        myAttackAbility.Events.Add(new EnergyGain(null, this, Parent) { Val = 10 });
        Abilities.Add(myAttackAbility);
    }

    public double? CalcMyAttack(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(ent) * 2.5, ent);
    }
}