using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using HSR_SIM_LIB.UnitStuff;


namespace HSR_SIM_LIB.Utils;

/// <summary>
///     Buffer for formula. Save calculated values and search in next calculations
///     Some formulas got reset(delete) when dependency stat changed
/// </summary>
public class FormulaBuffer
{
    //index by hash-attacker-defender
    private readonly Dictionary<Tuple<string, Unit, Unit>, BufferRec> _bufferRecs= new();

    //link to buffer by reset keys
    private readonly Dictionary<Tuple<object, Unit>, Tuple<string, Unit, Unit>> _resetSubscriptions  = new();

    //UnitRefToBuff
    private readonly Dictionary<Unit, List<Tuple<string, Unit, Unit>>> _unitToBuff = new();

    private readonly Unit _defaultUnit = new();

    //merge second into first
    public static void MergeDependencies(List<DependencyRec> parentDependencies, List<DependencyRec> childDependencies)
    {
        foreach (var dep in childDependencies)
            MergeDependency(parentDependencies, dep);
    }

    public static void MergeDependency(List<DependencyRec> parentDependencies, DependencyRec childDependency)
    {
        if (!parentDependencies.Any(x =>
                Equals(x.Stat, childDependency.Stat) && x.Relation == childDependency.Relation))
            parentDependencies.Add(childDependency);
    }

    /// <summary>
    ///     Full reset, or reset by Unit
    /// </summary>
    /// <param name="unit">clear cache for this unit</param>
    /// <param name="prm">Parameter to find. If null then remove all unit entry</param>
    public void Reset(Unit unit = null, object prm = null)
    {
        if (unit == null)
            _bufferRecs.Clear();
        
        if (prm != null)
        {
            var key = Tuple.Create(prm, unit);
            if (_resetSubscriptions.Remove(key, out var bRec))
            {
                _bufferRecs.Remove(bRec);
            }
        }
        //delete all buff recs by unit
        else if (_unitToBuff.Remove(unit, out var bRecList))
        {
            foreach (var key in bRecList.Where(x=>_bufferRecs.ContainsKey(x)))
            {
                _bufferRecs.Remove(key); 
            }
                
        }
    }


    private static string ToHex(ref byte[] bytes)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);

        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString("x2"));
        return result.ToString();
    }

    /// <summary>
    /// gen MD5 Hash for formula expression
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenerateHash(string input)
    {
        using var md5Hasher = MD5.Create();
        var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        return ToHex(ref data);
    }

    /// <summary>
    ///     search existing resolved formula
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="attacker"></param>
    /// <param name="defender"></param>
    /// <returns></returns>
    public Formula SearchBuff(string hash, Unit attacker, Unit defender)
    {
        if (!_bufferRecs.TryGetValue(Tuple.Create(hash, attacker, _defaultUnit), out var bRec))
        {
            _bufferRecs.TryGetValue(Tuple.Create(hash, attacker, defender), out bRec);
        }

        return bRec?.BuffFormula;
    }

    public void AddToBuff(Formula formula, Unit attacker, Unit defender, List<DependencyRec> dependencyRecs)
    {
        //save buff only for registered units
        if (attacker is { ParentTeam: null } || defender is { ParentTeam: null })
            return;
        bool defenderNotImportant = dependencyRecs.All(z => z.Relation != Formula.DynamicTargetEnm.Defender);
        var key = Tuple.Create(formula.Hash, attacker, defenderNotImportant ? _defaultUnit : defender);
        _bufferRecs[key] =
            new BufferRec
            {
                Hash = formula.Hash, BuffFormula = formula, TargetUnit = defender,
                SourceUnit = attacker, DependencyRecs = dependencyRecs
            };
        //save links to buffer by dependency
        foreach (var item in dependencyRecs)
        {
            _resetSubscriptions[
                    Tuple.Create(item.Stat,
                        (item.Relation == Formula.DynamicTargetEnm.Attacker) ? attacker : defender)] =
                key;
        }

        //save link from unit to all buffers
        if (!_unitToBuff.ContainsKey(attacker))
            _unitToBuff[attacker] = new List<Tuple<string, Unit, Unit>>();
        _unitToBuff[attacker].Add(key);
        //save link to defender if needed
        if (!defenderNotImportant)
        {
            if (!_unitToBuff.ContainsKey(defender))
                _unitToBuff[defender] = new List<Tuple<string, Unit, Unit>>();
            _unitToBuff[defender].Add(key);
        }
    }

    /// <summary>
    ///     dependency array. Who? Attacker or Defender  and what the stat we depend on
    /// </summary>
    public record DependencyRec
    {
        public required Formula.DynamicTargetEnm Relation { get; init; }
        public required object Stat { get; init; }
    }

    /// <summary>
    ///     Buffer struct
    /// </summary>
    public record BufferRec
    {
        public required Formula BuffFormula { get; init; }
        public required string Hash { get; init; }
        public required Unit SourceUnit { get; init; }
        public Unit TargetUnit { get; init; }
        public required List<DependencyRec> DependencyRecs { get; init; }
    }
}