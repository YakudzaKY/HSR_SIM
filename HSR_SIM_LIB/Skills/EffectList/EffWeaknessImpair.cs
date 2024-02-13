using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList
{
    /// <summary>
    /// WeaknessImpair debuff. Used by silver wolf
    /// </summary>
    public class EffWeaknessImpair:Effect
    {
        public Unit.ElementEnm Element { get; init; }
        public override bool DynamicValue { get; } = true;

        public override void OnApply(Event ent, Buff buff, Unit target = null)
        {
            (target??buff.Owner).ResetCondition(ConditionBuff.ConditionCheckParam.Weakness);
            base.OnApply(ent, buff,target);
        }

        public override void OnRemove(Event ent, Buff buff, Unit target = null)
        {
            (target??buff.Owner).ResetCondition(ConditionBuff.ConditionCheckParam.Weakness);
            base.OnRemove(ent, buff, target);
        }
    }
}
