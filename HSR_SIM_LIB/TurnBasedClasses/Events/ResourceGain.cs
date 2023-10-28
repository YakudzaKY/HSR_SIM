using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //drain some resource
    public class ResourceGain : Event
    {
        public Resource.ResourceType ResType { get => resType; set => resType = value; }
        private Resource.ResourceType resType;
        public ResourceGain(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return TargetUnit.Name + " res gain : " + Val + " " + ResType.ToString() + "(by " + SourceUnit.Name + ")";
        }

        public override void ProcEvent(bool revert)
        {
            Resource res = TargetUnit.GetRes(ResType);
            //Resource drain
            RealVal ??= ResType switch
            {
                Resource.ResourceType.Toughness => Math.Min((double)Val, TargetUnit.Stats.MaxToughness-res.ResVal),
                Resource.ResourceType.HP =>  Math.Min(TargetUnit.Stats.MaxHp-res.ResVal ,(double)Val ),
                Resource.ResourceType.Energy => Math.Min((double)Val, (double)TargetUnit.Stats.BaseMaxEnergy-res.ResVal),
                _ => Val
            };
            res.ResVal+= (double)(revert ? -RealVal : RealVal);
            base.ProcEvent(revert);
        }
    }
}
