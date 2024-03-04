using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using Microsoft.VisualBasic;

namespace HSR_SIM_LIB.Utils;

/// <summary>
/// Formula class.
/// Result will be calculated once
/// part of code got from https://stackoverflow.com/questions/34138314/creating-dynamic-formula
/// </summary>
public class Formula : ICloneable
{
    //event where formula is used
    public Event EventRef { get; set; }


    public enum DynamicTargetEnm
    {
        Attacker,
        Defender
    }

    public Unit Attacker => EventRef.SourceUnit;
    public Unit Defender => EventRef.TargetUnit;

    public static String GenerateReference<T>(Expression<Action<T>> expression)
    {
        var member = expression.Body as MethodCallExpression;

        if (member != null)
            return member.Method.Name;

        throw new ArgumentException("Expression is not a method", "expression");
    }


    private double? result;

    public record VarVal
    {
        public double? Result { get; set; }
        public string ReplaceExpression { get; init; }
        public Formula ResFormula { get; set; }
        public List<Effect> TraceEffects { get; set; } = new List<Effect>();
    }

    /// <summary>
    /// This simply stores a variable name and its value so when this key is found in a expression it gets the value accordingly.
    /// </summary>
    public Dictionary<string, VarVal> Variables { get; set; } = new();

    /// <summary>
    /// return result of Formula
    /// if no result then calculate it
    /// </summary>
    public double Result
    {
        get => result ??= CalculateResult();
        set => result = value;
    }


    /// <summary>
    /// The expression itself, each value and operation must be separated with SPACES. The expression does not support PARENTHESES at this point.
    /// </summary>
    public string Expression { get; init; }

    /// <summary>
    /// Parse  for DynamicTargetEnm and create and calc variable
    /// </summary>
    private void ParseVariables()
    {
        int GetNext(string pstr, int ndx)
        {
            return Expression.IndexOf(pstr.ToString(), ndx, StringComparison.Ordinal);
        }

        string GetNextMethod(string pstr, int ndx, out int nextPos)
        {
            if (ndx == -1)
            {
                nextPos = ndx;
                return string.Empty;
            }

            if (ndx + 1 >= pstr.Length)
            {
                nextPos = -1;
                return String.Empty;
            }

            int methodEndNdx = pstr.IndexOf("#", ndx, StringComparison.Ordinal);
            if (methodEndNdx == -1)
                methodEndNdx = pstr.Length;
            nextPos = methodEndNdx + 1;
            return pstr.Substring(ndx, methodEndNdx - ndx);
        }

        //extract variables
        foreach (DynamicTargetEnm dynVar in (DynamicTargetEnm[])Enum.GetValues(typeof(DynamicTargetEnm)))
        {
            int ndx = GetNext(dynVar.ToString(), 0);

            while (ndx >= 0)
            {
                int endNdx = GetNext(Strings.Space(1), ndx);
                if (endNdx < 0)
                    endNdx = Expression.Length;
                string varExpr = Expression.Substring(ndx, endNdx - ndx);
                Variables.Add(varExpr, new VarVal() { ReplaceExpression = varExpr });
                ndx = GetNext(dynVar.ToString(), ndx + 1);
            }
        }

        //calculate variables
        foreach (var variable in Variables.Where(x => x.Value.Result is null))
        foreach (var dynVar in (DynamicTargetEnm[])Enum.GetValues(typeof(DynamicTargetEnm)))
        {
            var expr = variable.Value.ReplaceExpression;
            var ndx = expr.IndexOf(dynVar.ToString(), StringComparison.Ordinal);
            if (ndx < 0) continue;
            var methodNdx = expr.IndexOf('#') + 1;

            var myFieldInfo = this.GetType().GetProperty(dynVar.ToString());

            var initUnit = (Unit)myFieldInfo!.GetValue(this);
            object finalMeth = initUnit;
            var prevObject = finalMeth;
            var nextMethod = GetNextMethod(expr, methodNdx, out methodNdx);
            //parsing all method and properties line
            while (nextMethod != string.Empty)
            {
                var mInfo = finalMeth.GetType().GetMethod(nextMethod);
                if (mInfo is not null)
                {
                    prevObject = finalMeth;
                    object[] objArr = [];
                    foreach (var prm in mInfo.GetParameters())
                    {
                        Array.Resize(ref objArr, objArr.Length + 1);
                        if (prm.ParameterType == typeof(Event))
                        {
                            objArr[^1] = EventRef;
                        }
                        else if (prm.Name == "outputEffects")
                        {
                            objArr[^1] = variable.Value.TraceEffects;
                        }
                        else
                        {
                            var nextPrm = GetNextMethod(expr, methodNdx, out methodNdx);
                            if (String.IsNullOrEmpty(nextPrm)) continue;
                            //first search Type
                            var searchType = Type.GetType(nextPrm);
                            if (searchType != null)
                            {
                                objArr[^1] = searchType;
                            }
                            else
                            {
                                objArr[^1] = null;
                            }
                        }
                    }

                    finalMeth = mInfo.Invoke(prevObject, objArr);
                }

                PropertyInfo pInfo = finalMeth.GetType().GetProperty(nextMethod);
                if (pInfo is not null)
                {
                    prevObject = finalMeth;
                    finalMeth = pInfo.GetValue(finalMeth);
                }


                nextMethod = GetNextMethod(expr, methodNdx, out methodNdx);
            }


            switch (finalMeth)
            {
                case double ds:
                    variable.Value.Result = ds;
                    break;
                case Formula fs:
                    variable.Value.Result = fs.Result;
                    variable.Value.ResFormula = fs;
                    break;
                case PropertyInfo pf:
                    variable.Value.Result = (double)pf.GetValue(prevObject)!;
                    break;
            }

            break;
        }
    }


