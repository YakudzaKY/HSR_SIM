using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.TurnBasedClasses.Events.Event;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.Skills
{/// <summary>
/// Ability class
/// </summary>
    public class Ability : CloneClass
    {


        public ElementEnm Element { get; set; }

        public AbilityTypeEnm AbilityType { get; set; }

        public IFighter Parent { get; set; }

        public string Name { get; internal set; }
        public List<Event> Events { get; set; } = new List<Event>();
        public double Cost { get; set; } = 0;
        public ResourceType? CostType { get; set; }
        public TargetTypeEnm TargetType { get; set; } = TargetTypeEnm.Enemy;
        public AdjacentTargetsEnm AdjacentTargets { get; set; } = AdjacentTargetsEnm.None;

        public bool Attack { get; set; }
        public double EnergyGain { get; set; }
        public bool IgnoreWeakness { get; set; }

        public int Cooldown { get; set; }
        public bool EndTheTurn { get; set; } = true;//If ability used - end the turn
        public delegate bool DCanUsePrc();

        public PriorityEnm Priority { get; set; } = PriorityEnm.Low;

        public DCanUsePrc Available { get; init; } = DefaultAbilityAvailable;
        public int SpGain { get; set; } = 0;

        public Ability(IFighter parent)
        {
            Parent = parent;
            if (Element == ElementEnm.None)
                Element = parent.Element;
        }

        public enum PriorityEnm
        {
            High,
            Medium,
            Ultimate,
            Low,
        }
      
        public enum AbilityTypeEnm
        {
            None = 0,
            Basic = 1,
            Ability = 2,
            Ultimate = 4,
            Technique = 8,
            FollowUpAction = 16
        }


        //default all abilities are ok
        public static bool DefaultAbilityAvailable()
        {
            return true;
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

            if (eventTargetType == null)
                eventTargetType = TargetType;
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
                    res = new[] { Parent.Parent };
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
                    if (eventTargetType == TargetTypeEnm.Friend)
                        res = GetAffectedTargets( Parent.Parent, true);
                    else
                        res = GetAffectedTargets(target, true);

                }
                else
                    throw new NotImplementedException();
            }

            return res;
        }

        /// <summary>
        /// Get units affected by ability
        /// </summary>
        /// <returns></returns>
        public List<Unit> GetAffectedTargets(Unit unit, bool onlyAdjacent = false)
        {
            List<Unit> res = new List<Unit>();
            if (AdjacentTargets == AdjacentTargetsEnm.Blast)
            {
                if (!onlyAdjacent)
                    res.Add(unit);
                List<Unit> party = new List<Unit>();
                party.AddRange(unit.GetTargetsForUnit(TargetTypeEnm.Friend));
                var leftTarget = party.LastOrDefault(x => party.IndexOf(x) < party.IndexOf(unit));
                if (leftTarget != null)
                    res.Add(leftTarget);
                var rightTarget = party.FirstOrDefault(x => party.IndexOf(x) > party.IndexOf(unit));
                if (rightTarget != null)
                    res.Add(rightTarget);
            }
            else if (AdjacentTargets == AdjacentTargetsEnm.One)
            {
                if (!onlyAdjacent)
                    res.Add(unit);
                List<Unit> party = new List<Unit>();
                party.AddRange(unit.GetTargetsForUnit(TargetTypeEnm.Friend));
                var leftTarget = party.LastOrDefault(x => party.IndexOf(x) < party.IndexOf(unit));
                if (leftTarget != null)
                    res.Add(leftTarget);
                else
                {
                    var rightTarget = party.FirstOrDefault(x => party.IndexOf(x) > party.IndexOf(unit));
                    if (rightTarget != null)
                        res.Add(rightTarget);
                }
            }
            else if (AdjacentTargets == AdjacentTargetsEnm.All)
            {
                if (unit!=null)
                    res.AddRange(Parent.Parent.GetTargetsForUnit(TargetTypeEnm.Friend));
                else
                {
                    res.AddRange(Parent.Parent.GetTargetsForUnit(TargetTypeEnm.Enemy));
                }
            }
            else
                throw new NotImplementedException();
            return res;
        }

        public Unit GetBestTarget()
        {
            Unit leader = Parent.Parent.ParentTeam.Units.FirstOrDefault(x => x.Fighter.Role == FighterUtils.UnitRole.MainDPS);
            if (TargetType == TargetTypeEnm.Self)
                return Parent.Parent;
            else if (TargetType == TargetTypeEnm.Enemy)
            {
                /*
                 * MainDD,SecondDD focus on massive dmg, pref max weakness targets
                 * Others- shield breaking
                 *
                 */

                if (AdjacentTargets == AdjacentTargetsEnm.None)
                {
                    //Support,Healer focus on Shield shred. 
                    if (Parent.Role is FighterUtils.UnitRole.Support or FighterUtils.UnitRole.Healer or FighterUtils.UnitRole.SecondDPS)
                        return Parent.Parent.GetTargetsForUnit(TargetType).OrderByDescending(x => x.Fighter.Weaknesses.Contains(Element)).ThenBy(x => x.GetRes(ResourceType.Toughness).ResVal * (leader?.Fighter.Path is FighterUtils.PathType.Destruction or FighterUtils.PathType.Erudition ? -1 : 1)).ThenBy(x => x.GetRes(ResourceType.HP).ResVal * (leader?.Fighter.Path is FighterUtils.PathType.Destruction or FighterUtils.PathType.Erudition ? -1 : 1)).FirstOrDefault();

                    else
                        // focus on High hp if main dps Destruction,Erudition. Other- low hp
                        return Parent.Parent.GetTargetsForUnit(TargetType).OrderByDescending(x => x.Fighter.Weaknesses.Contains(Element)).ThenBy(x => x.GetRes(ResourceType.HP).ResVal * (leader?.Fighter.Path is FighterUtils.PathType.Destruction or FighterUtils.PathType.Erudition ? -1 : 1)).FirstOrDefault();

                }
                else if (AdjacentTargets == AdjacentTargetsEnm.Blast)
                {
                    Unit bestTarget = null;
                    double bestScore = -1;
                    foreach (Unit unit in Parent.Parent.GetTargetsForUnit(TargetType))
                    {
                        double score = 0;
                        List<Unit> targets = GetAffectedTargets(unit);
                        score = 10 * targets.Count;
                        score += 5 * targets.Count(x =>x==unit&& x.GetRes(ResourceType.Toughness).ResVal == 0);
                        score += 3 * targets.Count(x => x==unit&&x.Fighter.Weaknesses.Any(x=>x==Element));
                        score += 2 * targets.Count(x => x.Fighter is DefaultNPCBossFIghter);
                        //if equal but hp diff go focus big target
                        if ((score > bestScore)||(score==bestScore&&unit.GetHpPrc(null)>bestTarget.GetHpPrc(null)))
                        {

                            bestTarget = unit;
                            bestScore = score;
                        }

                    }
                    return bestTarget;
                }
                if (AdjacentTargets == AdjacentTargetsEnm.All)
                {
                    
                    return null;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (TargetType == TargetTypeEnm.Friend)
            {
                return Parent.Parent.Friends.Where(x => x.IsAlive).OrderBy(x => x.Fighter.Role).First();
            }
            else
            {
                throw new NotImplementedException();
            }

            return null;
        }

        public enum WeaknessBreakOrderEnm
        {
            ToughnessFirst,
            DirectDamageFirst
        }
    }
}
