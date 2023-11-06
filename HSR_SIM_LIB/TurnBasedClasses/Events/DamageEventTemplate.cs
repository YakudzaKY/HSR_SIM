﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    public  abstract class DamageEventTemplate:Event
    {
        public double? RealBarrierVal { get => realBarrierVal; set => realBarrierVal = value; }
        private double? realBarrierVal;
        public Buff Modification { get; set; }
        public bool CanSetToZero { get; init; } = true;

        public DamageEventTemplate(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

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


                    //find the all shields and max value
                    double maxShieldval = 0;
                    double srchVal;
                    foreach (Buff mod in TargetUnit.Buffs.Where(x => x.Effects.Any(y => y is EffShield)))
                    {
                        foreach (Effect eff in mod.Effects.Where(x => x is EffShield))
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
            }

            //reduce all shields
            foreach (Buff mod in TargetUnit.Buffs.Where(x => x.Effects.Any(y => y is EffShield)))
            {
                foreach (Effect eff in mod.Effects.Where(x => x is EffShield))
                {
                    eff.Value -= revert ? -RealBarrierVal : RealBarrierVal;
                }
            }
            Resource res = TargetUnit.GetRes(Resource.ResourceType.HP);
            res.ResVal += (double)-(revert ? -RealVal : RealVal);
        }
        

    }
}