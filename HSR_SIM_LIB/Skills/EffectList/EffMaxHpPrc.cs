using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList;

public class EffMaxHpPrc : Effect
{
    public double GetCorrectedHP(Event ent, Buff buff)
    {
        var buffOwner = buff.Owner ?? ent.TargetUnit;
        var increasedHP = buffOwner.Stats.BaseMaxHp * Value ?? 0 * (StackAffectValue ? buff.Stack : 1);
        return increasedHP * buffOwner.GetHpPrc(ent);
    }

    public override void BeforeApply(Event ent, Buff buff)
    {
        var buffOwner = buff.Owner ?? ent.TargetUnit;
        buffOwner.GetRes(Resource.ResourceType.HP).ResVal += GetCorrectedHP(ent, buff);

        base.BeforeApply(ent, buff);
    }


    public override void BeforeRemove(Event ent, Buff buff)
    {
        var buffOwner = buff.Owner ?? ent.TargetUnit;
        buffOwner.GetRes(Resource.ResourceType.HP).ResVal -= GetCorrectedHP(ent, buff);
        base.BeforeRemove(ent, buff);
    }
}