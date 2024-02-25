using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Fighters;

//routine for Elite
internal class DefaultNPCBossFIghter : DefaultNPCFighter
{
    public DefaultNPCBossFIghter(Unit? parent) : base(parent)
    {
    }

    public override double Cost => Parent.GetAttack(null) * 1.5;
    public override bool IsEliteUnit => true;
}