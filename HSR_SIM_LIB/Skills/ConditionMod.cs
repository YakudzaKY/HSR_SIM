using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills
{
    public class ConditionMod : PassiveMod
    {

        public ConditionRec Condition { get; set; }
  


        private bool CheckExpression(double targetVal)
        {
            if (Condition.CondtionExpression == ConditionCheckExpression.EqualOrMore)
            {
                return (targetVal >= Condition.Value);

            }
            else if (Condition.CondtionExpression == ConditionCheckExpression.EqualOrLess)
            {
                return (targetVal <= Condition.Value);

            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public bool Truly(Unit chkUnit=null)
        {
            switch (chkUnit)
            {
                case null when IsTargetCheck:
                    return false;
                case null:
                    chkUnit = Parent;
                    break;
            }

            bool res = Condition.CondtionParam switch
            {
                ConditionCheckParam.SPD => CheckExpression(chkUnit.Stats.Speed),
                ConditionCheckParam.CritRate => CheckExpression(chkUnit.Stats.CritRatePrc),
                ConditionCheckParam.HPPrc => chkUnit.Stats.MaxHp != 0 &&
                                             CheckExpression(chkUnit.GetRes(Resource.ResourceType.HP).ResVal /
                                                             chkUnit.Stats.MaxHp),
                ConditionCheckParam.Weakness => chkUnit.Fighter.Weaknesses.Any(x => x == Condition.ElemValue),
                _ => throw new NotImplementedException()
            };
            return res;

        }




        public record ConditionRec
        {
            public ConditionCheckParam CondtionParam;
            public ConditionCheckExpression CondtionExpression;
            public double Value;
            public Unit.ElementEnm ElemValue;
        }


        public enum ConditionCheckParam
        {
            SPD,
            Weakness,
            CritRate,
            HPPrc

        }

        public enum ConditionCheckExpression
        {
            EqualOrMore,
            EqualOrLess
        }


        public ConditionMod(Unit parentUnit) : base(parentUnit)
        {
          
        }
    }
}
