using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones
{
    internal class TheUnreachableSide : DefaultLightCone
    {

        public sealed override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Destruction;

        private readonly Dictionary<int, double> modifiers = new() { { 1, 0.24 }, { 2, 0.28 }, { 3, 0.32 }, { 4, 0.36 }, { 5, 0.40 } };
        private readonly Mod uniqueBuff = null;

        //add buff when attacked or loose hp
        public override void DefaultLightCone_HandleEvent(Event ent)
        {
            //if unit consume hp or got attack then apply buff
            if (((ent.AbilityValue?.Parent == Parent && ent.TargetUnit == Parent.Parent && ent  is ResourceDrain &&
                ((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal != 0)
                || (ent.TargetUnit == Parent.Parent && ent is DirectDamage)) && uniqueBuff != null)
            {
                ApplyMod newEvent = new(ent.ParentStep, this, Parent.Parent)
                {
                    
                    TargetUnit = Parent.Parent,
                    Modification = uniqueBuff
                };
                ent.ChildEvents.Add(newEvent);
            }
            base.DefaultLightCone_HandleEvent(ent);
        }

        //remove buff when attack completed
        public override void DefaultLightCone_HandleStep(Step step)
        {
            if (step.StepType == Step.StepTypeEnm.ExecuteAbility && step.Actor == Parent.Parent && step.ActorAbility.Attack && uniqueBuff != null)
            {
                RemoveMod newEvent = new(step, this, Parent.Parent)
                {
                    TargetUnit = Parent.Parent,
                    Modification = uniqueBuff
                };
          
                step.Events.Add(newEvent);
            }
            base.DefaultLightCone_HandleStep(step);
        }

        public TheUnreachableSide(IFighter parent, int rank) : base(parent, rank)
        {
            if (Path == Parent.Path)
                uniqueBuff = new Mod(Parent.Parent)
                {
                    Type = Mod.ModType.Buff,
                    BaseDuration = null,
                    MaxStack = 1,
                    Effects = new List<Effect>() { new Effect() { EffType = Effect.EffectType.AllDamageBoost, Value = modifiers[rank] } }
                };
        }
    }
}
