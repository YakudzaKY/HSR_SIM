using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB.Fighters.LightCones
{
    public interface ILightCone
    {
        public delegate void EventHandler(Event ent);
        public delegate void StepHandler(Step step);
        public EventHandler EventHandlerProc{ get; set; }
        public StepHandler StepHandlerProc{ get; set; }
        
    }
}
