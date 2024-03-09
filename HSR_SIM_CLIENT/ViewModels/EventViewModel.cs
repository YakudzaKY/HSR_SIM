using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_CLIENT.ViewModels;

public class EventViewModel(Event ent)
{
    private readonly string emptyStr="(empty)";
    public string Name => ent.PrintName;
    public string Source => ent.Source?.GetType().ToString()??emptyStr;
    public string SourceUnit => ent.SourceUnit?.PrintName??emptyStr;
    public string TargetUnit => ent.TargetUnit?.PrintName??emptyStr;
    public string Value => ent.Value?.ToString()??String.Empty;
    public string RealValue => ent.RealValue?.ToString() ?? String.Empty;

}