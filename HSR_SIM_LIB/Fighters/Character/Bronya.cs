using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Fighters.FighterUtils;

namespace HSR_SIM_LIB.Fighters.Character;

public class Bronya : DefaultFighter
{
    private readonly int wbSkillLvl;
    public Bronya(Unit parent) : base(parent)
    {

        Parent.Stats.BaseMaxEnergy = 120;
        wbSkillLvl = Parent.Skills.FirstOrDefault(x => x.Name == "Windrider Bullet").Level;

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
        windriderBullet.Events.Add(new EnergyGain(null, this, Parent)
        { Val = 20, TargetUnit = Parent });


        Abilities.Add(windriderBullet);


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

    /*
     * if 2+ turns left we dont need SP.
     * duration 2= turn+ next from bronya turn or double turn between support turn
     */
    public override double WillSpend()
    {
        return WillCastE() ? 1 + GetFriendSpender(UnitRole.MainDPS) : 0;
    }

    //todo analyse mainDPS action value
    private bool WillCastE()
    {
        return true;
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