using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters;

/*
 *Dev mode functions
 */
public static class DevModeUtils
{
    //do we deal crit or not ?
    public static bool IsCrit(Event ent)
    {
        var wrk = ent.ParentStep.Parent.Parent;
        string[] critStrings = { "CRIT", "not crit" };
        var devLogVal = wrk.DevModeLog.ReadNext(critStrings,
            $"{ent.ParentStep.GetDescription()} is crit hit on #{ent.TargetUnit.ParentTeam.Units.IndexOf(ent.TargetUnit) + 1} {ent.TargetUnit.Name} ?");
        return devLogVal == 0 ? true : false;
    }

    //do we apply debuff or not ?
    public static bool IsDebuffed(Event ent)
    {
        var wrk = ent.ParentStep.Parent.Parent;
        string[] critStrings = { "Debuffed", "resisted" };
        var devLogVal = wrk.DevModeLog.ReadNext(critStrings,
            $"{ent.ParentStep.GetDescription()} is land debuff on #{ent.TargetUnit.ParentTeam.Units.IndexOf(ent.TargetUnit) + 1} {ent.TargetUnit.Name} ?");
        return devLogVal == 0 ? true : false;
    }

    public static string GetDevLogPath(string scenarioPath, string profilePath)
    {
        return
            $"{Path.GetDirectoryName(scenarioPath)}\\{Path.GetFileName(scenarioPath)}{Path.GetFileName(profilePath)}.dev_log";
    }

    /// <summary>
    ///     Pick ability to cast
    /// </summary>
    /// <param name="fighter">Reference to fighter</param>
    /// <param name="abilities"> List of abilities</param>
    /// <param name="canSkip"> Can we choose skip this step or not(for example we can skip ability in technique queue)</param>
    /// <returns></returns>
    public static Ability ChooseAbilityToCast(IFighter fighter, IEnumerable<Ability> abilities, bool canSkip = false)
    {
        var wrk = fighter.Parent.ParentTeam.ParentSim.Parent;
        var enmAbilities = abilities as Ability[] ?? abilities.ToArray();
        var abilityStrings = enmAbilities.Select(a => a.Name).ToArray();
        if (canSkip) abilityStrings = abilityStrings.Concat(new[] { "==SKIP==" }).ToArray();
        var devLogVal = wrk.DevModeLog.ReadNext(abilityStrings, $"{fighter.Parent.Name} chose ability to cast:");
        return devLogVal >= enmAbilities.Length ? null : enmAbilities.ElementAt(devLogVal);
    }

    /// <summary>
    ///     Pick target from list
    /// </summary>
    /// <param name="fighter">Reference to fighter</param>
    /// <param name="units">Unit list to select</param>
    /// <returns></returns>
    public static Unit GetTarget(IFighter fighter, IEnumerable<Unit> units, Ability abilityToCast)
    {
        var wrk = fighter.Parent.ParentTeam.ParentSim.Parent;
        var unitStrings = units.Select(a => a.Name).ToArray();
        var devLogVal = wrk.DevModeLog.ReadNext(unitStrings, $"{fighter.Parent.Name} cast {abilityToCast.Name} on:");
        return units.ElementAt(devLogVal);
    }

    public static object GetFixedObject(object items, Worker wrk)
    {
        if (items is IEnumerable ie)
        {
            object[] arr = { };
            var i = 0;
            foreach (var element in ie)
            {
                Array.Resize(ref arr, i + 1);
                arr[i] = element;
                i = i + 1;
            }

            var unitStrings = arr.Select(a =>
                a is Unit nt ? nt.Name :
                a is AppliedBuff ba ? ba.Effects.Select(x => x.GetType().Name).First() :
                a is Unit.ElementEnm em ? em.ToString() : a.GetType().Name).ToArray();
            var devLogVal = wrk.DevModeLog.ReadNext(unitStrings, "Need pick some of objects to execute");
            return arr.ElementAt(devLogVal);
        }

        throw new NotImplementedException();
    }
}