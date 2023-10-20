using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.Skills
{/// <summary>
/// Ability class
/// </summary>
    public class Ability : CloneClass
    {
        public ElementEnm? Element { get; set; }

        public AbilityTypeEnm AbilityType { get; set; }

        public Unit Parent { get; set; }
        public delegate double? DCalculateToughnessShred(Unit target);
        public DCalculateToughnessShred CalculateToughnessShred { get; init; }
        public double ToughnessShred;
        public string Name { get; internal set; }
        public List<Event> Events { get; set; } = new List<Event>();
        public short Cost { get; set; } = 0;
        public ResourceType CostType { get; set; } = ResourceType.nil;
        public TargetTypeEnm? TargetType { get; set; }
        public bool Attack { get; set; }
        public int EnergyGain { get; set; }

        public Ability(Unit parent)
        {
            Parent = parent;
        }


        public enum AbilityTypeEnm
        {
            Basic,
            Ability,
            Ultimate,
            FolowUpAttack,
            Technique,
            FollowUpAction//Add priority Hight Medium Low . Ultimate can use before low. Blade shuhu medium, Locha heal - hight. Kafka-hight, Locha aura - low
        }

        public enum TargetTypeEnm
        {
            Self,
            Target,
            Hostiles,
            Party,
            Blast
        }
    }
}
