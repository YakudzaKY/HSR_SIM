using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HSR_SIM_LIB.Fighters;
using static HSR_SIM_LIB.TurnBasedClasses.Step;
using static HSR_SIM_LIB.TurnBasedClasses.Events.Event;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Skills;
using static HSR_SIM_LIB.Skills.Effect;
using System.Runtime.Intrinsics.X86;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Skills.Buff;
using Buff = HSR_SIM_LIB.Skills.Buff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    /// <summary>
    /// Events. Situation changed when event was pro-ceded
    /// </summary>
    public abstract class Event : CloneClass
    {
        public delegate double? CalculateValuePrc(Event ent);
        public delegate IEnumerable<Unit> CalculateTargetPrc();

        public CalculateValuePrc CalculateValue { get; init; }
        public CalculateTargetPrc CalculateTargets { get; init; }
        private double? val;//Theoretical value
        private double? realVal;//Real hit value(cant exceed)

    
   
   
        public ICloneable Source { get; }
        public Ability.TargetTypeEnm? TargetType { get; set; }
        public List<Event> ChildEvents= new List<Event>();

        public Ability.AbilityCurrentTargetEnm? CurentTargetType { get; set; }
        public Unit SourceUnit { get; set; }
        public Unit TargetUnit { get; set; }
        public StepTypeEnm? OnStepType { get; init; }
        public Ability AbilityValue { get; set; }

        public double? Val
        {
            get
            {
                //calc value first
                if (CalculateValue != null && val == null)
                    val = CalculateValue(this);
                return val;
            }

            set => val = value; }
        public double? RealVal { get => realVal; set => realVal = value; }



        public Step ParentStep { get; set; } = null;
        public bool TriggersHandled { get; set; } = false;

        public bool IsDamageEvent => (this is ToughnessBreak or DoTDamage or DirectDamage or ToughnessBreakDoTDamage);


        public Event(Step parent, ICloneable source, Unit sourceUnit)
        {
            ParentStep = parent;
            Source = source;
            SourceUnit = sourceUnit;
        }

        public abstract string GetDescription();


        /// <summary>
        /// Proc one event. Used after child
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="revert"></param>
        public virtual void ProcEvent(bool revert)
        {
            ParentStep.ProceedEvents.Add(this);

            //call handlers
            if (!TriggersHandled)
            {
                TriggersHandled = true;
                ParentStep.Parent.EventHandlerProc?.Invoke(this);

            }
            //Child events
            foreach (Event e in ChildEvents)
            {
                e.ProcEvent(revert);
            }
            ChildEvents.Clear();
        }


  
       

        /// <summary>
        /// Remove modifictaion
        /// </summary>
        /// <param name="mod">Modification</param>
        /// <param name="naturalFinish">If true - MOD exceed by duratiuon. If false - dispeleed by ability</param>
        /// <exception cref="NotImplementedException"></exception>
        public void DispelMod(Buff mod, bool naturalFinish)
        {
            if (naturalFinish)
            {
                mod.ProceedNaturalExpire(this);
            }
            RemoveMod dispell = new RemoveMod(ParentStep, AbilityValue, SourceUnit)
            {  AbilityValue = AbilityValue, Modification = mod, TargetUnit = TargetUnit };
            ChildEvents.Add(dispell);
            mod.ProceedExpired(this);
        }


        /// <summary>
        /// attempt to apply debuff
        /// </summary>
        /// <param name="modType">What we modificate</param>
        /// <param name="effects">effect list </param>
        /// <param name="baseDuration">Duration of the mod</param>
        /// <param name="baseChance">base chance of debuff</param>
        /// <param name="maxStack">max stacks</param>
        /// <param name="uniqueStr">Unique buff per battle</param>
        /// <param name="uniqueUnit">Unique buff per unit</param>
        public void TryDebuff(Buff mod, double baseChance)
        {
            //add Dots and debuffs
            ApplyMod dotEvent = new(ParentStep, Source, SourceUnit)
            {
                AbilityValue = AbilityValue,
                TargetUnit = TargetUnit,
                BaseChance = baseChance,
                Modification = mod
            };


            if (FighterUtils.CalculateDebuffResisted(dotEvent))
            {
                ChildEvents.Add(dotEvent);
                //subscription to events(need calc stacks at attacks)
                dotEvent.Modification.AbilityValue = AbilityValue;
            }
            else
            {
                //debuff apply failed
                DebuffResisted failEvent = new(ParentStep, Source, SourceUnit)
                {
                    AbilityValue = AbilityValue,
                    TargetUnit = TargetUnit,
                    Modification = dotEvent.Modification

                };
                ChildEvents.Add(failEvent);
            }
        }



    }

}
