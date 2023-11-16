using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character;

public class SilverWolf : DefaultFighter
{
    public SilverWolf(Unit parent) : base(parent)
    {
        Parent.Stats.BaseMaxEnergy = 110;


        //=====================
        //Abilities
        //=====================

        Ability ability;
        //Force Quit Program
        ability = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Technique, Name = "Force Quit Program", Cost = 1,
            CostType = Resource.ResourceType.TP, Element = Element, AdjacentTargets = Ability.AdjacentTargetsEnm.All,
            IgnoreWeakness = true
        };
        //dmg events
        ability.Events.Add(new DirectDamage(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue, CalculateValue = CalculateFqpDmg,
            AbilityValue = ability
        });
        //shield break in this case going after skill dmg
        ability.Events.Add(new ResourceDrain(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue, ResType = Resource.ResourceType.Toughness, Val = 60,
            AbilityValue = ability
        });

        Abilities.Add(ability);


        Ability SystemWarning;
        //System Warning
        SystemWarning = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic, Name = "System Warning", Element = Element,
            AdjacentTargets = Ability.AdjacentTargetsEnm.None, EnergyGain = 20, SpGain = 1
        };
        //dmg events
        SystemWarning.Events.Add(new DirectDamage(null, this, Parent)
            { CalculateValue = CalculateBasicDmg, AbilityValue = SystemWarning });
        SystemWarning.Events.Add(new ToughnessShred(null, this, Parent) { Val = 30, AbilityValue = SystemWarning });

        Abilities.Add(SystemWarning);


        if (Parent.Rank >= 6)
            PassiveMods.Add(new PassiveMod(Parent)
            {
                Mod = new Buff(Parent)
                    { Effects = new List<Effect> { new EffAllDamageBoost { CalculateValue = CalculateE6 } } },
                Target = Parent,
                IsTargetCheck = true
            });
    }

    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Nihility;
    public override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Quantum;

    public override void DefaultFighter_HandleEvent(Event ent)
    {
        //if unit consume hp or got attack then apply buff
        if (Parent.Rank >= 2 && Parent.IsAlive && ent is UnitEnteringBattle)
            //if enemy enter combat need debuff
            if (Parent.Enemies.Any(x => x == ent.TargetUnit))
            {
                ApplyBuff newEvent = new(ent.ParentStep, this, Parent)
                {
                    TargetUnit = ent.TargetUnit,
                    BuffToApply = new Buff(Parent)
                    {
                        Type = Buff.ModType.Debuff,
                        Effects = new List<Effect> { new EffEffectResPrc { Value = -0.20 } }
                    }
                };

                ent.ChildEvents.Add(newEvent);
            }

        //TODO handle enemy break shield by any of party members(A6?)

        base.DefaultFighter_HandleEvent(ent);
    }


    public double? CalculateFqpDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(null) * 0.8, ent);
    }

    //50-110
    public double? CalculateBasicDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(
            Parent.GetAttack(null) * (0.4 + Parent.Skills.FirstOrDefault(x => x.Name == "System Warning").Level * 0.1),
            ent);
    }

    //get 0.2 AllDmg per debuff  on target
    public static double? CalculateE6(Event ent)
    {
        double maxDebufs = 5;
        double debufs = 0;


        debufs += ent.TargetUnit.Buffs.Count(x => x.Type == Buff.ModType.Debuff || x.Type == Buff.ModType.Dot);
        if (debufs > maxDebufs) debufs = maxDebufs;

        return debufs * 0.2;
    }
}