using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.UnitStuff;

public class Resist
{
    public required Ability.ElementEnm ResistType { get; init; }
    public required double ResistVal { get; init; }
}