using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.NPC;

public class Vagrant : DefaultNPCFighter
{
    private static readonly AppliedBuff AtkAppliedBuff = new(null)
    {
        BaseDuration = 2, Effects = new List<Effect> { new EffAtkPrc { Value = 0.3 } }
    };

    public Vagrant(Unit? parent) : base(parent)
    {
        //Elemenet
        Element = Unit.ElementEnm.Physical;

        NativeWeaknesses.Add(Unit.ElementEnm.Fire);
        NativeWeaknesses.Add(Unit.ElementEnm.Ice);
        NativeWeaknesses.Add(Unit.ElementEnm.Imaginary);
        //resist
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Physical, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Lightning, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Wind, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Quantum, ResistVal = 0.20 });

        var ability = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Ability,
            TargetType = Ability.TargetTypeEnm.Friend,
            Name = "Inspire",
            Cooldown = 2
        };
        //dmg events
        ability.Events.Add(new ApplyBuff(null, this, Parent)
        {
            AppliedBuffToApply = AtkAppliedBuff
        });
        ability.Events.Add(new AdvanceAV(null, this, Parent));
        Abilities.Add(ability);

        Ability? myAttackAbility;
        //Deals minor Physical DMG (250% ATK) to a single target.
        myAttackAbility = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "Shovel Attack",
            Element = Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.None
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