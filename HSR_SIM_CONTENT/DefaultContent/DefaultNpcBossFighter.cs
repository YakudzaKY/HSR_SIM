using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_CONTENT.DefaultContent;

//routine for Elite
internal abstract class DefaultNpcBossFighter : DefaultNPCFighter
{
    protected DefaultNpcBossFighter(Unit parent) : base(parent)
    {
        parent.IsEliteUnit = true;
    }

    public abstract override Ability.ElementEnm Element { get; }
    public override double Cost => Parent.Attack(ent: null).Result * 1.5;
}