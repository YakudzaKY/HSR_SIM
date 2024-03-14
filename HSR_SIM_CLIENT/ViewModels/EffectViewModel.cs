using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT.ViewModels;

public class EffectViewModel (Effect? effect,Formula? formula)
{
    public string Name => effect?.GetType().Name ?? string.Empty;
    public string? Value => effect?.DynamicValue??false?formula?.DescendantsAndSelfEffects().First(x=>x.TraceEffect==effect).TraceTotalValue.ToString() :effect?.Value.ToString();
    public string DynamicValue => effect?.DynamicValue.ToString()??string.Empty;
}