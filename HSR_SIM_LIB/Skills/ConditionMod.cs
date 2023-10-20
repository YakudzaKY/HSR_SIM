using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills
{
    public  class ConditionMod:PassiveMod
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

        public bool Truly
        {
            get
            {
                bool res;
                switch (Condition.CondtionParam)
                {
                    case ConditionCheckParam.SPD:
                        res = CheckExpression(Parent.Stats.Speed);
                        break;
                    case ConditionCheckParam.CritRate:
                        res = CheckExpression(Parent.Stats.CritRatePrc);
                        break;
                    case ConditionCheckParam.HPPrc:
                        res = Parent.Stats.MaxHp != 0 && CheckExpression(Parent.GetRes(Resource.ResourceType.HP).ResVal/Parent.Stats.MaxHp);
                        break;
                    default:
                        throw new NotImplementedException();
                        break;
                }
                return res;
            }
        }

        public record ConditionRec
        {
            public ConditionCheckParam CondtionParam;
            public ConditionCheckExpression CondtionExpression;
            public double Value;
        }


        public enum ConditionCheckParam
        {
            SPD,
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
