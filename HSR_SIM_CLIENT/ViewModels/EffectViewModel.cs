using System.Globalization;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.Utils;

namespace HSR_SIM_CLIENT.ViewModels;

public class EffectViewModel(Effect? effect, Formula? formula = null)
{
    public string Name => effect?.GetType().Name ?? string.Empty;

    //IF impair then return element else if dynamic value then try get value from formula else get effect Value
    public string? Value => effect is EffWeaknessImpair ew
        ? ew.Element.ToString()
        :
        effect?.DynamicValue ?? false
            ?
            formula?.DescendantsAndSelfEffects().FirstOrDefault(x => x.TraceEffect == effect)?.TraceTotalValue
                .ToString(CultureInfo.InvariantCulture) ?? effect?.Value.ToString()
            : effect?.Value.ToString();

    public string DynamicValue => effect?.DynamicValue.ToString() ?? string.Empty;
}