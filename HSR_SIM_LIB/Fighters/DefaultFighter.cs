using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.CallBacks;
using static HSR_SIM_LIB.Fighters.FighterUtils;

namespace HSR_SIM_LIB.Fighters
{
    /// <summary>
    /// default fighter logics
    /// </summary>
    public class DefaultFighter : IFighter
    {
        public Unit.ElementEnm? Element { get; set; }
        public List<Unit.ElementEnm> Weaknesses { get; set; } = null;
        public List<Resist> Resists { get; set; } = new List<Resist>();
        public Unit Parent { get; set; }

        //all alive enemies
        public IEnumerable<Unit> GetAoeTargets(Event ent)
        {
            return Parent.Enemies.Where(x => x.IsAlive);
        }

        //enemies with weakness to ability
        public IEnumerable<Unit> GetWeaknessTargets(Event ent)
        {
            return Parent.Enemies.Where(x => x.IsAlive && x.Fighter.Weaknesses.Any(x => x == ent.AbilityValue.Element));
        }
        //enemies with weakness to caster
        public IEnumerable<Unit> GetWeaknessTargets(Unit attackerUnit)
        {
            return Parent.Enemies.Where(x => x.IsAlive && x.Fighter.Weaknesses.Any(x => x == attackerUnit.Fighter.Element));
        }
        //alive friends
        public IEnumerable<Unit> GetFriends(Event ent)
        {
            return Parent.Friends.Where(x => x.IsAlive);
        }


        //Try to choose some good ability to cast
        public virtual Ability ChooseAbilityToCast(Step step)
        {
            //Technique before fight
            if (step.Parent.CurrentFight == null)
            {
                //sort by combat then cost. avalable for casting by cost
                foreach (Ability ability in Abilities
                            .Where(x => x.AbilityType == Ability.AbilityTypeEnm.Technique && x.Parent.ParentTeam.GetRes(Resource.ResourceType.TP).ResVal >= x.Cost)
                            .OrderBy(x => x.Events.Any(y => y.Type == Event.EventType.EnterCombat))
                            .ThenByDescending(x => x.Cost))
                {

                    //enter combat skills
                    if (ability.Events.Any(y => y.Type == Event.EventType.EnterCombat))
                    {
                        /*
                         *Cast ability if ability not in qeueu
                         * and we can penetrate weakness
                         * or others cant do that.
                         * So basic opener through weakness>>  combat technique that not penetrate
                         */
                        if (Parent.ParentTeam.ParentSim.BeforeStartQueue.IndexOf(ability) ==
                            -1 //check for existing in queue 
                            && (GetWeaknessTargets(Parent).Any() //We can penetrate shield 
                                || !GetFriends(null).Any(x => x != Parent
                                                           && GetWeaknessTargets(x).Any()
                                                           && x.Fighter.Abilities.Any(y =>
                                                               y.AbilityType == Ability.AbilityTypeEnm.Technique
                                                               && y.Events.Any(y => y.Type == Event.EventType.EnterCombat))) //or others cant penetrate 
                                )
                            && !(Parent.ParentTeam.GetRes(Resource.ResourceType.TP).ResVal >= ability.Cost + 1
                                    && GetFriends(null).Any(x => x.Fighter.Abilities.Any(y => y.AbilityType == Ability.AbilityTypeEnm.Technique && !y.Events.Any(y => y.Type == Event.EventType.EnterCombat) && x.ParentTeam.ParentSim.BeforeStartQueue.IndexOf(y) < 0)))// no unused buffers here when 2tp+
                            )
                        {
                            return ability;
                        }
                    
                    }
                    else
                    {
                        //if no skill in queue
                        if (Parent.ParentTeam.ParentSim.BeforeStartQueue.All(x => x != ability))
                        {
                            /*if have 2+ tp or
                             we have NOT friend who can penetrate weakness through  cost=1 ability
                            */
                            if (Parent.ParentTeam.GetRes(Resource.ResourceType.TP).ResVal >= ability.Cost + 1
                               || !GetFriends(null).Any(x =>GetWeaknessTargets(x).Any()&& x.Fighter.Abilities.Any(y =>
                                   y.AbilityType == Ability.AbilityTypeEnm.Technique && y.Cost > 0
                                   && y.Events.Any(y => y.Type == Event.EventType.EnterCombat)))
                               )
                                return ability;
                        }

                    }
                }
            }

            return null;
        }

        public virtual string GetSpecialText()
        {
            return null;
        }


        public IFighter.EventHandler EventHandlerProc { get; set; }
        public List<Ability> Abilities { get; set; } = new List<Ability>();

        //Blade constructor
        public DefaultFighter(Unit parent)
        {
            Parent = parent;
            EventHandlerProc += new IFighter.EventHandler(DefaultFighter_HandleEvent);

        }

        private void DefaultFighter_HandleEvent(Event ent)
        {
            throw new NotImplementedException();
        }
    }
}
