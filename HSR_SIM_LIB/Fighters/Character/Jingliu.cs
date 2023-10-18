using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters.Character
{
    public class Jingliu:DefaultFighter
    {
        public  override FighterUtils.PathType? Path { get; set; } = FighterUtils.PathType.Destruction;
        public Jingliu(Unit parent) : base(parent)
        {
        }
    }
}
