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
        /// After buff applied
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="buff"></param>
        public virtual void OnApply(Event ent, Buff buff)
        {
            
        }

        /// <summary>
        /// Before buff applied. buff.Owner will be null
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="buff"></param>
        public virtual void BeforeApply(Event ent, Buff buff)
        {
            
        }
        /// <summary>
        /// after buff removed. buff.Owner can be null
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="buff"></param>
        public virtual void OnRemove(Event ent, Buff buff)
        {

        }

        /// <summary>
        /// Before buff removed
        /// </summary>
        /// <param name="ent"></param>
        /// <param name="buff"></param>
        public virtual void BeforeRemove(Event ent, Buff buff)
        {
            
        }

    }
}
