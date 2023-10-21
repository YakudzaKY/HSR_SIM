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
 

        //reset values in mich dictionary
        public void Reset()
        {
            foreach (var item in Values)
            {
               
                
                Values[item.Key] = 0;
              
            }
        }




        public Dictionary<Ability, double> Values { get; }= new Dictionary<Ability, double>();

    
        
        public void AddVal(Ability ability)
        {
             Values.Add(ability,0);
        }

        
    }
}
