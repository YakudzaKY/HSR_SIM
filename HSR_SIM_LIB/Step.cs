using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Step-time simulator unit
    /// </summary>
    public class Step
    {
        private StepTypeEnm stepType;
        private Unit actor; //who the actor
        private Ability actorAbility;

        public StepTypeEnm StepType { get => stepType; set => stepType = value; }
        public Unit Actor { get => actor; set => actor = value; }
        public Ability ActorAbility { get => actorAbility; set => actorAbility = value; }
        public List<Event> Events { get => events; set => events = value; }

        private List<Event> events = new List<Event>();
        


        public Step()
        {
            StepType = StepTypeEnm.Iddle;
        }
        //Step have events
        public enum StepTypeEnm
        {
             SimInit//on scenario load and combat init
            ,Iddle//on Iddle, nothing to_do
            ,TechniqueUse
        }
    }
}
