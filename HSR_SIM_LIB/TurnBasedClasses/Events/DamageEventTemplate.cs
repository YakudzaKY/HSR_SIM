using System;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events;

public abstract class DamageEventTemplate : Event
{
    public DamageEventTemplate(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
    {
    }
    public double? RealBarrierVal { get; set; }
    public Buff Modification { get; set; }

    public void DamageWorks(bool revert)
    {
        if (RealVal == null)
        {
            //check for barrier 
            if (this is DirectDamage && TargetUnit.Buffs.Any(x => x.Effects.Any(y => y is EffBarrier)))
            {
                Val = 0;
                RealVal = 0;
            }
            else
            {
                double skillVal = (double)Val;
                //find the all shields and max value
                double maxShieldval = 0;
                double srchVal;
                foreach (var mod in TargetUnit.Buffs.Where(x => x.Effects.Any(y => y is EffShield)))
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
                RealVal = Math.Min(resVal,
                    skillVal- (double)RealBarrierVal);
            }
        }

        //reduce all shields
        foreach (var mod in TargetUnit.Buffs.Where(x => x.Effects.Any(y => y is EffShield)))
            foreach (var eff in mod.Effects.Where(x => x is EffShield))
                eff.Value -= revert ? -RealBarrierVal : RealBarrierVal;
        var res = TargetUnit.GetRes(Resource.ResourceType.HP);
        res.ResVal += (double)-(revert ? -RealVal : RealVal);
    }
}