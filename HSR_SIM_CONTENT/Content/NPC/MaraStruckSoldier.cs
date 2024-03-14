using System.Runtime.CompilerServices;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CONTENT.Content.NPC;

public class MaraStruckSoldier : DefaultNPCFighter
{
    //static because max 5 stacks by all units this type
    private static readonly AppliedBuff myDoTRef = new(null, null, typeof(MaraStruckSoldier))
    {
        Type = Buff.BuffType.Dot,
        Effects = []
    };

    private readonly AppliedBuff myDotDeAppliedBuff;
    private readonly Ability? Rejuvenate;
    private readonly AppliedBuff uniqueAppliedBuff;

    public MaraStruckSoldier(Unit? parent) : base(parent)
    {
        //Elemenet
        Parent.Element = Unit.ElementEnm.Wind;

        Parent.NativeWeaknesses.Add(Unit.ElementEnm.Fire);
        Parent.NativeWeaknesses.Add(Unit.ElementEnm.Ice);
        Parent.NativeWeaknesses.Add(Unit.ElementEnm.Quantum);
        Parent.Resists.Add(new Resist { ResistType = Unit.ElementEnm.Physical, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Unit.ElementEnm.Lightning, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Unit.ElementEnm.Wind, ResistVal = 0.20 });
        Parent.Resists.Add(new Resist { ResistType = Unit.ElementEnm.Imaginary, ResistVal = 0.20 });
        myDotDeAppliedBuff = new AppliedBuff(Parent, null, this)
        {
            Reference = myDoTRef,
            Type = Buff.BuffType.Dot,
            BaseDuration = 3,
            Stack = 2,
            MaxStack = 5,
            Effects = new List<Effect>
            {
                new EffWindShear
                {
                    DoTCalculateValue = FighterUtils.DamageFormula(new Formula()
                    {
                        Expression =
                            $"0.5 * {Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetMaxHp)}"
                    })
                }
            }
        };
        uniqueAppliedBuff = new AppliedBuff(Parent, null, this)
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
            FollowUpPriority = Ability.PriorityEnm.DefeatHandler,
            TargetType = Ability.TargetTypeEnm.Self
        };
        // Rejuvenate.Events.Add(new RemoveBuff(null,this,Parent) {TargetUnit = Parent,BuffToApply = uniqueBuff});
        Rejuvenate.Events.Add(new Healing(null, this, Parent) { CalculateValue = CalculateReHeal });
        Rejuvenate.Events.Add(new ResourceGain(null, this, Parent)
            { ResType = Resource.ResourceType.Toughness, Value = Parent.Stats.MaxToughness });
        Abilities.Add(Rejuvenate);

        Ability? myAttackAbility;
        //Deals minor Physical DMG (250% ATK) to a single target.
        myAttackAbility = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "Callous Tailwind",
            Element = Parent.Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.None
        };
        //dmg events

        foreach (var proportion in new[] { 0.25, 0.25, 0.5 })
        {
            myAttackAbility.Events.Add(new DirectDamage(null, this, Parent)
            {
                CalculateValue = FighterUtils.DamageFormula(new Formula()
                {
                    Expression =
                        $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas)}#{nameof(UnitFormulas.GetAttack)} * 2 * {proportion}"
                })
            });
            myAttackAbility.Events.Add(
                new EnergyGain(null, this, Parent) { Value = 15 * proportion });
        }

        myAttackAbility.Events.Add(new AttemptEffect(null, this, Parent)
            { BaseChance = 1, AppliedBuffToApply = myDotDeAppliedBuff });

        Abilities.Add(myAttackAbility);
    }


    public double? CalculateReHeal(Event ent)
    {
        return FighterUtils.CalculateHealByBasicVal(ent.TargetUnit.GetMaxHp(ent) * 0.5, ent);
    }

    protected override void DefaultFighter_HandleEvent(Event ent)
    {
        if (ent is UnitEnteringBattle && ent.TargetUnit == Parent)
        {
            ApplyBuff newEvent = new(ent.ParentStep, this, Parent)
            {
                TargetUnit = ent.TargetUnit,
                AppliedBuffToApply = uniqueAppliedBuff
            };
            ent.ChildEvents.Add(newEvent);
        }

        base.DefaultFighter_HandleEvent(ent);
    }

    private bool RejuvenateAvailable()
    {
        return Parent.AppliedBuffs.Any(x => x.Reference == uniqueAppliedBuff) ||
               Parent.LivingStatus == Unit.LivingStatusEnm.WaitingForFollowUp;
    }
}