using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.Fighters.FighterUtils;

namespace HSR_SIM_LIB.Fighters
{
    /// <summary>
    /// default npc fighter logics
    /// </summary>
    public class DefaultNPCFighter : IFighter
    {
 
        public PathType? Path { get; set; } = null;
        public Unit.ElementEnm Element { get; set; }
        public List<Unit.ElementEnm> Weaknesses { get; set; } = new List<Unit.ElementEnm>();
        public List<Resist> Resists { get; set; } = new List<Resist>();
        public Unit Parent { get; set; }
        public IEnumerable<Unit> GetAoeTargets()
        {
            return Parent.Enemies.Where(x => x.IsAlive);
        }
        public Ability ChooseAbilityToCast(Step step)
        {
            throw new NotImplementedException();
        }

        public string GetSpecialText()
        {
            return null;
        }


        public IFighter.EventHandler EventHandlerProc { get; set; }
        public IFighter.StepHandler StepHandlerProc { get; set; }
        public List<Ability> Abilities { get; set; } = new List<Ability>();


        public DefaultNPCFighter(Unit parent)
        {
            Parent = parent;

            EventHandlerProc += DefaultFighter_HandleEvent;
            StepHandlerProc += DefaultFighter_HandleStep;

        }

        public virtual void DefaultFighter_HandleEvent(Event ent)
        {
         
        }
        public virtual void DefaultFighter_HandleStep(Step step)
        {
           
        }

    }
}
