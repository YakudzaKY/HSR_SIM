using System;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public abstract class DamageEventTemplate(Step parent, ICloneable source, Unit sourceUnit)
    : Event(parent, source, sourceUnit)
{
    protected double? RealBarrierVal { get; private set; }
    public Buff BuffThatDamage { get; init; }

    protected void DamageWorks(bool revert)
    {
        if (RealValue == null)
        {
            //check for barrier 
            if (this is DirectDamage && TargetUnit.AppliedBuffs.Any(x => x.Effects.Any(y => y is EffBarrier)))
            {
                Value = 0;
                RealValue = 0;
                if (this is DirectDamage dd)
                    dd.IsCrit = false;
            }
            else
            {
                var skillVal = (double)Value;
                //find the all shields and max value
                double maxShieldval = 0;
                double srchVal;
                foreach (var mod in TargetUnit.AppliedBuffs.Where(x => x.Effects.Any(y => y is EffShield)))
                foreach (var eff in mod.Effects.Where(x => x is EffShield))
                {
                    srchVal = eff.Value ?? 0;
                    if (srchVal > maxShieldval) maxShieldval = srchVal;
                }

                //cant hit more than val
                RealBarrierVal = Math.Min(maxShieldval,
                    skillVal);
                //get current hp
                var resVal = TargetUnit.GetRes(Resource.ResourceType.HP).ResVal;
                //same shit here
                RealValue = Math.Min(resVal,
                    skillVal - (double)RealBarrierVal);
            }
        }

        //reduce all shields
        foreach (var mod in TargetUnit.AppliedBuffs.Where(x => x.Effects.Any(y => y is EffShield)))
        foreach (var eff in mod.Effects.Where(x => x is EffShield))
            eff.Value -= revert ? -RealBarrierVal : RealBarrierVal;
        var res = TargetUnit.GetRes(Resource.ResourceType.HP);
        res.ResVal += (double)-(revert ? -RealValue : RealValue);
        
    }
}