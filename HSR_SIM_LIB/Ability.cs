using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{/// <summary>
/// Ability class
/// </summary>
    public class Ability
    {

        private AbilityTypeEnm abilityType;
        private Unit parent;

        public AbilityTypeEnm AbilityType { get => abilityType; set => abilityType = value; }
        public Unit Parent { get => parent; set => parent = value; }

        // public AbilityParameters AbilityParams { get => abilityParams; set => abilityParams = value; }
        public string Name { get; internal set; }
        public List<Event> Events { get => events; set => events = value; }

        private List<Event> events = new List<Event>();

        public Ability(Unit parent) 
        { 
            Parent= parent; 
        }
      

        public enum AbilityTypeEnm
        {
            Basic,
            Skill,
            Ultimate,
            Talent,
            Technique
        }
    }
}
