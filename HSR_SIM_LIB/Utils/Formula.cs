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


    public double CalculateResult()
    {
        ParseVariables();
        if (string.IsNullOrWhiteSpace(this.Expression))
            throw new Exception("An expression must be defined in the Expression property.");

        double? result = null;
        string operation = string.Empty;

        //This will be necessary for priorities operations such as parentheses, etc... It is not being used at this point.
        List<double> aux = new List<double>();

        foreach (var lexema in Expression.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))
        {
            //If it is an operator
            if (lexema == "*" || lexema == "/" || lexema == "+" || lexema == "-")
            {
                operation = lexema;
            }
            else //It is a number or a variable
            {
                double value = double.MinValue;
                if (Variables.ContainsKey(lexema)) //If it is a variable, let's get the variable value
                    value = Variables[lexema].Result ?? 0;
                else //It is just a number, let's just parse
                    value = double.Parse(lexema);

                if (!result.HasValue) //No value has been assigned yet
                {
                    result = value;
                }
                else
                {
                    switch (operation) //Let's check the operation we should perform
                    {
                        case "*":
                            result = result.Value * value;
                            break;
                        case "/":
                            result = result.Value / value;
                            break;
                        case "+":
                            result = result.Value + value;
                            break;
                        case "-":
                            result = result.Value - value;
                            break;
                        default:
                            throw new Exception("The expression is not properly formatted.");
                    }
                }
            }
        }

        if (result.HasValue)
            return result.Value;
        else
            throw new Exception("The operation could not be completed, a result was not obtained.");
    }

    public string Explain(bool isFinish=true)
    {

        string finalStr = String.Empty;
        foreach (var lexema in Expression.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (Variables.TryGetValue(lexema, out var val))
            {
                string valStr = val.ResFormula != null ? val.ResFormula.Explain(false) +"=" :String.Empty;
                finalStr += $"{lexema}[{valStr}{val.Result}]"+Strings.Space(1);
            }
            else
                finalStr += lexema+Strings.Space(1);
        }

        return finalStr+(isFinish?" = "+Result:"");
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