using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Effect;

namespace HSR_SIM_LIB.Skills
{
    public class Mod : CloneClass
    {

        public ModType Type { get; init; }

        public string CustomIconName { get; set; }
        public List<Effect> Effects { get; init; } = new List<Effect>();

        //dot will be auto on start
        public static List<EffectType> EarlyProcMods = new List<EffectType>() { EffectType.Entanglement };
        public Ability AbilityValue { get; set; }

        //do buff/debuff work on turn start?(DoT always at start)
        public bool IsEarlyProc()
        {

            foreach (Effect effect in Effects)
            {
                if (EarlyProcMods.Contains(effect.EffType))
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
                    x.EffType is EffectType.Entanglement
                    or EffectType.Freeze
                    or EffectType.Imprisonment
                    or EffectType.Dominated
                    or EffectType.Outrage);
            }
        }

        //proceed the mod proc(dot tick etc)
        public void Proceed(Step step)
        {
            //do some shit
            if (Type == ModType.Dot)
                foreach (var dotProcEvent in Effects.Select(effect => new DoTDamage(step, this.Caster, Caster)
                {
                    CalculateValue = effect.CalculateValue,
                    TargetUnit = step.Actor,
                    Modification = this,
                    AbilityValue = AbilityValue


                }))
                {
                    step.Events.Add(dotProcEvent);
                }

            //minus duration
            Event reduceModDuration = new ReduceDuration(step, this.Caster, Caster)
            {
               
                Modification = this,
                TargetUnit = step.Actor,

            };
            step.Events.Add(reduceModDuration);

        }
        public delegate void EventHandler(Event ent);
        public delegate void StepHandler(Step step);

        public void EntanglementEventHandler(Event ent)
        {
            if (ent is DirectDamage && ent.TargetUnit == this.Owner)
                Stack = Math.Min(Stack + 1, MaxStack);

        }

        public Unit Owner { get; set; }

        public IFighter.EventHandler EventHandlerProc { get; set; }
        public IFighter.StepHandler StepHandlerProc { get; set; }


        /// <summary>
        /// handle the Buff 
        /// </summary>
        /// <param name="ent"></param>
        public void ProceedExpire(Event ent)
        {
            //delayed damage
            foreach (var dotProcEvent in Effects.Where(x => x.EffType is EffectType.Freeze or EffectType.Entanglement).Select(effect => new DoTDamage(ent.ParentStep, this.Caster, Caster)
            {
                CalculateValue = effect.CalculateValue,
                TargetUnit = ent.TargetUnit,
                Modification = this,
                AbilityValue = AbilityValue


            }))
            {
                dotProcEvent.ProcEvent(false);
                ent.ParentStep.Events.Add(dotProcEvent);
            }



        }
        public int Stack { get; set; } = 1;

        public int MaxStack { get; set; } = 1;
        public int? BaseDuration { get; set; }
        public int? DurationLeft { get; set; }
        public Unit Caster { get; set; }

        public string UniqueStr { get; set; }

        public Mod RefMod { get; set; }

        public bool Dispellable { get; init; }
        public Unit UniqueUnit { get; set; }
        public bool DoNotClone { get; set; } = false;


        public enum ModType
        {
            Buff,
            Debuff,
            Dot
        }




        public Mod(Unit caster, Mod reference = null)
        {
            RefMod = reference;
            Caster = caster;

        }

        public string GetDescription()
        {

            string modsStr = "";
            foreach (var eff in Effects)
            {
                modsStr += $"{eff.EffType.ToString():s} val= {eff.Value:f} ; ";
            }

            return
                $">> {Type.ToString():s} for {modsStr:s} duration={BaseDuration.ToString():D} dispellable={Dispellable.ToString():s}";
        }


    }
}
