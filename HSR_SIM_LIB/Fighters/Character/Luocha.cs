using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class Luocha:DefaultFighter
    {
        public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Abundance;
        public sealed override Unit.ElementEnm Element { get;  } =Unit.ElementEnm.Imaginary;
        private readonly Mod uniqueBuff = null;
        private Ability cycleOfLife;
        private readonly double cycleOfLifeMaxCnt = 2;
        public override string GetSpecialText()
        {
            return $"CoL: {(int)Mechanics.Values[cycleOfLife]:d}\\{(int)cycleOfLifeMaxCnt:d}";
        }

        public bool ColAvailable()
        {
            return (Mechanics.Values[cycleOfLife] == cycleOfLifeMaxCnt);
        }

        public bool ColBuffAvailable()
        {
            return Parent.Mods.Any(x => x.RefMod == uniqueBuff);
        }
        public Luocha(Unit parent) : base(parent)
        {
            Parent.Stats.BaseMaxEnergy = 100;

            uniqueBuff = new Mod(Parent)
            {
                Type = Mod.ModType.Buff, BaseDuration = 2, MaxStack = 1, CustomIconName = "Icon_Abyss_Flower"
            };

            //=====================
            //Abilities
            //=====================
            //Cycle of life
            cycleOfLife = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.FollowUpAction
                , Name = "Cycle of life"
                , Element = Element
                , Available=ColAvailable
                , Priority = Ability.PriorityEnm.Low
                , TargetType = Ability.TargetTypeEnm.Self
            };
            cycleOfLife.Events.Add(new Event(null, this, this.Parent) {Type = Event.EventType.MechanicValChg, TargetUnit = Parent , AbilityValue = cycleOfLife , Val = -cycleOfLifeMaxCnt});
            cycleOfLife.Events.Add(new(null, this, Parent)
            {
                AbilityValue = cycleOfLife,
                Type = Event.EventType.Mod,
                TargetUnit = Parent,
                Modification = uniqueBuff
            });

            Mechanics.AddVal(cycleOfLife);
            Abilities.Add(cycleOfLife);

            //Mercy of a Fool
            var ability = new Ability(this) {   AbilityType = Ability.AbilityTypeEnm.Technique
                , Name = "Mercy of a Fool"
                , Cost = 1
                , CostType = Resource.ResourceType.TP
                , Element = Element
            };
            ability.Events.Add(new Event(null, this,Parent) { OnStepType = Step.StepTypeEnm.ExecuteAbility, TargetUnit = Parent,Type = Event.EventType.MechanicValChg, Val = cycleOfLifeMaxCnt, AbilityValue = cycleOfLife });//AbilityValue for Cycle of life
            Abilities.Add(ability);

            //CoL buffs
            ConditionMods.Add(new ConditionMod(Parent)
            {

                Mod=new Mod(Parent){Effects = new List<Effect>(){ new Effect() {EffType = Effect.EffectType.AtkPrc,Value = 0.08}},CustomIconName = uniqueBuff.CustomIconName}
                , Target=Parent.ParentTeam
                ,Condition= new ConditionMod.ConditionRec(){ConditionAvailable=ColBuffAvailable}
                    
            });

        }
    }
}
