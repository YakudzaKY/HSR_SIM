using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CONTENT.Content.NPC;

internal class AutomatonGrizzlyComplete : DefaultNpcBossFighter
{
    public AutomatonGrizzlyComplete(Unit parent) : base(parent)
    {

        Parent.NativeWeaknesses.Add(Ability.ElementEnm.Fire);
        Parent.NativeWeaknesses.Add(Ability.ElementEnm.Lightning);
        Parent.NativeWeaknesses.Add(Ability.ElementEnm.Ice);
        Parent.Resists.Add(new Resist { ResistType = Ability.ElementEnm.Physical, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Ability.ElementEnm.Wind, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Ability.ElementEnm.Quantum, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Ability.ElementEnm.Imaginary, ResistVal = 0.20 });
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
            };
        //dmg events
        myAttackAbility.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = FighterUtils.DamageFormula(new Formula()
            {
                Expression =
                    $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.Attack)} * 2.5 "
            })
        });
        myAttackAbility.Events.Add(new EnergyGain(null, this, Parent) { Value = 10 });
        Abilities.Add(myAttackAbility);
    }
}