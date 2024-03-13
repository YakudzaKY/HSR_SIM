﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using Microsoft.VisualBasic;

namespace HSR_SIM_LIB.Utils;

/// <summary>
///     Formula class.
///     Result will be calculated once
///     part of code got from https://stackoverflow.com/questions/34138314/creating-dynamic-formula
/// </summary>
public class Formula : ICloneable
{
    public enum DynamicTargetEnm
    {
        Attacker,
        Defender
    }
    
    
    private string expression;


    private double? result;

    //event where formula is used
    public Event EventRef { get; set; }

    public Unit Attacker => EventRef.SourceUnit;
    public Unit Defender => EventRef.TargetUnit;

    /// <summary>
    ///     This simply stores a variable name and its value so when this key is found in a expression it gets the value
    ///     accordingly.
    /// </summary>
    public Dictionary<string, VarVal> Variables { get; set; } = new();

    public IEnumerable<Formula> ChildFormulas
    {
        get
        {
            return Variables.Select(x => x.Value.ResFormula).Where(z => z != null);
        }
    }

    /// <summary>
    ///     return result of Formula
    ///     if no result then calculate it
    /// </summary>
    public double Result
    {
        get => result ??= CalculateResult();
        set => result = value;
    }


    /// <summary>
    ///     The expression itself, each value and operation must be separated with SPACES. The expression does not support
    ///     PARENTHESES at this point.
    /// </summary>
    public string Expression
    {
        get => expression;
        init => expression = value;
    }


    public object Clone()
    {
        var newClone = (Formula)MemberwiseClone();
        var oldVariables = newClone.Variables;
        newClone.Variables = new Dictionary<string, VarVal>();
        foreach (var oldVar in oldVariables)
            newClone.Variables[oldVar.Key] = oldVar.Value with { TraceEffects = new List<EffectTraceRec>() };

        return newClone;
    }


    /// <summary>
    ///     Parse  for DynamicTargetEnm and create and calc variable
    /// </summary>
    private void ParseVariables()
    {
        int GetNext(string pstr, int ndx)
        {
            if (Expression.Length < ndx) return -1;
            return Expression.IndexOf(pstr, ndx, StringComparison.Ordinal);
        }

        int CountCharsUsingFor(string source, char toFind, int startNdx, int endNdx)
        {
            var count = 0;
            for (var n = startNdx; n < endNdx; n++)
                if (source[n] == toFind)
                    count++;

            return count;
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
                return string.Empty;
            }

            var methodEndNdx = pstr.IndexOf("#", ndx, StringComparison.Ordinal);
            if (methodEndNdx == -1)
                methodEndNdx = pstr.Length;
            nextPos = methodEndNdx + 1;
            return pstr.Substring(ndx, methodEndNdx - ndx);
        }

        //do replace on ( ) expressions
        var expNdx = GetNext("(", 0);
        while (expNdx >= 0)
        {
            int endNdx;
            var level = -1;
            endNdx = expNdx;
            while (level != 0)
            {
                endNdx = GetNext(")", endNdx + 1);
                level = CountCharsUsingFor(Expression, '(', expNdx + 1, endNdx) -
                        CountCharsUsingFor(Expression, ')', expNdx + 1, endNdx);
            }

            //save to replacers expression without closing parentheses
            var varExpr = Expression.Substring(expNdx + 1, endNdx - expNdx - 1);
            //will replace original str
            var newExp = Expression.Substring(expNdx, endNdx - expNdx + 1).Replace(" ", string.Empty);
            //remove old expression and replace with trimmed
            expression = Expression.Remove(expNdx, endNdx - expNdx + 1).Insert(expNdx, newExp);

            var newVal = new VarVal
            {
                ResFormula = new Formula
                    { Expression = varExpr, Variables = Variables.ToDictionary(), EventRef = EventRef }
            };
            newVal.Result = newVal.ResFormula.Result;
            Variables.Add(newExp, newVal);
            expNdx = GetNext("(",
                endNdx - (varExpr.Length + 2 /*"(" and ")" deleted from string then add 2 to length*/ - newExp.Length));
        }

        //extract variables
        foreach (var dynVar in (DynamicTargetEnm[])Enum.GetValues(typeof(DynamicTargetEnm)))
        {
            var ndx = GetNext(dynVar.ToString(), 0);

            while (ndx >= 0)
            {
                /*
                 * since the value in parentheses has been compressed by removing a space,
                 * we can check as follows: there will be a space before the variable, or an index =0
                 */
                if (ndx == 0 || Expression[ndx - 1] == ' ')
                {
                    var endNdx = GetNext(" ", ndx);
                    if (endNdx < 0)
                        endNdx = Expression.Length;
                    var varExpr = Expression.Substring(ndx, endNdx - ndx);
                    Variables.Add(varExpr, new VarVal { ReplaceExpression = varExpr });
                }

                ndx = GetNext(dynVar.ToString(), ndx + 1);
            }
        }

