using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    public  abstract class DamageEventTemplate:Event
    {
        public double? RealBarrierVal { get => realBarrierVal; set => realBarrierVal = value; }
        private double? realBarrierVal;
        public Mod Modification { get; set; }
        public bool CanSetToZero { get; init; } = true;

        public DamageEventTemplate(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public void DamageWorks(bool revert)
        {
     
            if (RealVal == null)
            {
                //find the all shields and max value
                double maxShieldval = 0;
                double srchVal;
                foreach (Mod mod in TargetUnit.Mods.Where(x => x.Effects.Any(y => y.EffType == Effect.EffectType.Shield)))
                {
                    foreach (Effect eff in mod.Effects.Where(x => x.EffType == Effect.EffectType.Shield))
                    {
                        srchVal = eff.Value ?? 0;
                        if (srchVal > maxShieldval)
                        {
                            maxShieldval = srchVal;
                        }
                    }

                }
                //cant hit more than val
                RealBarrierVal = Math.Min((double)maxShieldval,
                    (double)Val);
                //get current hp
                var resVal = TargetUnit.GetRes(Resource.ResourceType.HP).ResVal;
                //same shit here
                RealVal = Math.Min((double)resVal,
                    (double)Val - (double)RealBarrierVal);
            }

            //reduce all shields
            foreach (Mod mod in TargetUnit.Mods.Where(x => x.Effects.Any(y => y.EffType == Effect.EffectType.Shield)))
            {
                foreach (Effect eff in mod.Effects.Where(x => x.EffType == Effect.EffectType.Shield))
                {
                    eff.Value -= revert ? -RealBarrierVal : RealBarrierVal;
                }
            }
            Resource res = TargetUnit.GetRes(Resource.ResourceType.HP);
            res.ResVal += (double)-(revert ? -RealVal : RealVal);
        }
        

    }
}
