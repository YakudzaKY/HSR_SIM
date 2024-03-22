using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public required Formula.DynamicTargetEnm Relation { get; set; }
        public required Condition.ConditionCheckParam Stat { get; set; }
    }

    //merge second into first
    public static void MergeDependencies(List<DependencyRec> parentDependencies, List<DependencyRec> childDependencies)
    {
        foreach (var dep in childDependencies)
            MergeDependency(parentDependencies, dep);
           
    }

    public static void MergeDependency(List<DependencyRec> parentDependencies, DependencyRec childDependency)
    {
        if (!parentDependencies.Any(x => x.Stat == childDependency.Stat && x.Relation == childDependency.Relation))
        {
            parentDependencies.Add(childDependency);
        }
    }

    /// <summary>
    /// Buffer struct
    /// </summary>
    public record BufferRec
    {
        public required Formula BuffFormula { get; set; }
        public string Hash { get; set; }
        public required Unit SourceUnit { get; set; }
        public required Unit TargetUnit { get; set; }
        public required List<DependencyRec> DependencyRecs { get; set; }
    }

    public List<BufferRec> BufferRecs { get; set; } = [];

    /// <summary>
    /// Full reset, or reset by Unit
    /// </summary>
    /// <param name="unit">clear cache for this unit</param>
    /// <param name="prm">Parameter to find. If null then remove all unit entry</param>
    public void Reset(Unit unit = null, Condition.ConditionCheckParam? prm = null)
    {
        if (unit == null)
            BufferRecs.Clear();
        else
            foreach (var buff in BufferRecs.Where(x => (x.TargetUnit == unit
                                                        && (prm == null || x.DependencyRecs.Any(z =>
                                                            z.Relation == Formula.DynamicTargetEnm.Defender &&
                                                            z.Stat == prm))
                         ) || (x.SourceUnit == unit && (prm == null || x.DependencyRecs.Any(z =>
                             z.Relation == Formula.DynamicTargetEnm.Attacker && z.Stat == prm)))).ToList())
                BufferRecs.Remove(buff);
                
            
    }
    static string GenerateHash(string input)
    {
        MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        return BitConverter.ToString(data);
    }
    /// <summary>
    /// search existing resolved formula
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    /// <returns></returns>
    public Formula SearchBuff(string expression, Unit attacker, Unit defender)
    {
        return BufferRecs.FirstOrDefault(x =>
                x.SourceUnit == attacker && x.TargetUnit == defender && x.Hash == GenerateHash(expression))
            ?.BuffFormula;
    }

    public void AddToBuff(Formula formula, Unit attacker, Unit defender, List<DependencyRec> dependencyRecs)
    {
        BufferRecs.Add(new BufferRec()
            {Hash =GenerateHash(formula.Expression), BuffFormula = formula, TargetUnit = defender, SourceUnit = attacker, DependencyRecs = dependencyRecs });
    }
}