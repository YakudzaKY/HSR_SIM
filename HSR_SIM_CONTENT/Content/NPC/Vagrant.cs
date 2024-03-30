using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CONTENT.Content.NPC;

public class Vagrant : DefaultNPCFighter
{
    //reff for unique stacking
    private  static readonly AppliedBuff AtkBuffReference = new(null, null, typeof(Vagrant));

    public Vagrant(Unit parent) : base(parent)
    {
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
            AppliedBuffToApply = new AppliedBuff(Parent,AtkBuffReference,this) {   BaseDuration = 2, Effects = new List<Effect> { new EffAtkPrc { Value = 0.3 } }}
        });
        ability.Events.Add(new AdvanceAV(null, this, Parent));
        Abilities.Add(ability);

        Ability? myAttackAbility;
        //Deals minor Physical DMG (250% ATK) to a single target.
        myAttackAbility = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "Shovel Attack",
            AdjacentTargets = Ability.AdjacentTargetsEnm.None
        };
        //dmg events
        myAttackAbility.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = FighterUtils.DamageFormula(new Formula()
            {
                Expression =
                    $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.Attack)} * 2.5"
            })
        });
        myAttackAbility.Events.Add(new EnergyGain(null, this, Parent) { Value = 10 });
        Abilities.Add(myAttackAbility);
    }
}