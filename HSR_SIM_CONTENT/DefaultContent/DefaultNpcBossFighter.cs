using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.DefaultContent;

//routine for Elite
internal class DefaultNpcBossFighter : DefaultNPCFighter
{
    protected DefaultNpcBossFighter(Unit parent) : base(parent)
    {
        parent.IsEliteUnit = true;
    }

    public override double Cost => Parent.GetAttack(ent:null).Result * 1.5;

}