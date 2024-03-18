using System.Runtime.InteropServices.JavaScript;
using System.Windows.Forms;
using HSR_SIM_CLIENT.Views;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT.ViewModels;

public class EventViewModel(Event ent)
{
    private readonly string emptyStr = "(empty)";
    public string Name => ent.PrintName;
    public string Source => ent.Source?.GetType().ToString() ?? emptyStr;
    public string SourceUnit => ent.SourceUnit?.PrintName ?? emptyStr;
    public string TargetUnit => ent.TargetUnit?.PrintName ?? emptyStr;
    public string Value => ent.Value?.ToString() ?? string.Empty;
    public string RealValue => ent.RealValue?.ToString() ?? string.Empty;
    public string Description => ent.GetDescription();
    public string Explain => ent.CalculateValue is Formula fm ? fm.Explain() : "";
    public object CalculateValue => ent.CalculateValue;

    public IEnumerable<BuffViewModel>? Buffs
    {
        get
        {
            var res = new List<BuffViewModel>(){};
            
            
            if (ent is DamageEventTemplate { CalculateValue: Formula fm })
            {
                foreach (var buff in fm.DescendantsAndSelfEffects().Select(y => y.TraceBuff).Distinct())
                {
                    res.Add(new BuffViewModel(buff,Formula));
                }
           
            }
            if (ent is BuffEventTemplate be)
            {
                res.Add(new BuffViewModel(be.AppliedBuffToApply,Formula));
                
            }


            return res;
        }
    }

    /// <summary>
    /// adoptation to TreeView
    /// </summary>
    public IEnumerable<Formula> Formulas => (ent.CalculateValue is Formula fm) ? new List<Formula>() { fm } : [];
    public Formula? Formula => (ent.CalculateValue is Formula fm) ? fm : null;
}