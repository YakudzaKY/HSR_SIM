using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Fighters.FighterUtils;
using static HSR_SIM_LIB.Skills.Ability;

namespace HSR_SIM_LIB.Fighters.Character;

public class Bronya : DefaultFighter
{
    private readonly int wbSkillLvl;
    private readonly int abilitySkillLvl;
    private readonly int ultimateSkillLvl;
    private readonly int talentSkillLvl;
    private Ability ability;
    public Bronya(Unit parent) : base(parent)
    {

        Parent.Stats.BaseMaxEnergy = 120;
        wbSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Windrider Bullet").Level;
        abilitySkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Combat Redeployment").Level;
        ultimateSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "The Belobog March").Level;
        talentSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Leading the Way").Level;

        //=====================
        //Abilities
        //=====================

        var techniqueAbility =
            new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Technique,
                Name = "Banner of Command",
                Cost = 1,
                CostType = Resource.ResourceType.TP,
                Element = Element,
                AdjacentTargets = Ability.AdjacentTargetsEnm.All,
                TargetType = Ability.TargetTypeEnm.Friend
            };

        //buff apply
        ApplyBuff eventBuff = new(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            BuffToApply = new Buff(Parent)
            {
                Type = Buff.BuffType.Buff,
                Effects = new List<Effect> { new EffAtkPrc { Value = 0.15 } },
                BaseDuration = 2,
                Dispellable = true
            }
        };
        techniqueAbility.Events.Add(eventBuff);

        Abilities.Add(techniqueAbility);


        //Windrider Bullet
        Ability windriderBullet;
        windriderBullet = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "Windrider Bullet",
            AdjacentTargets = Ability.AdjacentTargetsEnm.None,
            SpGain = 1
        };
        //dmg events

        windriderBullet.Events.Add(new DirectDamage(null, this, Parent)
        { CalculateValue = CalculateBasicDmg });
        windriderBullet.Events.Add(new ToughnessShred(null, this, Parent) { Val = 30 });
        windriderBullet.Events.Add(new ModActionValue(null, this, Parent){CalculateValue = CalcTalentAV });
        windriderBullet.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 20, TargetUnit = Parent });


        Abilities.Add(windriderBullet);

        //Ability
        ability = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Ability,
            TargetType = Ability.TargetTypeEnm.Friend,
            Name = "Combat Redeployment",
            Cooldown = 2
        };
        ability.Events.Add(new DispelShit(null, this, Parent));
        //dmg events
        ability.Events.Add(new ApplyBuff(null, this, Parent)
        {

            BuffToApply = new Buff(this.Parent, null) { BaseDuration = 1, Effects = { new EffAllDamageBoost() { Value = GetAbilityScaling(0.33, 0.66, abilitySkillLvl) } } }
        });
        ability.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 30, TargetUnit = Parent });
        Abilities.Add(ability);


        //ultimate
        var ultimate =
            new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Ultimate,
                Priority = PriorityEnm.Ultimate,
                Name = "The Belobog March",
                CostType = Resource.ResourceType.Energy,
                Cost = Parent.Stats.BaseMaxEnergy,
                AdjacentTargets = Ability.AdjacentTargetsEnm.All,
                TargetType = Ability.TargetTypeEnm.Friend,
                Available = UltimateAvailable
            };

        //buff apply
        ApplyBuff ultimateBuff = new(null, this, Parent)
        {
            BuffToApply = new Buff(Parent)
            {
                CustomIconName = "The_Belobog_March",
                Type = Buff.BuffType.Buff,
                Effects = new List<Effect> { new EffAtkPrc { Value = GetAbilityScaling(0.33, 0.55, ultimateSkillLvl) }, new EffCritDmg { CalculateValue = CalcUltCritDmg } },
                BaseDuration = 2
            }
        };
        ultimate.Events.Add(ultimateBuff);
        ultimate.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 5, TargetUnit = Parent });

        Abilities.Add(ultimate);


        //=====================
        //Ascended Traces
        //=====================

        if (Atraces.HasFlag(ATracesEnm.A6))
            PassiveBuffs.Add(new PassiveBuff(Parent)
            {
                AppliedBuff = new Buff(Parent)
                { Effects = new List<Effect> { new EffAllDamageBoost { Value = 0.10 } } },
                Target = Parent.ParentTeam
            });
    }


    private double? CalcUltCritDmg(Event ent)
    {
        return Parent.GetCritDamage(ent) * GetAbilityScaling(0.12, 0.16, ultimateSkillLvl) + GetAbilityScaling(0.12, 0.20, ultimateSkillLvl);
    }

    private double? CalcTalentAV(Event ent)
    {
        return Parent.GetActionValue(ent) * GetAbilityScaling(0.15, 0.30, talentSkillLvl);
    }

    public override void DefaultFighter_HandleEvent(Event ent)
    {
        //if E castet not on self
        if (ent.SourceUnit == Parent && ent.TargetUnit != Parent && ent is ExecuteAbilityFinish af &&
            af.ParentStep.ActorAbility == ability)
        {
            ent.ChildEvents.Add(new AdvanceAV(ent.ParentStep, this, Parent) {TargetUnit = ent.TargetUnit});
        }


        base.DefaultFighter_HandleEvent(ent);
    }


    /*
     * if 2+ turns left we dont need SP.
     * duration 2= turn+ next from bronya turn or double turn between support turn
     */
    public override double WillSpend()
    {
        return WillCastE() ? 1 + GetFriendSpender(UnitRole.MainDPS) : 0;
    }

    //mainDPS have >=50% action value or got CC
    private bool WillCastE()
    {
        Unit mainDPS = GetFriendByRole(UnitRole.MainDPS).Parent;
        if (mainDPS != Parent && (mainDPS.Controlled || mainDPS.Stats.PerformedActionValue < mainDPS.GetActionValue(null) * 0.5))
            return true;
        return false;
    }
    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Harmony;
    public override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Wind;


    //50-110
    private double? CalculateBasicDmg(Event ent)
    {
        return FighterUtils.CalculateDmgByBasicVal(
            Parent.GetAttack(ent) * (0.4 + wbSkillLvl * 0.1),
            ent);
    }

}