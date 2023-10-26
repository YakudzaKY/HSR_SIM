using System.Collections.Generic;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;

namespace HSR_SIM_LIB.Fighters.LightCones
{
    internal abstract class DefaultLightCone:ILightCone
    {
        public List<ConditionMod> ConditionMods { get; set; }=new List<ConditionMod>();
        public List<PassiveMod> PassiveMods { get; set; } = new List<PassiveMod>();
        public int Rank { get; set; }
        public abstract  FighterUtils.PathType Path { get; } 
        public ILightCone.EventHandler EventHandlerProc { get; set; }
        public ILightCone.StepHandler StepHandlerProc { get; set; }
        public List<Ability> Abilities { get; set; }
        public void Reset()
        {
            
        }

        public DefaultLightCone(IFighter parent,int rank)
        {
            Parent = parent;
            Rank = rank;
            if (Path != Parent.Path)//prevent from wrong Path lc
                return;
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

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
