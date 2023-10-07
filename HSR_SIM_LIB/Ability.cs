using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.Resource;

namespace HSR_SIM_LIB
{/// <summary>
/// Ability class
/// </summary>
    public class Ability
    {

        private AbilityTypeEnm abilityType;
        private Unit parent;
        private short cost=0;
        private ResourceType costType= ResourceType.nil;

        public AbilityTypeEnm AbilityType { get => abilityType; set => abilityType = value; }
        public Unit Parent { get => parent; set => parent = value; }

        public string Name { get; internal set; }
        public List<Event> Events { get => events; set => events = value; }
        public short Cost { get => cost; set => cost = value; }
        public ResourceType CostType { get => costType; set => costType = value; }
        internal List<Condition> ExecuteWhen { get => executeWhen; set => executeWhen = value; }

        private List<Event> events = new List<Event>();
        private List<Condition> executeWhen = new List<Condition>();

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
