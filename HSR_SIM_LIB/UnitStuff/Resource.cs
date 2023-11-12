namespace HSR_SIM_LIB.UnitStuff;

/// <summary>
///     Class for resources. It can be party (like SP) or character HP
/// </summary>
public class Resource : CloneClass
{
    public enum ResourceType
    {
        TP,
        SP,
        HP,
        Toughness,
        Energy
    }

    public ResourceType? ResType { get; set; }

    public double ResVal { get; set; }
}