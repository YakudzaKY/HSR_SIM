using System;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

/// <summary>
///     Add effect to existing buff
/// </summary>
public class ApplyBuffEffect(Step parent, ICloneable source, Unit sourceUnit) : Event(parent, source, sourceUnit)
{
    public Effect Eff;

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public AppliedBuff AppliedBuffToApply { get; set; }

    public override string GetDescription()
    {
        var elmName = Eff is EffWeaknessImpair wi ? wi.Element.ToString() : "";
        return $"Apply effect {Eff.GetType().Name} {elmName}{Eff.Value} on {TargetUnit.Name} buff";
    }


    public override void ProcEvent(bool revert)
    {
        var currentAppliedBuff = TargetUnit.AppliedBuffs.FirstOrDefault(x => x.Reference == AppliedBuffToApply);
        if (currentAppliedBuff != null)
            if (!revert)
                currentAppliedBuff.Effects.Add(Eff);
            else
                currentAppliedBuff.Effects.Remove(Eff);


        base.ProcEvent(revert);
    }
}