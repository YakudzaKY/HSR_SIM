using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList
{
    
    internal class EffWeaknessImpair:Effect
    {
        public Unit.ElementEnm Element { get; init; }

        public override void OnApply(Event ent, Buff buff)
        {
            buff.Owner.ResetCondition(ConditionBuff.ConditionCheckParam.Weakness);
            base.OnApply(ent, buff);
        }

        public override void OnRemove(Event ent, Buff buff)
        {
            buff.Owner.ResetCondition(ConditionBuff.ConditionCheckParam.Weakness);
            base.OnRemove(ent, buff);
        }
    }
}
