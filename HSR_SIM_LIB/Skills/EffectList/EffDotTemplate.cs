using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;

namespace HSR_SIM_LIB.Skills.EffectList
{
    public abstract class EffDotTemplate : Effect
    {
        public  Event.CalculateValuePrc DoTCalculateValue { get; init; }
        public abstract Unit.ElementEnm Element { get; init; }
    }
}
