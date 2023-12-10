using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills;

public class PassiveBuff
{
    public PassiveBuff(Unit parentUnit)
    {
        Parent = parentUnit;
    }

    public Buff AppliedBuff { get; set; }
    public object Target { get; set; } //in most cases target==parent, but when target is full team then not
    public Unit Parent { get; init; }
    public bool IsTargetCheck { get; set; }
}