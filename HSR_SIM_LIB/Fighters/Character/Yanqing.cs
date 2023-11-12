using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character;

public class Yanqing : DefaultFighter
{
    public Yanqing(Unit parent) : base(parent)
    {
    }

    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Hunt;
    public override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Ice;
}