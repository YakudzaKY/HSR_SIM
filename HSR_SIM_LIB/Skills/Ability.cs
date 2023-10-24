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
        

        //public Event.CalculateValuePrc CalculateValue { get; init; }
        //public Event.CalculateTargetPrc CalculateTargets { get; init; }

        public ElementEnm? Element { get; set; }

        public AbilityTypeEnm AbilityType { get; set; }

        public Unit Parent { get; set; }
        public delegate double? DCalculateToughnessShred(Unit target);
        public DCalculateToughnessShred CalculateToughnessShred { get; init; }
        public double ToughnessShred;
        public string Name { get; internal set; }
        public List<Event> Events { get; set; } = new List<Event>();
        public short Cost { get; set; } = 0;
        public ResourceType? CostType { get; set; } 
        public TargetTypeEnm TargetType { get; set; } = TargetTypeEnm.Enemy;
        public AdjacentTargetsEnm AdjacentTargets { get; set; } = AdjacentTargetsEnm.None;

        public bool Attack { get; set; }
        public int EnergyGain { get; set; }
        public bool IgnoreWeakness { get; set; }
        public int Cooldown { get; set; }

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
            Friend,
            Enemy,

        }

        public enum AbilityCurrentTargetEnm
        {
            AbilityAdjacent,
            AbilityMain
        }

        public enum AdjacentTargetsEnm
        {
            Random,
            None,
            Blast,
            One,
            All
        }

        //get targets by event from ability
        public IEnumerable<Unit> GetTargets(Unit target, TargetTypeEnm? eventTargetType,
            AbilityCurrentTargetEnm? currTargetType)
        {
            IEnumerable<Unit> res = null;

            if (eventTargetType==null)
                eventTargetType=TargetType;
            if (currTargetType == null)
            {
                if (AdjacentTargets == AdjacentTargetsEnm.All)
                {
                    currTargetType = AbilityCurrentTargetEnm.AbilityAdjacent;
                }
                else
                {
                    currTargetType = AbilityCurrentTargetEnm.AbilityMain;
                }
            }

            if (eventTargetType == TargetTypeEnm.Self)
            {
                if (currTargetType == AbilityCurrentTargetEnm.AbilityMain)
                {
                    res=  new[]{Parent};
                }
                else 
                    throw new NotImplementedException();
            }
            else 
            {
                if (currTargetType == AbilityCurrentTargetEnm.AbilityMain)
                {
                    res = new[] { target };
                }
                else if (currTargetType == AbilityCurrentTargetEnm.AbilityAdjacent)
                {
                    if (AdjacentTargets==AdjacentTargetsEnm.All)
                     res =Parent.GetTargetsForUnit(eventTargetType);
                    else
                        throw new NotImplementedException();
                }
                else 
                    throw new NotImplementedException();
            }
  
            return res;
        }
    }
}
