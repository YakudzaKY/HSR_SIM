using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.TurnBasedClasses.Events
{
    public  class SetLiveStatus:Event
    {
        public Unit.LivingStatusEnm ToState { get; set; }
        private Unit.LivingStatusEnm fromState;
        public SetLiveStatus(Step parentStep, ICloneable source, Unit sourceUnit) : base(parentStep, source, sourceUnit)
        {

        }

        public override void ProcEvent(bool revert)
        {
            if (!TriggersHandled)
                fromState = this.TargetUnit.LivingStatus;
            if (!revert)
                TargetUnit.LivingStatus=ToState;
            else
                TargetUnit.LivingStatus=fromState;
            TargetUnit.ParentTeam.ResetRoles();     
            //need reset enemy team roles(if 1 enemy is alive then Hunt>Destruction)
            TargetUnit.EnemyTeam.ResetRoles();
            
            base.ProcEvent(revert);
        }

        public override string GetDescription()
        {
            return $"set {TargetUnit.Name} state to {ToState.ToString()}";
        }
    }
}
