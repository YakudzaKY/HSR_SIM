using HSR_SIM_CONTENT.DefaultContent;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.Content.Character;

public class Jingliu : DefaultFighter
{
    public Jingliu(Unit parent) : base(parent)
    {
    }

    public override double MaxEnergy { get; } = 140;
    public override Ability.ElementEnm Element { get; } = Ability.ElementEnm.Ice;
    public override FighterUtils.PathType? Path { get; } = FighterUtils.PathType.Destruction;
}