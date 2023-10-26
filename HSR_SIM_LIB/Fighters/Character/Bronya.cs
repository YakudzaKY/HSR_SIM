using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class Bronya:DefaultFighter
    {
        public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Harmony;
        public override Unit.ElementEnm Element { get;  } =Unit.ElementEnm.Wind;
        public Bronya(Unit parent) : base(parent)
        {
            Parent.Stats.BaseMaxEnergy = 120;
            var ability =
                //buff tech
                new Ability(this) { AbilityType = Ability.AbilityTypeEnm.Technique, Name = "Banner of Command", Cost = 1, CostType = Resource.ResourceType.TP, Element = Element , AdjacentTargets = Ability.AdjacentTargetsEnm.All,TargetType = Ability.TargetTypeEnm.Friend};

            //buff apply
            Event eventBuff = new(null, this,this.Parent)
                { OnStepType = Step.StepTypeEnm.ExecuteAbility, Type = Event.EventType.Mod, AbilityValue = ability,
                    Modification = (new Mod(Parent){Type=Mod.ModType.Buff,Effects = new List<Effect>(){new Effect() { EffType=Effect.EffectType.AtkPrc,Value = 0.15}} ,BaseDuration= 2,Dispellable = true})
                };
            ability.Events.Add(eventBuff);

            Abilities.Add(ability);

            //=====================
            //Ascended Traces
            //=====================

            if (Atraces.HasFlag(ATracesEnm.A6))
            {
                PassiveMods.Add(new PassiveMod(Parent)
                {
                    Mod = new Mod(Parent)
                    { Effects =  new List<Effect>() { new Effect(){ EffType = Effect.EffectType.AllDamageBoost, Value = 0.10 }} },
                    Target = Parent.ParentTeam
                   
                });
            }


        }
    }
}
