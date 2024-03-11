using HSR_SIM_LIB.Skills;

namespace HSR_SIM_CLIENT.ViewModels;

public class EffectViewModel (Effect effect)
{
    public string Name => effect?.GetType().Name ?? string.Empty;
    public double? Value => effect?.Value;
    public string DynamicValue => effect?.DynamicValue.ToString()??string.Empty;
}