    private double CalculateResult()
    {
        ParseVariables();
        if (string.IsNullOrWhiteSpace(this.Expression))
            throw new Exception("An expression must be defined in the Expression property.");

        double? calculationResult = null;
        var operation = string.Empty;

        //This will be necessary for priorities operations such as parentheses, etc... It is not being used at this point.
        List<double> aux = new List<double>();

        foreach (var lexeme in Expression.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))
        {
            //If it is an operator
            if (lexeme == "*" || lexeme == "/" || lexeme == "+" || lexeme == "-")
            {
                operation = lexeme;
            }
            else //It is a number or a variable
            {
                double value = double.MinValue;
                if (Variables.ContainsKey(lexeme)) //If it is a variable, let's get the variable value
                    value = Variables[lexeme].Result ?? 0;
                else //It is just a number, let's just parse
                    value = double.Parse(lexeme);

                if (!calculationResult.HasValue) //No value has been assigned yet
                {
                    calculationResult = value;
                }
                else
                {
                    switch (operation) //Let's check the operation we should perform
                    {
                        case "*":
                            calculationResult = calculationResult.Value * value;
                            break;
                        case "/":
                            calculationResult = calculationResult.Value / value;
                            break;
                        case "+":
                            calculationResult = calculationResult.Value + value;
                            break;
                        case "-":
                            calculationResult = calculationResult.Value - value;
                            break;
                        default:
                            throw new Exception("The expression is not properly formatted.");
                    }
                }
            }
        }

