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
    public string Explain => ent.CalculateValue is Formula fm ? fm.Explain(out _) : "";
    public object CalculateValue => ent.CalculateValue;

    public IEnumerable<BuffViewModel>? Buffs
    {
        get
        {
            var res = new List<BuffViewModel>(){};
            
            
            switch (ent)
            {
                case DamageEventTemplate { CalculateValue: Formula fm }:
                {
                    res.AddRange(fm.TraceBuffs().Select(buff => new BuffViewModel(buff, CalcFormula, ent)));

                    break;
                }
                case BuffEventTemplate be:
                    res.Add(new BuffViewModel(be.AppliedBuffToApply,CalcFormula,ent));
                    break;
                case SetLiveStatus se:
                {
                    res.AddRange(se.RemovedMods.Select(buff => new BuffViewModel(buff, CalcFormula, ent)));

                    break;
                }
            }
            return res;
        }
    }

    public IEnumerable<EffectViewModel>? Effects
    {
        get
        {
            var res = new List<EffectViewModel>(){};
            
            
            switch (ent)
            {
              
                case ApplyBuffEffect be:
                    res.Add(new EffectViewModel(be.Eff));
                    break;
              
            }
            return res;
        }
    }

    /// <summary>
    /// adaptation to TreeView
    /// </summary>
    public IEnumerable<Formula> Formulas => (ent.CalculateValue is Formula fm) ? new List<Formula>() { fm } : [];

    private Formula? CalcFormula => (ent.CalculateValue is Formula fm) ? fm : null;
}