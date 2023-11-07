﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Effect;

namespace HSR_SIM_LIB.Skills
{
    public class Buff : CloneClass
    {

        public ModType Type { get; init; }

        public string CustomIconName { get; set; }
        public List<Effect> Effects { get; init; } = new List<Effect>();

        //dot will be auto on start
        public static List<Type> EarlyProcMods = new List<Type>() { typeof(EffEntanglement) };
        public Ability AbilityValue { get; set; }
        public bool IsOld { get; set; } = false;

        //do buff/debuff work on turn start?(DoT always at start)
        public bool IsEarlyProc()
        {

            foreach (Effect effect in Effects)
            {
                if (EarlyProcMods.Contains(effect.GetType()))
                    return true;

            }

            return false;
        }
        //debuf is CC? 
        public bool CrowdControl
        {
            get
            {
                return Effects.Any(x =>
                    x is EffEntanglement
                    or EffFreeze
                    or EffImprisonment
                    or EffDominated
                    or EffOutrage);
            }
        }

        //proceed the mod proc(dot tick etc)
        public void Proceed(Step step)
        {
            //do some shit
            if (Type == ModType.Dot)
                foreach (var effect in Effects)
                {
                   
                    if (Reference == Caster.Fighter.ShieldBreakMod)
                    {
                         var dotProcEvent = new ToughnessBreakDoTDamage(step, this.Caster, Caster) { CalculateValue = effect.CalculateValue, TargetUnit = step.Actor, Modification = this, AbilityValue = AbilityValue };
                         step.Events.Add(dotProcEvent);
                    }

                    else
                    {
                        var dotProcEvent = new DoTDamage(step, this.Caster, Caster) { CalculateValue = effect.CalculateValue, TargetUnit = step.Actor, Modification = this, AbilityValue = AbilityValue };
                        step.Events.Add(dotProcEvent); 
                    }
                    
                   
                }

            //only Buffs aplied before turn started
            if (IsOld)
            {
                //minus duration
                Event reduceModDuration = new ReduceDuration(step, this.Caster, Caster)
                {

                    BuffToApply = this,
                    TargetUnit = step.Actor,

                };
                step.Events.Add(reduceModDuration);
            }

        }
        public delegate void EventHandler(Event ent);
        public delegate void StepHandler(Step step);



        public Unit Owner { get; set; }

        
        public IFighter.EventHandler EventHandlerProc { get; set; }
        public IFighter.StepHandler StepHandlerProc { get; set; }

        /// <summary>
        /// handle the Buff 
        /// </summary>
        /// <param name="ent"></param>
        public void ProceedNaturalExpire(Event ent)
        {
            //delayed damage
            foreach (var x in Effects)
            {
                x.OnNaturalExpire(ent,this);
            }
        }
        public int Stack { get; set; } = 1;

        public int MaxStack { get; set; } = 1;
        public int? BaseDuration { get; set; }
        public int? DurationLeft { get; set; }
        public Unit Caster { get; set; }
        public object SourceObject { get; set; }

        public string UniqueStr { get; set; }

        public Buff Reference { get; set; }

        public bool Dispellable { get; init; } = true;
        public Unit UniqueUnit { get; set; }
        public bool DoNotClone { get; set; } = false;


        public enum ModType
        {
            Buff,
            Debuff,
            Dot
        }




        public Buff(Unit caster, Buff reference = null)
        {
            Reference = reference;
            Caster = caster;

        }

        public string GetDescription()
        {

            string modsStr = "";
            foreach (var eff in Effects)
            {
                modsStr += $"{eff.GetType().Name:s} val= {eff.Value:f} ; ";
            }

            return
                $">> {Type.ToString():s} for {modsStr:s} duration={BaseDuration.ToString():D} dispellable={Dispellable.ToString():s}";
        }

    }
}
