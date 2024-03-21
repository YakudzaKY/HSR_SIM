using System.Globalization;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;


namespace HSR_SIM_CLIENT.ViewModels;

/// <summary>
/// 
/// </summary>
/// <param name="condition"></param>
/// <param name="buff"></param>
/// <param name="overrideTruly">set Truly to always true. For example in DirectDamage when only active buffs appears</param>
public class ConditionViewModel(Condition condition, PassiveBuff? buff, bool? overrideTruly = null)
{
    public bool Truly => overrideTruly ?? condition.Truly(parentBuff: buff);
    public string Description => $"[{Truly}] {condition.ConditionParam}";
    public string ConditionParam => condition.ConditionParam.ToString();
    public string ConditionExpression => condition.ConditionExpression.ToString();
    public Ability.ElementEnm? ElemValue => condition.ElemValue;
    public double? Value => condition.Value;
    public Resource.ResourceType? ResourceValue => condition?.ResourceValue;


    public IEnumerable<BuffViewModel>? Buffs
    {
        get
        {
            IEnumerable<BuffViewModel>? res = null;
            if (condition.AppliedBuffValue != null)
                res = new List<BuffViewModel>
                {
                    new BuffViewModel(condition.AppliedBuffValue, null, null)
                };


            return res;
        }
    }
}