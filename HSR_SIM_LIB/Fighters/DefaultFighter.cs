using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters.LightCones;
using HSR_SIM_LIB.Fighters.Relics;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Utils.CallBacks;
using static HSR_SIM_LIB.Fighters.FighterUtils;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.Fighters
{
    /// <summary>
    /// default fighter logics
    /// </summary>
    public class DefaultFighter : IFighter
    {
        public virtual PathType? Path { get; set; } = null;
        public Unit.ElementEnm Element { get; set; }
        public List<Unit.ElementEnm> Weaknesses { get; set; } = null;
        public List<Resist> Resists { get; set; } = new List<Resist>();
        public Unit Parent { get; set; }

        private ILightCone lightCone = null;
        public ILightCone LightCone
        {
            get =>
                lightCone ??= ((ILightCone)Activator.CreateInstance(Type.GetType(Parent.LightConeStringPath)!, this,Parent.LightConeInitRank));
            set => lightCone = value;
        }

        public List<IRelicSet> relics;

        public List<IRelicSet> Relics
        {
            get
            {
                if (relics == null)
                {
                    relics = new List<IRelicSet>();
                    foreach (var keyValrelic in Parent.RelicsClasses)
                    {
                        IRelicSet relicSet = (IRelicSet)Activator.CreateInstance(Type.GetType(keyValrelic.Key)!, this);

                        relicSet.num = keyValrelic.Value;
                        relics.Add(relicSet);

                    }
                }

                return relics;
            }
            set
            {

            }
        }



 

       

        public List<Skill> Skills { get; set; }=new List<Skill>();

        //all alive enemies
        public IEnumerable<Unit> GetAoeTargets()
        {
            return Parent.Enemies.Where(x => x.IsAlive);
        }

        //enemies with weakness to ability and without shield
        public IEnumerable<Unit> GetWeaknessTargets()
        {
            return Parent.Enemies.Where(x => x.IsAlive
                                             && x.GetRes(Resource.ResourceType.Barrier).ResVal==0
                                             && x.Fighter.Weaknesses.Any(x => x == Parent.Fighter.Element));
        }

        //alive friends
        public IEnumerable<Unit> GetFriends()
        {
            return Parent.Friends.Where(x => x.IsAlive);
        }

        public static double? CalculateOpeningThg(Unit target)
        {
            return target.GetRes(Resource.ResourceType.Toughness).ResVal;
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
                            .OrderBy(x => x.Attack)
                            .ThenByDescending(x => x.Cost))
                {

                    //enter combat skills
                    if (ability.Attack)
                    {
                        /*
                         *Cast ability if ability not in qeueu
                         * and we can penetrate weakness
                         * or others cant do that.
                         * So basic opener through weakness>>  combat technique that not penetrate
                         */
                        if (Parent.ParentTeam.ParentSim.BeforeStartQueue.IndexOf(ability) ==
                            -1 //check for existing in queue 
                            && (GetWeaknessTargets().Any() //We can penetrate shield 
                                || !GetFriends().Any(x => x != Parent
                                                           && GetWeaknessTargets().Any()
                                                           && x.Fighter.Abilities.Any(y =>
                                                               y.AbilityType == Ability.AbilityTypeEnm.Technique
                                                               && y.Attack)) //or others cant penetrate 
                                )
                            && !(Parent.ParentTeam.GetRes(Resource.ResourceType.TP).ResVal >= ability.Cost + 1
                                    && GetFriends().Any(x => x.Fighter.Abilities.Any(y => y.AbilityType == Ability.AbilityTypeEnm.Technique && !y.Attack && x.ParentTeam.ParentSim.BeforeStartQueue.IndexOf(y) < 0)))// no unused buffers here when 2tp+
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
                               || !GetFriends().Any(x =>GetWeaknessTargets().Any()&& x.Fighter.Abilities.Any(y =>
                                   y.AbilityType == Ability.AbilityTypeEnm.Technique && y.Cost > 0
                                   && y.Attack))
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
        public IFighter.StepHandler StepHandlerProc { get; set; }
        public List<Ability> Abilities { get; set; } = new List<Ability>();

        //Blade constructor
        public DefaultFighter(Unit parent)
        {
            Parent = parent;
            EventHandlerProc += DefaultFighter_HandleEvent;
            StepHandlerProc += DefaultFighter_HandleStep;

        }

        public virtual void DefaultFighter_HandleEvent(Event ent)
        {
            
            LightCone?.EventHandlerProc.Invoke(ent);
            foreach (IRelicSet relic in Relics)
            {
                relic.EventHandlerProc.Invoke(ent);
            }
            
        }
        public virtual  void DefaultFighter_HandleStep(Step step)
        {
            LightCone?.StepHandlerProc.Invoke(step);
            foreach (IRelicSet relic in Relics)
            {
                relic.StepHandlerProc.Invoke(step);
            }
        }
    }
}
