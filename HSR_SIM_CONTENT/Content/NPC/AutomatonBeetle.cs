using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CONTENT.Content.NPC;

public class AutomatonBeetle : DefaultNPCFighter
{
    private readonly AppliedBuff barierAppliedBuff;

    public AutomatonBeetle(Unit parent) : base(parent)
    {
        barierAppliedBuff = new AppliedBuff(Parent, null, this)
            { EventHandlerProc = MyBarrierEventHandler, Effects = new List<Effect> { new EffBarrier() } };
        //Elemenet
        Element = Ability.ElementEnm.Physical;

        Parent.NativeWeaknesses.Add(Ability.ElementEnm.Wind);
        Parent.NativeWeaknesses.Add(Ability.ElementEnm.Lightning);
        Parent.NativeWeaknesses.Add(Ability.ElementEnm.Imaginary);
        Parent.Resists.Add(new Resist { ResistType = Ability.ElementEnm.Lightning, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Ability.ElementEnm.Physical, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Ability.ElementEnm.Ice, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Ability.ElementEnm.Quantum, ResistVal = 0.20 });

        Ability? myAttackAbility;
        //Deals Physical DMG (300% ATK) to a single target, and grants a Barrier to self. The Barrier nullifies all DMG received except for DoT until after being attacked.
        myAttackAbility = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "Unstable Forcefield",
            Element = Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.None
        };
        //dmg events
        myAttackAbility.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = FighterUtils.DamageFormula(new Formula()
            {
                Expression =
                    $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAttack)} * 3 "
            })
        });
        myAttackAbility.Events.Add(new EnergyGain(null, this, Parent) { Value = 15 });
        myAttackAbility.Events.Add(new ApplyBuff(null, this, Parent)
            { TargetUnit = Parent, AppliedBuffToApply = barierAppliedBuff });
        myAttackAbility.Events.Add(new ApplyBuff(null, this, Parent)
            { TargetUnit = Parent, AppliedBuffToApply = barierAppliedBuff });
        Abilities.Add(myAttackAbility);
    }


    public void MyBarrierEventHandler(Event ent)
    {
        if (ent is DirectDamage && ent.TargetUnit == Parent &&
            ent.TargetUnit.AppliedBuffs.Any(x => x.Effects.Any(y => y is EffBarrier)))
        {
            RemoveBuff newEvent = new(ent.ParentStep, this, Parent)
            {
                TargetUnit = Parent,
                AppliedBuffToApply = barierAppliedBuff,
                NotFoundIgnore = true //Because this event at bottom of list (waiting all attack hits barrier)
                //a lot of RemoveBuff events cant be generated
            };
            ent.ParentStep.Events.Add(newEvent);
        }
    }
}