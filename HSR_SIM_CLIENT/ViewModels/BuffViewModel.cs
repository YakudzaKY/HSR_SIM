using System.Windows.Media.Imaging;
using HSR_SIM_CLIENT.Utils;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT.ViewModels;

/// <summary>
///     VM creation for buff
/// </summary>
/// <param name="buff">model</param>
/// <param name="formula">used for effect info(calculated values) can be null</param>
/// <param name="ent">
///     if param exists all conditions will be marked as TRUE(becase conditions was pre calculated in event
///     proc)
/// </param>
public class BuffViewModel(Buff? buff, Formula? formula = null, Event? ent = null)
{
    private List<ConditionViewModel>? applyConditions;

    public string EffectDescription
    {
        get
        {
            var res = string.Empty;
            buff?.Effects.ForEach(x =>
                res +=
                    $"{x.Explain(formula?.DescendantsAndSelfEffects().FirstOrDefault(z => z.TraceEffect == x)?.TraceTotalValue)}; ");
            return res;
        }
    }

    public List<ConditionViewModel>? ApplyConditions
    {
        get
        {
            if (applyConditions == null && PassiveBuffRef?.ApplyConditions != null)
            {
                applyConditions = new List<ConditionViewModel>();
                foreach (var condition in PassiveBuffRef.ApplyConditions)
                    applyConditions.Add(new ConditionViewModel(condition, PassiveBuffRef,
                        EventRef != null ? true : null));
            }

            return applyConditions;
        }
    }

    public Event? EventRef => ent;
    public Buff BuffRef => buff;

    public IEnumerable<EffectViewModel>? Effects
    {
        get
        {
            var res = new List<EffectViewModel>();
            if (buff is not null)
                foreach (var eff in buff.Effects)
                    res.Add(new EffectViewModel(eff, formula));

            return res;
        }
    }

    public BitmapImage? Icon => buff?.IconImage != null ? buff?.IconImage!.ToBitmapImage() : null;

    public string? SourceUnit => buff?.SourceUnit.PrintName;
    public string? SourceObject => buff?.SourceObject.GetType().ToString();
    public string? Type => buff?.GetType().Name;
    public string? BuffType => buff?.Type.ToString();
    public string? CarrierUnit => buff?.CarrierUnit?.PrintName;
    public string? Stacks => buff != null ? $"current: {buff.Stack} start: {buff.Reference?.Stack}" : "";
    public int? MaxStacks => buff?.MaxStack;
    public AppliedBuff? AppliedBuffRef => buff as AppliedBuff;
    public PassiveBuff? PassiveBuffRef => buff as PassiveBuff;
}