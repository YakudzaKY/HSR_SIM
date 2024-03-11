using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.ReadyBuffs;

/// <summary>
///     Imprisonment on weakness break
/// </summary>
public class AppliedBuffImprisonmentWb : AppliedBuff
{
    public AppliedBuffImprisonmentWb(Unit sourceUnit, AppliedBuff reference =null , Event ent  =null ) : base(sourceUnit,
        reference,typeof(AppliedBuffImprisonmentWb))
    {
        Type = BuffType.Debuff;
        BaseDuration = 1;
        Effects =
        [
            new EffImprisonment(),
            new EffDelay { Value = 0.30 * (1 + sourceUnit.GetBreakDmg(ent)) },
            new EffSpeedPrc { Value = -0.1 }
        ];
    }
}