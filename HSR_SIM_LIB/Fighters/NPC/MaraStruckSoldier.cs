using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using System.Linq;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.Skills.ReadyBuffs;

namespace HSR_SIM_LIB.Fighters.NPC;

public class MaraStruckSoldier : DefaultNPCFighter
{
    private Buff uniqueBuff;
    //static because max 5 stacks by all units this type
    private static Buff myDoTRef= new Buff(null, null);
    private Buff myDotDeBuff;
    private Ability Rejuvenate;

    public static double? CalcMyDoT(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(ent.SourceUnit.GetAttack(ent) * 0.5, ent);
    }

    public MaraStruckSoldier(Unit parent) : base(parent)
    {
        //Elemenet
        Element = Unit.ElementEnm.Wind;

        NativeWeaknesses.Add(Unit.ElementEnm.Fire);
        NativeWeaknesses.Add(Unit.ElementEnm.Ice);
        NativeWeaknesses.Add(Unit.ElementEnm.Quantum);
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Physical, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Lightning, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Wind, ResistVal = 0.20 });
        Resists.Add(new Resist { ResistType = Unit.ElementEnm.Imaginary, ResistVal = 0.20 });
        myDotDeBuff= new Buff(Parent, null)
        {
            Reference = myDoTRef,
            DoNotClone = true,
            Type = Buff.BuffType.Dot,
            BaseDuration = 3,
            Stack = 2,
            MaxStack = 5,
            Effects = new List<Effect>
            {
                new EffWindShear { DoTCalculateValue = CalcMyDoT }

            }
        };
        uniqueBuff = new Buff(Parent, null)
        {
            Dispellable = true,
            Type = Buff.BuffType.Buff,
            Effects = new List<Effect> { new EffRebirth() }
        };

        //=====================
        //Abilities
        //=====================
        //Rejuvenate
        Rejuvenate = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.FollowUpAction,
            Name = "Rejuvenate",
            Available = RejuvenateAvailable,
            Priority = Ability.PriorityEnm.DefeatHandler,
            TargetType = Ability.TargetTypeEnm.Self
        };
       // Rejuvenate.Events.Add(new RemoveBuff(null,this,Parent) {TargetUnit = Parent,BuffToApply = uniqueBuff});
        Rejuvenate.Events.Add(new Healing(null, this, Parent) {  CalculateValue = CalculateReHeal });
        Rejuvenate.Events.Add(new ResourceGain(null, this, Parent) { ResType = Resource.ResourceType.Toughness,Val = Parent.Stats.MaxToughness});
        Abilities.Add(Rejuvenate);

        Ability myAttackAbility;
        //Deals minor Physical DMG (250% ATK) to a single target.
        myAttackAbility = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "Callous Tailwind",
            Element = Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.None
        };
        //dmg events

        foreach (double proportion in new[] { 0.25, 0.25, 0.5 })
        {
            myAttackAbility.Events.Add(new DirectDamage(null, this, Parent)
            { CalculateValue = CalcMyAttack, CalculateProportion = proportion });
            myAttackAbility.Events.Add(new EnergyGain(null, this, Parent) { Val = 15, CalculateProportion = proportion });
        }
        myAttackAbility.Events.Add(new AttemptEffect(null, this, Parent) { BaseChance = 1, BuffToApply = myDotDeBuff });

        Abilities.Add(myAttackAbility);

       

    }

    public double? CalculateReHeal(Event ent)
    {
        return FighterUtils.CalculateHealByBasicVal(ent.TargetUnit.GetMaxHp(ent) * 0.5, ent);
    }

    public override void DefaultFighter_HandleEvent(Event ent)
    {

        if (ent is UnitEnteringBattle && ent.TargetUnit == Parent)
        {
            ApplyBuff newEvent = new(ent.ParentStep, this, Parent)
            {
                TargetUnit = ent.TargetUnit,
                BuffToApply = uniqueBuff

            };
            ent.ChildEvents.Add(newEvent);
        }
        base.DefaultFighter_HandleEvent(ent);
    }

    private bool RejuvenateAvailable()
    {
        return Parent.Buffs.Any(x => x.Reference == uniqueBuff)||Rejuvenate.FollowUpTargets.Count>0;
    }

    public double? CalcMyAttack(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(ent) * 2, ent);
    }


}