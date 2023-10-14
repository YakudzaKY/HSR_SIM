using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static HSR_SIM_LIB.Resource;
using static HSR_SIM_LIB.Unit;

namespace HSR_SIM_LIB
{/// <summary>
/// Ability class
/// </summary>
    public class Ability: CheckEssence
    {
        public ElementEnm? Element { get; set; }

        public AbilityTypeEnm AbilityType { get; set; }

        public Unit Parent { get; set; }

        public string Name { get; internal set; }
        public List<Event> Events { get; set; } = new List<Event>();
        public short Cost { get; set; } = 0;
        public ResourceType CostType { get; set; } = ResourceType.nil;


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
            Technique,
            Trigger
        }

        public enum TargetTypeEnm
        {
            Self,
            Target,
            Hostiles,
            Party,
            AOE,
            Blast
        }
    }
}
