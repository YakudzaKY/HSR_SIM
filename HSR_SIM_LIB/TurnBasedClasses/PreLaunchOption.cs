namespace HSR_SIM_LIB.TurnBasedClasses;

/*
 *some pre launch options
 * like set team points to value
 * or fill energy of team
 */
public class PreLaunchOption
{
    public enum PreLaunchOptionEnm
    {
        SetEnergy,
        SetTp,
        SetSp
    }

    public PreLaunchOptionEnm OptionType { get; set; }
    public double Value { get; set; }
}