        //calculate variables
        foreach (var variable in Variables.Where(x => x.Value.Result is null))
        foreach (var dynVar in (DynamicTargetEnm[])Enum.GetValues(typeof(DynamicTargetEnm)))
        {
            var expr = variable.Value.ReplaceExpression;
            var ndx = expr?.IndexOf(dynVar.ToString(), StringComparison.Ordinal) ?? -1;
            if (ndx < 0) continue;
            var methodNdx = expr.IndexOf('#') + 1;

            var myFieldInfo = GetType().GetProperty(dynVar.ToString());

            var initUnit = (Unit)myFieldInfo!.GetValue(this);
            object finalMeth = initUnit;
            var prevObject = finalMeth;
            var nextMethod = GetNextMethod(expr, methodNdx, out methodNdx);
            //parsing all method and properties line
            while (nextMethod != string.Empty)
            {
                MethodInfo mInfo;
                //proxy to unit formulas
                if (nextMethod == nameof(UnitFormulas))
                    mInfo = typeof(UnitFormulas).GetMethod(GetNextMethod(expr, methodNdx, out methodNdx));
                else
                    mInfo = finalMeth.GetType().GetMethod(nextMethod);

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
                        else if (prm.ParameterType == typeof(DynamicTargetEnm))
                        {
                            objArr[^1] = dynVar;
                        }
                        else if (prm.ParameterType == typeof(Unit.ElementEnm)||prm.ParameterType == typeof(Unit.ElementEnm?))
                        {
                            if (EventRef is DoTDamage dt)
                                objArr[^1] = dt.Element;
                            else
                                objArr[^1] = EventRef.ParentStep.ActorAbility.Element;
                        }
                        else if (prm.ParameterType == typeof(Unit.ElementEnm)||prm.ParameterType == typeof(Unit.ElementEnm?))
                        {
                            if (EventRef is DoTDamage dt)
                                objArr[^1] = dt.Element;
                            else
                                objArr[^1] = EventRef.ParentStep.ActorAbility.Element;
                        }
                        else if (prm.ParameterType == typeof(Ability.AbilityTypeEnm) || prm.ParameterType ==typeof(Ability.AbilityTypeEnm?))
                        {
                            //if Followup action and ability is not follow up type then add flag
                            Ability.AbilityTypeEnm abilityTypeEnm = EventRef.ParentStep.ActorAbility.AbilityType;
                            if (EventRef.ParentStep.ActorAbility.AbilityType != Ability.AbilityTypeEnm.FollowUpAction &&
                                EventRef.ParentStep.StepType == Step.StepTypeEnm.UnitFollowUpAction)
                                abilityTypeEnm |= Ability.AbilityTypeEnm.FollowUpAction;
                            objArr[^1] = abilityTypeEnm;
                        }
                        else if (prm.ParameterType == typeof(Ability))
                        {
                            objArr[^1] = EventRef.ParentStep.ActorAbility;
                        }
                        else if (prm.ParameterType == typeof(List<EffectTraceRec>))
                        {
                            objArr[^1] = variable.Value.TraceEffects;
                        }
                        else if (prm.ParameterType == typeof(Type))
                        {
                            objArr[^1] = EventRef.ParentStep.ActorAbility;
                            var nextPrm = GetNextMethod(expr, methodNdx, out methodNdx);
                            if (string.IsNullOrEmpty(nextPrm)) continue;
                            //first search Type
                            var searchType = Type.GetType(nextPrm);
                            if (searchType != null)
                                objArr[^1] = searchType;
                            else
                                objArr[^1] = null;
                        }
                       
                    }

                    finalMeth = mInfo.Invoke(prevObject, objArr);
                }

                var pInfo = finalMeth.GetType().GetProperty(nextMethod);
                if (pInfo is not null)
                {
                    prevObject = finalMeth;
                    finalMeth = pInfo.GetValue(finalMeth);
                }


