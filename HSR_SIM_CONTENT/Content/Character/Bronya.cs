using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Content.FighterUtils;

namespace HSR_SIM_CONTENT.Content.Character;

public class Bronya : DefaultFighter
{
    private readonly Ability? ability;
    private readonly int talentSkillLvl;
    private readonly int ultimateSkillLvl;
    private readonly int wbSkillLvl;
    private readonly Ability windriderBullet;

    public Bronya(Unit parent) : base(parent)
    {
        Element = Ability.ElementEnm.Wind;
        Parent.Stats.BaseMaxEnergy = 120;
        wbSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Windrider Bullet")!.Level;
        var abilitySkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Combat Redeployment")!.Level;
        ultimateSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "The Belobog March")!.Level;
        talentSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Leading the Way")!.Level;

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
                AdjacentTargets = Ability.AdjacentTargetsEnm.All,
                TargetType = Ability.TargetTypeEnm.Friend
            };

        //buff apply
        ApplyBuff eventBuff = new(null, this, Parent)
        {
            OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,
            AppliedBuffToApply = new AppliedBuff(Parent, null, this)
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

        windriderBullet = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Basic,
            Name = "Windrider Bullet",
            AdjacentTargets = Ability.AdjacentTargetsEnm.None,
            SpGain = 1
        };
        //dmg events
        windriderBullet.Events.Add(new MechanicValChg(null, this, Parent)
            { AbilityValue = windriderBullet, Value = 1 });
        windriderBullet.Events.Add(new DirectDamage(null, this, Parent)
        {
            CalculateValue = DamageFormula(new Formula()
            {
                Expression =
                    $"{Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas.GetAttack)}  * (0.4 + {wbSkillLvl} * 0.1) "
            })
        });


        windriderBullet.Events.Add(new ToughnessShred(null, this, Parent) { Value = 30 });
        windriderBullet.Events.Add(new EnergyGain(null, this, Parent)
            { Value = 20, TargetUnit = Parent });
        Mechanics.AddVal(windriderBullet);

        Abilities.Add(windriderBullet);


        //Ability
        ability = new Ability(this)
        {
            AbilityType = Ability.AbilityTypeEnm.Ability,
            TargetType = Ability.TargetTypeEnm.Friend,
            Name = "Combat Redeployment",
            Cooldown = 2,
            CostType = Resource.ResourceType.SP,
            Cost = 1,
            IWannaUseIt = WillCastE
        };
        ability.Events.Add(new DispelBad(null, this, Parent));
        //dmg events
        ability.Events.Add(new ApplyBuff(null, this, Parent)
        {
            AppliedBuffToApply = new AppliedBuff(Parent, null, this)
            {
                BaseDuration = 1,
                Effects = [new EffAllDamageBoost { Value = GetAbilityScaling(0.33, 0.66, abilitySkillLvl) }]
            }
        });
        ability.Events.Add(new EnergyGain(null, this, Parent)
            { Value = 30, TargetUnit = Parent });
        Abilities.Add(ability);


        //ultimate
        var ultimate =
            new Ability(this)
            {
                AbilityType = Ability.AbilityTypeEnm.Ultimate,
                Name = "The Belobog March",
                CostType = Resource.ResourceType.Energy,
                Cost = Parent.Stats.BaseMaxEnergy,
                AdjacentTargets = Ability.AdjacentTargetsEnm.All,
                TargetType = Ability.TargetTypeEnm.Friend
            };

        //buff apply
        ApplyBuff ultimateBuff = new(null, this, Parent)
        {
            AppliedBuffToApply = new AppliedBuff(Parent, null, this)
            {
                CustomIconName = "The_Belobog_March",
                Type = Buff.BuffType.Buff,
                Effects = new List<Effect>
                {
                    new EffAtkPrc { Value = GetAbilityScaling(0.33, 0.55, ultimateSkillLvl) },
                    new EffCritDmg { CalculateValue = CalcUltCritDmg }
                },
                BaseDuration = 2
            }
        };
        ultimate.Events.Add(ultimateBuff);
        ultimate.Events.Add(new EnergyGain(null, this, Parent)
            { Value = 5, TargetUnit = Parent });

        Abilities.Add(ultimate);


        //=====================
        //Ascended Traces
        //=====================

        if (ATraces.HasFlag(ATracesEnm.A6))
            Parent.PassiveBuffs.Add(new PassiveBuff(Parent, this)
            {
                Effects = [new EffAllDamageBoost { Value = 0.10 }],
                Target = Parent.ParentTeam
            });
    }

    public override PathType? Path { get; } = PathType.Harmony;


    private Formula CalcUltCritDmg(Event ent)
    {
        return new Formula()
        {
            Expression =
                $" {Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas.GetCritDamage)} * {GetAbilityScaling(0.12, 0.16, ultimateSkillLvl)} " +
                $" +  {GetAbilityScaling(0.12, 0.20, ultimateSkillLvl)}"
        };
    }

    //windrider bullet advance calc
    private Formula CalcTalentAv(Event ent)
    {
        return new Formula()
        {
            Expression =
                $" {Formula.DynamicTargetEnm.Attacker}#{nameof(UnitFormulas.GetActionValue)} * {GetAbilityScaling(0.15, 0.30, talentSkillLvl)}"
        };
    }

    protected override void DefaultFighter_HandleStep(Step step)
    {
        if (((step.StepType is Step.StepTypeEnm.UnitFollowUpAction && step.ActorAbility == ability) ||
             step.StepType is Step.StepTypeEnm.UnitTurnEnded) && step.Actor == Parent &&
            Mechanics.Values[windriderBullet] > 0)
        {
            step.Events.Add(new ModActionValue(step, this, Parent)
                { CalculateValue = CalcTalentAv, TargetUnit = Parent });
            step.Events.Add(new MechanicValChg(step, this, Parent)
                { AbilityValue = windriderBullet, Value = -Mechanics.Values[windriderBullet] });
        }

        base.DefaultFighter_HandleStep(step);
    }

    protected override void DefaultFighter_HandleEvent(Event ent)
    {
        //if E castet not on self
        if (ent.SourceUnit == Parent && ent.TargetUnit != Parent && ent is ExecuteAbilityFinish af &&
            af.ParentStep.ActorAbility == ability)
            ent.ChildEvents.Add(new AdvanceAV(ent.ParentStep, this, Parent) { TargetUnit = ent.TargetUnit });


        base.DefaultFighter_HandleEvent(ent);
    }


    /*
        If wanna cast E then return sp cost+ maindps sp cost
     */
    protected override double WillSpend()
    {
        return WillCastE() ? 1 + GetFriendSpender(UnitRole.MainDps) : 0;
    }

    //mainDPS have >=50% action value or got CC or 5 sp
    private bool WillCastE()
    {
        var mainDps = GetFriendByRole(UnitRole.MainDps).Parent;
        if ((mainDps != Parent &&
             (mainDps.Controlled || mainDps.Stats.PerformedActionValue < mainDps.GetActionValue().Result * 0.5)) ||
            Parent.ParentTeam.GetRes(Resource.ResourceType.SP).ResVal >= Constant.MaxSp - 1)
            return true;
        return false;
    }
}