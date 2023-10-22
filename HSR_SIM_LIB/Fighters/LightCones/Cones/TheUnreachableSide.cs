using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.LightCones.Cones
{
    internal class TheUnreachableSide : DefaultLightCone
    {

        private readonly Dictionary<int, double> modifiers = new () { { 1, 0.24 }, { 2, 0.28 }, { 3, 0.32 }, { 4, 0.36 }, { 5, 0.40 } };
        private readonly Mod uniqueBuff = null;

        //add buff when attacked or loose hp
        public override void DefaultLightCone_HandleEvent(Event ent)
        {
            //if unit consume hp or got attack then apply buff
            if ((ent.AbilityValue?.Parent==Parent.Parent&&ent.TargetUnit == Parent.Parent && ent.Type == Event.EventType.ResourceDrain &&
                ent.ResType == Resource.ResourceType.HP && ent.RealVal != 0)
                || (ent.TargetUnit == Parent.Parent && ent.Type == Event.EventType.DirectDamage))
            {
                Event newEvent = new (ent.ParentStep,this)
                {
                    Type = Event.EventType.Mod
                    ,TargetUnit =  Parent.Parent,
                    Modification = uniqueBuff
                };
                newEvent.ProcEvent(false);
                ent.ParentStep.Events.Add(newEvent);
            }
            base.DefaultLightCone_HandleEvent(ent);
        }

        //remove buff when attack completed
        public override void DefaultLightCone_HandleStep(Step step)
        {
            if (step.StepType == Step.StepTypeEnm.ExecuteAbility&&step.Actor == Parent.Parent && step.ActorAbility.Attack)
            {
                Event newEvent = new (step, this)
                {
                    Type = Event.EventType.RemoveMod
                    ,TargetUnit =  Parent.Parent,
                    Modification = uniqueBuff
                };
                newEvent.ProcEvent(false);
                step.Events.Add(newEvent);
            }
            base.DefaultLightCone_HandleStep(step);
        }

        public TheUnreachableSide(IFighter parent, int rank) : base(parent, rank)
        {
            uniqueBuff = new Mod()
            {
                Type = Mod.ModType.Buff, BaseDuration = null, MaxStack = 1, 
                Effects = new List<Effect>(){ new Effect() {EffType = Effect.EffectType.AllDamageBoost, Value = modifiers[rank]}}  
            };
        }
    }
}
