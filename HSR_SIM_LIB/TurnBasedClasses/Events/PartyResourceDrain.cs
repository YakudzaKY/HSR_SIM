using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    //drain party resource
    public class PartyResourceDrain : Event
    {
        public Resource.ResourceType ResType { get => resType; set => resType = value; }
        private Resource.ResourceType resType;
        public Team TargetTeam { get; set; }
        public PartyResourceDrain(Step parent, ICloneable source, Unit sourceUnit) : base(parent, source, sourceUnit)
        {
        }

        public override string GetDescription()
        {
            return "Party res drain : " + Val + " " + ResType.ToString();
        }

        public override void ProcEvent(bool revert)
        {
            Team tarTeam = TargetTeam ?? SourceUnit.ParentTeam;
            //SP or technical points
            tarTeam.GetRes(ResType).ResVal -= (double)(revert ? -Val : Val);
            base.ProcEvent(revert);

        }
    }
}
