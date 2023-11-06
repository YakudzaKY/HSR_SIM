using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList
{
    public class EffMaxHp : Effect
    {
        public double GetCorrectedHP(Event ent, Buff buff)
        {
            Unit buffOwner = buff.Owner ?? ent.TargetUnit;
            double increasedHP = buffOwner.Stats.BaseMaxHp +(this.Value??0 * (this.StackAffectValue ? buff.Stack : 1));
            return increasedHP * buffOwner.GetHpPrc(ent);
        }
        public override void BeforeApply(Event ent, Buff buff)
        {
            Unit buffOwner = buff.Owner ?? ent.TargetUnit;
            buffOwner.GetRes(Resource.ResourceType.HP).ResVal += GetCorrectedHP(ent, buff);
            
            base.BeforeApply(ent, buff);
        }



        public override void BeforeRemove(Event ent, Buff buff)
        {
            Unit buffOwner = buff.Owner ?? ent.TargetUnit;
            buffOwner.GetRes(Resource.ResourceType.HP).ResVal -= GetCorrectedHP(ent, buff);
            base.BeforeRemove(ent, buff);
        }
    }
}
