using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Utils.Utils;
using Newtonsoft.Json;

namespace HSR_SIM_LIB.Fighters
{
    /*
     *Dev mode functions 
     */
    public static class DevModeUtils
    {
        //do we deal crit or not ?
        public static bool IsCrit(Event ent)
        {
            Worker wrk = ent.ParentStep.Parent.Parent;
            string[] critStrings = { "CRIT", "not crit" };
            int devLogVal = wrk.DevModeLog.ReadNext(critStrings, $"{ent.ParentStep.GetDescription()} is crit hit on #{ent.TargetUnit.ParentTeam.Units.IndexOf(ent.TargetUnit) + 1} {ent.TargetUnit.Name} ?");
            return devLogVal == 0 ? true : false;


        }

        //do we apply debuff or not ?
        public static bool IsDebuffed(Event ent)
        {
            Worker wrk = ent.ParentStep.Parent.Parent;
            string[] critStrings = { "Debuffed", "resisted" };
            int devLogVal = wrk.DevModeLog.ReadNext(critStrings, $"{ent.ParentStep.GetDescription()} is land debuff on #{ent.TargetUnit.ParentTeam.Units.IndexOf(ent.TargetUnit) + 1} {ent.TargetUnit.Name} ?");
            return devLogVal == 0 ? true : false;


        }

        /// <summary>
        /// Pick ability to cast
        /// </summary>
        /// <param name="fighter">Reference to fighter</param>
        /// <param name="abilities"> List of abilities</param>
        /// <param name="canSkip"> Can we choose skip this step or not(for example we can skip ability in technique queue)</param>
        /// <returns></returns>
        public static Ability ChooseAbilityToCast(IFighter fighter, IEnumerable<Ability> abilities, bool canSkip = false)
        {
            Worker wrk = fighter.Parent.ParentTeam.ParentSim.Parent;
            var enmAbilities = abilities as Ability[] ?? abilities.ToArray();
            string[] abilityStrings = enmAbilities.Select(a => a.Name).ToArray();
            if (canSkip)
            {
                abilityStrings = abilityStrings.Concat(new[] { "==SKIP==" }).ToArray();
            }
            int devLogVal = wrk.DevModeLog.ReadNext(abilityStrings, $"{fighter.Parent.Name} chose ability to cast:");
            return (devLogVal >= enmAbilities.Length) ? null : enmAbilities.ElementAt(devLogVal);
        }

        /// <summary>
        /// Pick target from list
        /// </summary>
        /// <param name="fighter">Reference to fighter</param>
        /// <param name="units">Unit list to select</param>
        /// <returns></returns>
        public static Unit GetTarget(IFighter fighter, IEnumerable<Unit> units, Ability abilityToCast)
        {
            Worker wrk = fighter.Parent.ParentTeam.ParentSim.Parent;
            string[] unitStrings = units.Select(a => a.Name).ToArray();
            int devLogVal = wrk.DevModeLog.ReadNext(unitStrings, $"{fighter.Parent.Name} cast {abilityToCast.Name} on:");
            return units.ElementAt(devLogVal);
        }

        public static object GetFixedObject(object items,Worker wrk)
        {
            if (items is IEnumerable ie)
            {
             
                object[] arr = new object[] { };
                int i = 0;
                foreach (object element in ie)
                {
                    Array.Resize<object>(ref arr, i + 1);
                    arr[i] = element;
                    i = i + 1;
                }
                string[] unitStrings = arr.Select(a => a is Unit nt? nt.Name: a is Buff ba?ba.Effects.Select(x=> x.GetType().Name).First(): a is Unit.ElementEnm em?em.ToString(): a.GetType().Name).ToArray();
                int devLogVal = wrk.DevModeLog.ReadNext(unitStrings, $"Need pick some of objects to execute");
                return arr.ElementAt(devLogVal);

            }

            throw new NotImplementedException();


        }

    }
}
