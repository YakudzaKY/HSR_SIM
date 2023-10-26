using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.TurnBasedClasses.Event;
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

        public ElementEnm Element { get; set; }

        public AbilityTypeEnm AbilityType { get; set; }

        public IFighter Parent { get; set; }
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
        public bool EndTheTurn { get; set; } = true;//If ability used - end the turn
        public delegate bool DCanUsePrc(Step step);
        public DCanUsePrc CanUsePrc { get; init; }
        public Ability(IFighter parent)
        {
            Parent = parent;
            Element = parent.Element;
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
                    res=  new[]{Parent.Parent};
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
                     res =Parent.Parent.GetTargetsForUnit(eventTargetType);
                    else
                        throw new NotImplementedException();
                }
                else 
                    throw new NotImplementedException();
            }
  
            return res;
        }

        public Unit GetBestTarget()
        {
            if (TargetType == TargetTypeEnm.Self)
                return Parent.Parent;
            else if (TargetType == TargetTypeEnm.Enemy)
            {
                /*
                 * MainDD,SecondDD focus on massive dmg, pref max weakness targets
                 * Others- shield breaking
                 *
                 */

                if (AdjacentTargets==AdjacentTargetsEnm.None)
                {

                    return Parent.Parent.Enemies.Where(x => x.IsAlive).OrderByDescending(x=>x.Fighter.Weaknesses.Contains(Element)).MinBy(x=>x.GetRes(ResourceType.HP).ResVal);

                }
                else
                {
                    throw new NotImplementedException(); 
                }
            }
            else
            {
                throw new NotImplementedException();  
            }

            return null;
        }
    }
}
