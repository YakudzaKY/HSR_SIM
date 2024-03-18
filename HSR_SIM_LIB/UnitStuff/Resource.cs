using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.UnitStuff;

/// <summary>
///     Class for resources. It can be party (like SP) or character HP
/// </summary>
public class Resource(CloneClass parent) : CloneClass
{
    public enum ResourceType
    {
        TP,
        SP,
        HP,
        Toughness,
        Energy
    }

    private double resVal;

    public ResourceType? ResType { get; set; }
    private CloneClass Parent { get; } = parent;

    public double ResVal
    {
        get => resVal;

        set
        {
            resVal = value;
            if (ResType == ResourceType.HP) ((Unit)Parent).ResetCondition(Condition.ConditionCheckParam.HpPrc);
        }
    }
}