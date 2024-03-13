using System.Collections.Generic;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

/// <summary>
///     freeze on weakness break
/// </summary>
public class AppliedBuffFreezeWb : AppliedBuff
{
    public AppliedBuffFreezeWb(Unit sourceUnit, AppliedBuff reference =null ) : base(sourceUnit, reference,typeof(AppliedBuffFreezeWb))
    {
        Type = BuffType.Debuff;
        BaseDuration = 1;
        Effects = new List<Effect> { new EffFreeze { DoTCalculateValue = FighterUtils.WeaknessBreakFormula()  } };
        EventHandlerProc += FreezeEventHandler;
    }

    private void FreezeEventHandler(Event ent)
    {
        if (ent is ResetAV && ent.TargetUnit == CarrierUnit) //50% reduce av if frosted
            ent.ChildEvents.Add(new ModActionValue(ent.ParentStep, CarrierUnit, CarrierUnit)
            {
                TargetUnit = CarrierUnit, Value = CarrierUnit.GetActionValue(ent) * 0.5
            });
    }
}