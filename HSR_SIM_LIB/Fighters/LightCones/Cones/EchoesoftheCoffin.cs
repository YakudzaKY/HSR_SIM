namespace HSR_SIM_LIB.Fighters.LightCones.Cones;

internal class EchoesoftheCoffin : DefaultLightCone
{
    public EchoesoftheCoffin(IFighter parent, int rank) : base(parent, rank)
    {
        if (Path == Parent.Path)
        {
        }
    }

    public override FighterUtils.PathType Path { get; } = FighterUtils.PathType.Abundance;
}