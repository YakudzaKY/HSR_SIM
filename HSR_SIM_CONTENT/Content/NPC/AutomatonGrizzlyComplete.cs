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