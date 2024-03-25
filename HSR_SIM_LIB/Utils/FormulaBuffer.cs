using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Utils;

/// <summary>
/// Buffer for formula. Save calculated values and search in next calculations
/// Some formulas got reset(delete) when dependency stat changed
/// </summary>
public class FormulaBuffer
{
    /// <summary>
    /// dependency array. Who? Attacker or Defender  and what the stat we depend on
    /// </summary>
    public record DependencyRec
    {
        public required Formula.DynamicTargetEnm Relation { get; init; }
        public required Object Stat { get; init; }
    }

    //merge second into first
    public static void MergeDependencies(List<DependencyRec> parentDependencies, List<DependencyRec> childDependencies)
    {
        foreach (var dep in childDependencies)
            MergeDependency(parentDependencies, dep);
    }

    public static void MergeDependency(List<DependencyRec> parentDependencies, DependencyRec childDependency)
    {
        if (!parentDependencies.Any(x =>  Equals(x.Stat , childDependency.Stat)   && x.Relation == childDependency.Relation))
        {
            parentDependencies.Add(childDependency);
        }
    }

    /// <summary>
    /// Buffer struct
    /// </summary>
    public record BufferRec
    {
        public required Formula BuffFormula { get; init; }
        public string Hash { get; init; }
        public required Unit SourceUnit { get; init; }
        public required Unit TargetUnit { get; init; }
        public required List<DependencyRec> DependencyRecs { get; init; }
    }

    private List<BufferRec> BufferRecs { get; } = [];

    /// <summary>
    /// Full reset, or reset by Unit
    /// </summary>
    /// <param name="unit">clear cache for this unit</param>
    /// <param name="prm">Parameter to find. If null then remove all unit entry</param>
    public void Reset(Unit unit = null, object prm = null)
    {
        if (unit == null)
            BufferRecs.Clear();
        else
            foreach (var buff in BufferRecs.Where(x =>
                         ((x.DependencyRecs.Any(z =>
                              z.Relation == Formula.DynamicTargetEnm.Attacker && Equals(z.Stat , prm)) || prm == null) &&
                          x.SourceUnit == unit)
                         //check defender relations. have no attacker relations or SourceUnit=attacker
                         || (x.DependencyRecs.Any(
                                  z => z.Relation == Formula.DynamicTargetEnm.Defender && ( Equals(z.Stat , prm)||
                                       prm == null))  &&
                             x.TargetUnit == unit)
                     ).ToList())
                BufferRecs.Remove(buff);
    }

    public static string GenerateHash(string input)
    {
        using MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        return BitConverter.ToString(data);
    }

    /// <summary>
    /// search existing resolved formula
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    /// <returns></returns>
    public Formula SearchBuff(string hash, Unit attacker, Unit defender)
    {
        return BufferRecs.FirstOrDefault(x => x.Hash == hash
                                              && (x.SourceUnit == attacker )
                                              //check defender relations. or have no defender relations or TargetUnit=defender
                                              && (x.TargetUnit == defender || x.DependencyRecs.All(z => z.Relation != Formula.DynamicTargetEnm.Defender))
            )
            ?.BuffFormula;
    }

    public void AddToBuff(Formula formula, Unit attacker, Unit defender, List<DependencyRec> dependencyRecs)
    {
        //save buff only for registered units
        if ( attacker is { ParentTeam: null } || defender is { ParentTeam: null })
            return;
        BufferRecs.Add(new BufferRec()
        {
            Hash = formula.Hash, BuffFormula = formula, TargetUnit = defender,
            SourceUnit = attacker, DependencyRecs = dependencyRecs
        });
    }
}