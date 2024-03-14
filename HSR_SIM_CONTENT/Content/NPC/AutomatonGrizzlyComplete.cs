using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CONTENT.Content.NPC;

internal class AutomatonGrizzlyComplete : DefaultNPCBossFIghter
{
    public AutomatonGrizzlyComplete(Unit? parent) : base(parent)
    {
        //Elemenet
        Parent.Element = Unit.ElementEnm.Physical;

        Parent.NativeWeaknesses.Add(Unit.ElementEnm.Fire);
        Parent.NativeWeaknesses.Add(Unit.ElementEnm.Lightning);
        Parent.NativeWeaknesses.Add(Unit.ElementEnm.Ice);
        Parent.Resists.Add(new Resist { ResistType = Unit.ElementEnm.Physical, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Unit.ElementEnm.Wind, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Unit.ElementEnm.Quantum, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Unit.ElementEnm.Imaginary, ResistVal = 0.20 });
        Parent.DebuffResists.Add(new DebuffResist { Debuff = typeof(EffFreeze), ResistVal = 0.5 });
        Parent.DebuffResists.Add(new DebuffResist { Debuff = typeof(EffImprisonment), ResistVal = 0.5 });
        Parent.DebuffResists.Add(new DebuffResist { Debuff = typeof(EffEntanglement), ResistVal = 0.5 });

        //TODO: need implement boss abilities
        var myAttackAbility =
            //Deals minor Physical DMG (250% ATK) to a single target.
            new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Basic,
                Name = "Shovel Attack",
                Element = Parent.Element
            };
        //dmg events
        myAttackAbility.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = FighterUtils.DamageFormula(new Formula()
            {
                Expression =
                    $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAttack)} * 2.5 "
            })
        });
        myAttackAbility.Events.Add(new EnergyGain(null, this, Parent) { Value = 10 });
        Abilities.Add(myAttackAbility);
    }
}