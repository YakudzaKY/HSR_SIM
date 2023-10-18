using System.Collections.Generic;

namespace HSR_SIM_LIB.Fighters.LightCones
{
    internal class DefaultLightCone:ILightCone
    {
        public ILightCone.EventHandler EventHandlerProc { get; set; }
        public ILightCone.StepHandler StepHandlerProc { get; set; }
        public List<Ability> Abilities { get; set; }

        public DefaultLightCone(IFighter parent)
        {
            Parent = parent;
            EventHandlerProc += DefaultLightCone_HandleEvent;
            StepHandlerProc += DefaultLightCone_HandleStep;

        }

        public IFighter Parent { get; set; }

        public virtual void DefaultLightCone_HandleEvent(Event ent)
        {
            
           
        }
        public virtual  void DefaultLightCone_HandleStep(Step step)
        {
            
            
        }
    }
}
