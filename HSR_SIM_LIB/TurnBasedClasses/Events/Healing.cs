using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    public class Healing:Event
    {
        public Healing(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return $"Healing {TargetUnit.Name} " +
                   $" overall={Val:f} to_hp={RealVal:f}";
        }

        public override void ProcEvent(bool revert)
        {
            Resource res = TargetUnit.GetRes(Resource.ResourceType.HP);
            if (RealVal == null)
            {
         
  
                //cant overheal
                RealVal = Math.Min(TargetUnit.GetMaxHp(null) -res.ResVal ,(double)Val );
            }
    
           
            res.ResVal += (double)(revert ? -RealVal : RealVal);
            base.ProcEvent(revert);
        }
    }
}
