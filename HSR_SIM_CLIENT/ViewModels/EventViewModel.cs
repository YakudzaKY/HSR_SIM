using System.Runtime.InteropServices.JavaScript;
using System.Windows.Forms;
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

    public IEnumerable<Buff>? Buffs
    {
        get
        {
            if (ent is DamageEventTemplate de)
            {
                IEnumerable<Buff>? res = (de.CalculateValue is Formula fm)
                    ? fm.DescendantsAndSelfEffects().Select(y => y.TraceBuff).Distinct()
                    : null;
                return res;
            }


            return null;
        }
    }

    public IEnumerable<Formula> Formulas => (ent.CalculateValue is Formula fm) ? new List<Formula>() { fm } : [];
}