                nextMethod = GetNextMethod(expr, methodNdx, out methodNdx);
            }

            //get result by type

            switch (finalMeth)
            {
                case int nt:
                    variable.Value.Result = (double) nt;
                    break;
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
        if (string.IsNullOrWhiteSpace(Expression))
            throw new Exception("An expression must be defined in the Expression property.");

        double? calculationResult = null;
        var operation = string.Empty;

        foreach (var lexeme in Expression.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries))
            //If it is an operator
            if (lexeme is "*" or "/" or "+" or "-"or "min"or "max")
            {
                operation = lexeme;
            }
            else //It is a number or a variable
            {
                double value;
                if (Variables.TryGetValue(lexeme, out var variable)) //If it is a variable, let's get the variable value
                    value = variable.Result ?? 0;
                else //It is just a number, let's just parse
                    value = double.Parse(lexeme.Replace(",","."), NumberStyles.Any, CultureInfo.InvariantCulture);

                if (!calculationResult.HasValue) //No value has been assigned yet
                    calculationResult = value;
                else
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
                        case "min":
                            calculationResult = Math.Min(calculationResult.Value, value);
                            break;
                        case "max":
                            calculationResult = Math.Max(calculationResult.Value , value);
                            break;
                        default:
                            throw new Exception("The expression is not properly formatted.");
                    }
            }

        if (calculationResult.HasValue)
            return calculationResult.Value;
        throw new Exception("The operation could not be completed, a result was not obtained.");
    }


    public IEnumerable<EffectTraceRec> DescendantsAndSelfEffects()
    {
        foreach (var eff in Variables.Values.SelectMany(x => x.TraceEffects))
            yield return eff;
        foreach (var formula in Variables.Values.Where(x => x.ResFormula != null).Select(x => x.ResFormula))
        foreach (var eff in formula.DescendantsAndSelfEffects())
            yield return eff;
    }

    /// <summary>
    ///     output formula
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
        replacedResults ??= [];
        var nextRunAllowed = true;
        //if short explain then always 1 explain iteration
        var firstRun = shortExplain;

        bool IncompleteLevel(Dictionary<string, VarVal> sVals)
        {
            return nextRunAllowed && sVals.Any(
                x => (x.Value.ReplaceExpression != null && !replacedVariables.ReplacedExpressions.Contains(x.Value))
                     || (x.Value.ResFormula != null && !replacedVariables.ReplacedFormulas.Contains(x.Value))
                     || !replacedVariables.ReplacedRaw.Contains(x.Value)
                     || (x.Value.Result != null && !replacedResults.Contains(x.Value))
            );
        }

        var finalStr = string.Empty;

        if (!shortExplain)
        {
            var enumerable = DescendantsAndSelfEffects().ToArray();
            if (EventRef is DirectDamage dd)
                finalStr += $"(!) Critical hit={dd.IsCrit} Crit rate= {dd.CritRate}" +
                            Environment.NewLine;
            if (enumerable.Any())
                finalStr += "-==USED BUFFS==-" + Environment.NewLine;

            foreach (var buff in enumerable.Distinct().OrderBy(x => x.TraceEffect.GetType().Name)
                         .ThenBy(x => x.TraceBuff.GetType().Name))
            {
                finalStr += $"# {buff.TraceBuff.Explain()} Effect:";
                finalStr += $" {buff.TraceEffect.Explain()} Value= {buff.TraceTotalValue}" + Environment.NewLine;
            }

            if (enumerable.Any())
                finalStr += "-============-" + Environment.NewLine;
        }

        while (firstRun || IncompleteLevel(Variables))
        {
            firstRun = false;
            foreach (var lexeme in Expression.Split([" "],
                         StringSplitOptions.RemoveEmptyEntries))
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
                    else if (!string.IsNullOrEmpty(val.ReplaceExpression) &&
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
                            ? "(" + string.Join("+", val.TraceEffects.Select(x => x.TraceTotalValue)) + ")"
                            : val.Result) + Strings.Space(1);
                    }
                    else
                    {
                        finalStr += (val.TraceEffects.Any()
                            ? "(" + string.Join("+", val.TraceEffects.Select(x => x.TraceTotalValue)) + ")"
                            : val.Result) + Strings.Space(1);
                    }
                }
                else
                {
                    finalStr += lexeme + Strings.Space(1);
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

            finalStr += shortExplain ? "" : " = " + Environment.NewLine;
            nextRunAllowed = !shortExplain;
        }


        return finalStr + (shortExplain ? "" : result);
    }

    public record EffectTraceRec(Buff TraceBuff, Effect TraceEffect, double TraceTotalValue)
    {
        public Buff TraceBuff { get; } = TraceBuff;
        public Effect TraceEffect { get; } = TraceEffect;
        public double TraceTotalValue { get; } = TraceTotalValue;
    }

    public record VarVal
    {
        public double? Result { get; set; }
        public string ReplaceExpression { get; init; }
        public Formula ResFormula { get; set; }
        public List<EffectTraceRec> TraceEffects { get; init; } = [];
    }

    /// <summary>
    ///     Rec with Replace variables
    /// </summary>
    public record ReplacersRec
    {
        public List<VarVal> ReplacedExpressions { get; set; } = [];
        public List<VarVal> ReplacedFormulas { get; set; } = [];
        public List<VarVal> ReplacedRaw { get; set; } = [];
    }
}