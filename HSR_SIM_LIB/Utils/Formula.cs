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
public class Formula
{
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
        public List<Effect> TraceEffects { get; set; }
    }

    /// <summary>
    /// This simply stores a variable name and its value so when this key is found in a expression it gets the value accordingly.
    /// </summary>
    public Dictionary<string, VarVal> Variables { get; init; } = new();

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
            if (ndx + 1 >= pstr.Length)
            {
                nextPos = 0;
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
        foreach (DynamicTargetEnm dynVar in (DynamicTargetEnm[])Enum.GetValues(typeof(DynamicTargetEnm)))
        {
            string expr = variable.Value.ReplaceExpression;
            int ndx = expr.IndexOf(dynVar.ToString(), StringComparison.Ordinal);
            if (ndx >= 0)
            {
                int methodNdx = expr.IndexOf("#", StringComparison.Ordinal) + 1;

                PropertyInfo myFieldInfo = this.GetType().GetProperty(dynVar.ToString());

                Unit initUnit = (Unit)myFieldInfo!.GetValue(this);
                object finalMeth = initUnit;
                object prevObject = finalMeth;
                string nextMethod = GetNextMethod(expr, methodNdx, out methodNdx);
                //parsing all method and properties line
                while (nextMethod != String.Empty)
                {
                    //TODO: check for params if current method is not standard(event)
                    //TODO:and repeat GetNextMethod additional X times where x= method params-1(event already exists)
                    MethodInfo mInfo = finalMeth.GetType().GetMethod(nextMethod);
                    if (mInfo is not null)
                    {
                        prevObject = finalMeth;
                        finalMeth = mInfo.Invoke(prevObject, [EventRef]);
                        ;
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

    private record ExplainRec(string Expr, Dictionary<string, VarVal> Dict)
    {
        public string Expression { get; set; } = Expr;
        public Dictionary<string, VarVal> Variables { get; set; } = Dict;
    }

    /// <summary>
    /// output formula 
    /// </summary>
    /// <param name="shortExplain"></param>
    /// <param name="pReplaceList"></param>
    /// <param name="pFormulas"></param>
    /// <param name="pResults"></param>
    /// <param name="pRaw"></param>
    /// <param name="pReplaceListNew"></param>
    /// <param name="pFormulasNew"></param>
    /// <param name="pResultsNew"></param>
    /// <param name="pRawNew"></param>
    /// <returns></returns>
    public string Explain(bool shortExplain = false, List<VarVal> pReplaceList = null, List<VarVal> pFormulas = null,
        List<VarVal> pResults = null, List<VarVal> pRaw = null,
        List<VarVal> pReplaceListNew = null, List<VarVal> pFormulasNew = null,
        List<VarVal> pResultsNew = null, List<VarVal> pRawNew = null)
    {
        List<VarVal> parsedReplaceExpressions = pReplaceList ?? [];
        List<VarVal> parsedFormulas = pFormulas ?? [];
        List<VarVal> parsedResults = pResults ?? [];
        List<VarVal> parsedRaw = pRaw ?? [];

        //get allowed to replace values once per while run
        List<VarVal> parsedReplaceExpressionsNew = pReplaceListNew ?? [];
        List<VarVal> parsedFormulasNew = pFormulasNew ?? [];
        List<VarVal> parsedResultsNew = pResultsNew ?? [];
        List<VarVal> parsedRawNew = pRawNew ?? [];

        bool nextRunAllowed = true;

        bool IncompleteLevel(Dictionary<string, VarVal> sVals)
        {
            return nextRunAllowed && sVals.Any(
                x => (x.Value.ReplaceExpression != null && !parsedReplaceExpressions.Contains(x.Value))
                     && (x.Value.ResFormula != null || !parsedFormulas.Contains(x.Value))
                     || !parsedRaw.Contains(x.Value)
                     || (x.Value.Result != null && !parsedResults.Contains(x.Value))
            );
        }

        var finalStr = String.Empty;


        while (IncompleteLevel(Variables))
        {
            foreach (var lexeme in Expression.Split(new string[] { " " },
                         StringSplitOptions.RemoveEmptyEntries))
            {
                if (Variables.TryGetValue(lexeme, out var val))
                {
                    //if expression exists and not handled
                    if (val.ReplaceExpression == lexeme)
                        if (!parsedRaw.Contains(val))
                            parsedRaw.Add(val);
                    if (!parsedRaw.Contains(val))
                    {
                        if (!parsedRawNew.Contains(val))
                            parsedRawNew.Add(val);
                        finalStr += lexeme + Strings.Space(1);
                    }
                    else if (!String.IsNullOrEmpty(val.ReplaceExpression) && !parsedReplaceExpressions.Contains(val))
                    {
                        if (!parsedReplaceExpressionsNew.Contains(val))
                            parsedReplaceExpressionsNew.Add(val);
                        var valStr = val.ReplaceExpression;
                        finalStr += $"{valStr}" + Strings.Space(1);
                    }
                    else if (val.ResFormula != null && !parsedFormulas.Contains(val))
                    {
                        /*
                         * TODO: убрать () * 2 + () , т.к. на последней итерации Explain зовется когда IncompleteLevel =true
                         * BladeAttack * 2 + BladeAttack
                           = Attacker#GetAttackFs * 2 + Attacker#GetAttackFs
                           = (1 + Attacker#Stats#AttackPrc * Attacker#Stats#BaseAttack ) * 2 + (1 + Attacker#Stats#AttackPrc * Attacker#Stats#BaseAttack )
                           = (1 + 0,1080000063 * 1125,4319999999998 ) * 2 + (1 + 0,1080000063 * 1125,4319999999998 )
                           = () * 2 + ()  = 1246,9786630902213 * 2 + 1246,9786630902213  = 3740,935989270664

                         */
                        var valStr = "(" + val.ResFormula.Explain(true, parsedReplaceExpressions, parsedFormulas,
                                         parsedResults, parsedRaw, parsedReplaceExpressionsNew, parsedFormulasNew,
                                         parsedResultsNew, parsedRawNew) +
                                     ")";
                        if (!IncompleteLevel(val.ResFormula.Variables))
                            if (!parsedFormulasNew.Contains(val))
                                parsedFormulasNew.Add(val);
                        finalStr += $"{valStr}" + Strings.Space(1);
                    }
                    else if (val.Result != null && !parsedResults.Contains(val))
                    {
                        if (!parsedResultsNew.Contains(val))
                            parsedResultsNew.Add(val);
                        finalStr += val.Result + Strings.Space(1);
                    }
                    else
                    {
                        finalStr += val.Result + Strings.Space(1);
                    }
                }
                else
                {
                    finalStr += lexeme + Strings.Space(1);
                }
            }

            if (!shortExplain)
            {
                parsedReplaceExpressions.AddRange(parsedReplaceExpressionsNew);
                parsedFormulas.AddRange(parsedFormulasNew);
                parsedResults.AddRange(parsedResultsNew);
                parsedRaw.AddRange(parsedRawNew);
            }

            finalStr += (shortExplain ? "" : " = ");
            nextRunAllowed = !shortExplain;
        }


        return finalStr + (shortExplain ? "" : result);
    }

    /// <summary>
    /// Add variables to the dynamic math formula. The variable should be properly declared.
    /// </summary>
    /// <param name="variableDeclaration">Should be declared as "VariableName=VALUE" without spaces</param>
    public void AddVariable(string variableDeclaration)
    {
        if (!string.IsNullOrWhiteSpace(variableDeclaration))
        {
            var variable =
                variableDeclaration.ToLower()
                    .Split('='); //Let's make sure the variable's name is LOWER case and then get its name/value
            string variableName = variable[0];

            if (double.TryParse(variable[1], out var variableValue))
                this.Variables.Add(variableName, new VarVal() { Result = variableValue });
            else
                throw new ArgumentException("Variable value is not a number");
        }
        else
        {
            //Could throw an exception... or just ignore as it not important...
        }
    }
}