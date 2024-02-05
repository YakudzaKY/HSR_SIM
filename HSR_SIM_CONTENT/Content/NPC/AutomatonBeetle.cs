using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.NPC;

public class AutomatonBeetle : DefaultNPCFighter
{
    private readonly Buff barierBuff;

    public AutomatonBeetle(Unit parent) : base(parent)
    {
        barierBuff = new Buff(Parent)
            { EventHandlerProc = MyBarrierEventHandler, Effects = new List<Effect> { new EffBarrier() } };
        //Elemenet
        Element = Unit.ElementEnm.Physical;

        NativeWeaknesses.Add(Unit.ElementEnm.Wind);
        NativeWeaknesses.Add(Unit.ElementEnm.Lightning);
        NativeWeaknesses.Add(Unit.ElementEnm.Imaginary);
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Lightning, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Physical, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Ice, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Quantum, ResistVal = 0.20 });

        Ability myAttackAbility;
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
            { CalculateValue = CalcMyAttack});
        myAttackAbility.Events.Add(new EnergyGain(null, this, Parent) { Val = 15 });
        myAttackAbility.Events.Add(new ApplyBuff(null, this, Parent)
            { TargetUnit = Parent, BuffToApply = barierBuff });
        Abilities.Add(myAttackAbility);
    }

    public double? CalcMyAttack(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(ent) * 3, ent);
    }

    public void MyBarrierEventHandler(Event ent)
    {
        if (ent is DirectDamage && ent.TargetUnit == Parent &&
            ent.TargetUnit.Buffs.Any(x => x.Effects.Any(y => y is EffBarrier)))
        {
            RemoveBuff newEvent = new(ent.ParentStep, this, Parent)
            {
                TargetUnit = Parent,
                BuffToApply = barierBuff
            };
            ent.ParentStep.Events.Add(newEvent);
        }
    }
}