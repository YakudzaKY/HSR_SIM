namespace HSR_SIM_LIB.Fighters.LightCones.Cones;

internal class BeforetheTutorialMissionStarts : DefaultLightCone
{
    public BeforetheTutorialMissionStarts(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
        {
        }
    }

    public sealed override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Nihility;
}