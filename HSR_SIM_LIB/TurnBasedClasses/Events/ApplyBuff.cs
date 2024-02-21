using System;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

//apply buff debuff dot etc
public class ApplyBuff : BuffEventTemplate
{
    private int stacksApplied = 0;
    /// <summary>
    /// place buff/debuff on target. If have some chance to place buff then use AttemptEffect instead
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="source"></param>
    /// <param name="sourceUnit"></param>
    public ApplyBuff(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }

    private Buff appliedBuff;

    public override string GetDescription()
    {
        return $"Apply buff on {TargetUnit.Name}. Source: {Source?.GetType()?.Name:s}";
    }

    public override void ProcEvent(bool revert)
    {
  
        if (TargetUnit.LivingStatus==Unit.LivingStatusEnm.Alive)
        {
            //calc value first
            foreach (var modEffect in BuffToApply.Effects.Where(modEffect =>
                         modEffect.CalculateValue != null && modEffect.Value == null))
                modEffect.Value = modEffect.CalculateValue(this);

            if (!revert)
            {
                
                stacksApplied = TargetUnit.ApplyBuff(this, appliedBuff??BuffToApply, out appliedBuff);

            }
            else
            {
                if (TargetUnit.GetStacks(BuffToApply) - stacksApplied > 0)
                    TargetUnit.AddStack(BuffToApply, -stacksApplied);
                else
                {
                    TargetUnit.RemoveBuff(this, BuffToApply);
                }



            }
        }




        base.ProcEvent(revert);
    }
}