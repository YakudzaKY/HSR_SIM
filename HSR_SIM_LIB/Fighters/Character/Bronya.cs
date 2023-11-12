using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class Bronya:DefaultFighter
    {
        public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Harmony;
        public override Unit.ElementEnm Element { get;  } =Unit.ElementEnm.Wind;

        public double? CalculateBasicDmg(Event ent)
        {
            return FighterUtils.CalculateDmgByBasicVal(Parent.GetAttack(null) *(0.4 + (Parent.Skills.FirstOrDefault(x=>x.Name=="Windrider Bullet").Level*0.1)), ent);
        }
        public Bronya(Unit parent) : base(parent)
        {
            Parent.Stats.BaseMaxEnergy = 120;
            var ability =
                //buff tech
                new Ability(this) { AbilityType = Ability.AbilityTypeEnm.Technique, Name = "Banner of Command", Cost = 1, CostType = Resource.ResourceType.TP, Element = Element , AdjacentTargets = Ability.AdjacentTargetsEnm.All,TargetType = Ability.TargetTypeEnm.Friend};

            //buff apply
            ApplyBuff eventBuff = new(null, this,this.Parent)
                { OnStepType = Step.StepTypeEnm.ExecuteAbilityFromQueue,  AbilityValue = ability,
                    BuffToApply = (new Buff(Parent){Type=Buff.ModType.Buff,Effects = new List<Effect>(){new EffAtkPrc() { Value = 0.15}} ,BaseDuration= 2,Dispellable = true})
                };
            ability.Events.Add(eventBuff);

            Abilities.Add(ability);


            Ability SystemWarning;
            //System Warning
            SystemWarning = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Basic
                , Name = "FIX THIS SHIT!!!"
                , Element = Element
                , AdjacentTargets = Ability.AdjacentTargetsEnm.None
                , Attack=true
                , EnergyGain = 20
                , SpGain = 1
            };
            //dmg events
            SystemWarning.Events.Add(new DirectDamage(null, this, this.Parent) { CalculateValue = CalculateBasicDmg,  AbilityValue = SystemWarning });
            SystemWarning.Events.Add(new ToughnessShred(null, this, this.Parent) { Val = 30,  AbilityValue = SystemWarning });
            Abilities.Add(SystemWarning);
            //=====================
            //Ascended Traces
            //=====================

            if (Atraces.HasFlag(ATracesEnm.A6))
            {
                PassiveMods.Add(new PassiveMod(Parent)
                {
                    Mod = new Buff(Parent)
                    { Effects =  new List<Effect>() { new EffAllDamageBoost(){  Value = 0.10 }} },
                    Target = Parent.ParentTeam
                   
                });
            }


        }
    }
}
