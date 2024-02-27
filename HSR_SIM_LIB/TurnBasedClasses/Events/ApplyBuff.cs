using System;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//apply buff debuff dot etc
public class ApplyBuff : BuffEventTemplate
{
    private AppliedBuff appliedAppliedBuff;
    private int stacksApplied;

    /// <summary>
    ///     place buff/debuff on target. If have some chance to place buff then use AttemptEffect instead
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="source"></param>
    /// <param name="sourceUnit"></param>
    public ApplyBuff(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    public override string GetDescription()
    {
        return $"Apply buff on {TargetUnit.Name}. Source: {Source?.GetType()?.Name:s}";
    }

    public override void ProcEvent(bool revert)
    {
        if (TargetUnit.LivingStatus == Unit.LivingStatusEnm.Alive)
        {
            //calc value first

            foreach (var modEffect in AppliedBuffToApply.Effects.Where(modEffect =>
                         modEffect.CalculateValue != null && modEffect.Value == null))
                modEffect.Value = modEffect.CalculateValue(this);

            if (!revert)
            {
                stacksApplied = TargetUnit.ApplyBuff(this, appliedAppliedBuff ?? AppliedBuffToApply,
                    out appliedAppliedBuff);
            }
            else
            {
                if (TargetUnit.GetStacks(AppliedBuffToApply) - stacksApplied > 0)
                    TargetUnit.AddStack(AppliedBuffToApply, -stacksApplied);
                else
                    TargetUnit.RemoveBuff(this, AppliedBuffToApply);
            }
        }


        base.ProcEvent(revert);
    }
}