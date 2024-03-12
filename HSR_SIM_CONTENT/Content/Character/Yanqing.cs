using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.Character;

public class Yanqing : DefaultFighter
{
    public Yanqing(Unit? parent) : base(parent)
    {
        Parent.Element = Unit.ElementEnm.Ice;
    }

    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Hunt;
}