using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //drain some resource
    public class ResourceDrain : Event
    {
        public Resource.ResourceType ResType { get => resType; set => resType = value; }
        private Resource.ResourceType resType;
        public bool CanSetToZero { get; init; } = true;
        public ResourceDrain(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return TargetUnit.Name + " res drain : " + Val + " " + ResType.ToString() + "(by " + SourceUnit.Name + ")";
        }

        public override void ProcEvent(bool revert)
        {
            
            //Resource drain

            if (RealVal == null)
            {
                RealVal = Math.Min((double)TargetUnit.GetRes(ResType).ResVal, (double)Val);
                if (!CanSetToZero && RealVal >=(double)TargetUnit.GetRes(ResType).ResVal)
                {
                    RealVal -= 1;
                }
                //Game mechanics depend on Val instead
                Val = RealVal;
            }

            TargetUnit.GetRes(ResType).ResVal += (double)-(revert ? -RealVal : RealVal );
            base.ProcEvent(revert);
        }
    }
}
