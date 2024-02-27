using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.Character;

public class Jingliu : DefaultFighter
{
    public Jingliu(Unit? parent) : base(parent)
    {
        Parent.Stats.BaseMaxEnergy = 140;
    }

    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Destruction;
    public override Unit.ElementEnm Element { get; } = Unit.ElementEnm.Ice;
}