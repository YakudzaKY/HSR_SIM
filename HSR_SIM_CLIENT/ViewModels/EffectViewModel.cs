using System.Globalization;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT.ViewModels;

public class EffectViewModel (Effect? effect,Formula? formula)
{
    public string Name => effect?.GetType().Name ?? string.Empty;
    public string? Value => effect?.DynamicValue??false?formula?.DescendantsAndSelfEffects().FirstOrDefault(x=>x.TraceEffect==effect)?.TraceTotalValue.ToString(CultureInfo.InvariantCulture)??effect?.Value.ToString() :effect?.Value.ToString();
    public string DynamicValue => effect?.DynamicValue.ToString()??string.Empty;
}