        if (calculationResult.HasValue)
            return calculationResult.Value;
        else
            throw new Exception("The operation could not be completed, a result was not obtained.");
    }


    public IEnumerable<Effect> DescendantsAndSelfEffects()
    {
        foreach (var eff in Variables.Values.SelectMany(x=>x.TraceEffects))
            yield return eff;
        foreach (var formula in Variables.Values.Where(x=>x.ResFormula!=null).Select(x=>x.ResFormula))
        {
            foreach (var eff in formula.DescendantsAndSelfEffects())
                yield return eff;
        }
    }

    /// <summary>
    /// Rec with Replace variables
    /// </summary>
    public record ReplacersRec
    {
        public List<VarVal> ReplacedExpressions { get; set; } = [];
        public List<VarVal> ReplacedFormulas { get; set; } = [];
        public List<VarVal> ReplacedRaw { get; set; } = [];
    }

    /// <summary>
    /// output formula 
    /// </summary>
    /// <param name="shortExplain">is child explanation. will run 1 times</param>
    /// <param name="replacedResults">previously made replacements of variables with the result of calculations</param>
    /// <param name="replacedVariables">previously made replacements of variables with contents (formulas, etc.) </param>
    /// <param name="newlyReplacedVariables">replacements made in child calculations, but not yet added to the main array</param>
    /// <returns></returns>
    public string Explain(
        bool shortExplain = false,
        List<VarVal> replacedResults = null,
        ReplacersRec replacedVariables = null,
        ReplacersRec newlyReplacedVariables = null)

    {
        replacedResults ??= [];
        replacedVariables ??= new ReplacersRec();
        newlyReplacedVariables ??= new ReplacersRec();

        bool nextRunAllowed = true;
        //if short explain then always 1 explain iteration
        bool firstRun = shortExplain;

        bool IncompleteLevel(Dictionary<string, VarVal> sVals)
        {
            return nextRunAllowed && sVals.Any(
                x => (x.Value.ReplaceExpression != null && !replacedVariables.ReplacedExpressions.Contains(x.Value))
                     || (x.Value.ResFormula != null && !replacedVariables.ReplacedFormulas.Contains(x.Value))
                     || !replacedVariables.ReplacedRaw.Contains(x.Value)
                     || (x.Value.Result != null && !replacedResults.Contains(x.Value))
            );
        }

        var finalStr = String.Empty;

        if (!shortExplain)
        {

            var enumerable =  DescendantsAndSelfEffects().ToArray();
            if (enumerable.Any())
                finalStr += "Used effects:"+ Environment.NewLine;
            //todo ORDER BY TYPE, BUFF/DEBUFF, AppliedDebuff or Passive and mark if got by condition
            foreach (var effect in enumerable.Distinct())
            {
                finalStr += $"{effect.GetType().Name} for {effect.Value}"+ Environment.NewLine;
            }


        }

        while (firstRun || IncompleteLevel(Variables))
        {
            firstRun = false;
            foreach (var lexeme in Expression.Split(new string[] { " " },
                         StringSplitOptions.RemoveEmptyEntries))
            {
                if (Variables.TryGetValue(lexeme, out var val))
                {
                    //if expression exists and not handled
                    if (val.ReplaceExpression == lexeme)
                        if (!replacedVariables.ReplacedRaw.Contains(val))
                            replacedVariables.ReplacedRaw.Add(val);
                    if (!replacedVariables.ReplacedRaw.Contains(val))
                    {
                        if (!newlyReplacedVariables.ReplacedRaw.Contains(val))
                            newlyReplacedVariables.ReplacedRaw.Add(val);
                        finalStr += lexeme + Strings.Space(1);
                    }
                    else if (!String.IsNullOrEmpty(val.ReplaceExpression) &&
                             !replacedVariables.ReplacedExpressions.Contains(val))
                    {
                        if (!newlyReplacedVariables.ReplacedExpressions.Contains(val))
                            newlyReplacedVariables.ReplacedExpressions.Add(val);
                        var valStr = val.ReplaceExpression;
                        finalStr += $"{valStr}" + Strings.Space(1);
                    }
                    else if (val.ResFormula != null && !replacedVariables.ReplacedFormulas.Contains(val))
                    {
                        var valStr = "(" + val.ResFormula.Explain(true, replacedResults, replacedVariables,
                                         newlyReplacedVariables) +
                                     ")";
                        if (!IncompleteLevel(val.ResFormula.Variables))
                            if (!newlyReplacedVariables.ReplacedFormulas.Contains(val))
                                newlyReplacedVariables.ReplacedFormulas.Add(val);
                        finalStr += $"{valStr}" + Strings.Space(1);
                    }
                    else if (val.Result != null && !replacedResults.Contains(val))
                    {
                        //add val to primary results array immediately
                        if (!replacedResults.Contains(val))
                            replacedResults.Add(val);
                        //finalStr += val.Result + Strings.Space(1);
                        finalStr += (val.TraceEffects.Any()
                            ? "(" + String.Join("+", val.TraceEffects.Select(x => x.Value)) + ")"
                            : val.Result) + Strings.Space(1);
                    }
                    else
                    {
                        finalStr += (val.TraceEffects.Any()
                            ? "(" + String.Join("+", val.TraceEffects.Select(x => x.Value)) + ")"
                            : val.Result) + Strings.Space(1);
                    }
                }
                else
                {
                    finalStr += lexeme + Strings.Space(1);
                }
            }

            //Every iteration in main cycle add new items to main array
            if (!shortExplain)
            {
                replacedVariables.ReplacedExpressions.AddRange(newlyReplacedVariables.ReplacedExpressions);
                newlyReplacedVariables.ReplacedExpressions.Clear();
                replacedVariables.ReplacedFormulas.AddRange(newlyReplacedVariables.ReplacedFormulas);
                newlyReplacedVariables.ReplacedFormulas.Clear();
                replacedVariables.ReplacedRaw.AddRange(newlyReplacedVariables.ReplacedRaw);
                newlyReplacedVariables.ReplacedRaw.Clear();
            }

            finalStr += (shortExplain ? "" : " = " + Environment.NewLine);
            nextRunAllowed = !shortExplain;
        }


        return finalStr + (shortExplain ? "" : result);
    }


    public object Clone()
    {
        var newClone = (Formula)MemberwiseClone();
        var oldVariables = newClone.Variables;
        newClone.Variables = new Dictionary<string, VarVal>();
        foreach (var oldVar in oldVariables)
        {
            newClone.Variables[oldVar.Key] = oldVar.Value with { TraceEffects = new List<Effect>() };
        }

        return newClone;
    }
}