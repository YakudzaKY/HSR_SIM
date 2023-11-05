using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills
{
    public class Effect
    {
        public Event.CalculateValuePrc CalculateValue { get; init; }

        public double? Value { get; set; }
        public bool StackAffectValue { get; set; } = true;// do we multiply final value by stack count ?
        /// <summary>
        /// on natural (by timer) expire but not expired
        /// </summary>
        public virtual void OnNaturalExpire(Event ent,Buff mod)
        {

        }


        /// <summary>
        /// On expired and dispelled(no buff at this moment)
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void OnExpired(Event ent,Buff mod)
        {
           
        }
    }
}
