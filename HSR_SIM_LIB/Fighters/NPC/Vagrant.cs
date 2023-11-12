using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.NPC;

public class Vagrant : DefaultNPCFighter
{
    public Vagrant(Unit parent) : base(parent)
    {
        //Elemenet
        Element = Unit.ElementEnm.Physical;

        Weaknesses.Add(Unit.ElementEnm.Fire);
        Weaknesses.Add(Unit.ElementEnm.Ice);
        Weaknesses.Add(Unit.ElementEnm.Imaginary);
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
            AbilityValue = ability,
            BuffToApply = new Buff(Parent)
            {
                AbilityValue = ability, BaseDuration = 2, Effects = new List<Effect> { new EffAtkPrc { Value = 0.3 } }
            }
        });
        ability.Events.Add(new AdvanceAV(null, this, Parent) { AbilityValue = ability });
        Abilities.Add(ability);

        Ability myAttackAbility;
        //Deals minor Physical DMG (250% ATK) to a single target.
        myAttackAbility = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "Shovel Attack",
            Element = Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.None,
            Attack = true,
            SpGain = 1
        };
        //dmg events
        myAttackAbility.Events.Add(new DirectDamage(null, this, Parent)
            { CalculateValue = CalcMyAttack, AbilityValue = myAttackAbility });
        myAttackAbility.Events.Add(new EnergyGain(null, this, Parent) { Val = 10, AbilityValue = myAttackAbility });
        Abilities.Add(myAttackAbility);
    }

    public double? CalcMyAttack(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(ent) * 2.5, ent);
    }
}