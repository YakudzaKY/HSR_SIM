using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.Fighters
{
    /// <summary>
    /// dictionary with mechancis
    /// </summary>
    public class MechDictionary
    {
        public record RMechCounter
        {
            public MechCounterTypeEnm Type;
            public int IntValue;
            public double DoubleValue;
            public string StringValue;

        }

        //reset values in mich dictionary
        public void Reset()
        {
            foreach (KeyValuePair<Ability, RMechCounter> item in values)
            {
                if (item.Value.Type == MechCounterTypeEnm.StringType)
                {
                    item.Value.StringValue = "";
                }
                else
                    throw new Exception("Unknown type: "+item.Value.Type.ToString());
            }
        }

        public enum MechCounterTypeEnm
        {
            IntType,
            DoubleType,
            StringType
        }


        private Dictionary<Ability, RMechCounter> values;

        public MechDictionary()
        {
            values=new Dictionary<Ability, RMechCounter>();
        }
        
        public void AddVal(Ability ability,MechCounterTypeEnm type)
        {
             values.Add(ability,new RMechCounter(){Type = type});
        }

        //get value by type
        public double GetDVal(Ability ability)
        {
            return values[ability].DoubleValue;
        }

        public int GetIVal(Ability ability)
        {
            return values[ability].IntValue;
        }

        public string GetSVal(Ability ability)
        {
            return values[ability].StringValue;
        }

        public void SetVal(Ability ability,string value)
        {
            values[ability].StringValue=value;
        }

        public void SetVal(Ability ability, double value)
        {
            values[ability].DoubleValue = value;
        }

        public void SetVal(Ability ability,int value)
        {
            values[ability].IntValue=value;
        }

    }
}
