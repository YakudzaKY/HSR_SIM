using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters.LightCones;
using HSR_SIM_LIB.Fighters.Relics;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Utils.CallBacks;
using static HSR_SIM_LIB.Fighters.FighterUtils;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.Skills;
using static HSR_SIM_LIB.Fighters.IFighter;

namespace HSR_SIM_LIB.Fighters
{
    /// <summary>
    /// default fighter logics
    /// </summary>
    public class DefaultFighter : IFighter
    {
        public List<ConditionMod> ConditionMods { get; set; } = new List<ConditionMod>();
        public List<PassiveMod> PassiveMods { get; set; } = new List<PassiveMod>();
        public virtual PathType? Path { get; set; } = null;
        public Unit.ElementEnm Element { get; set; }
        public List<Unit.ElementEnm> Weaknesses { get; set; } = null;
        public List<DebuffResist> DebuffResists { get; set; }
        public List<Resist> Resists { get; set; } = new List<Resist>();
        public Unit Parent { get; set; }
        public MechDictionary Mechanics;
        public ATracesEnm Atraces { get; set; }
        [Flags]
        public enum ATracesEnm
        {
            A2 = 1,
            A4 = 2,
            A6 = 4

        }

        private ILightCone lightCone = null;

        public ILightCone LightCone
        {
            get
            {
                if (!String.IsNullOrEmpty(Parent.LightConeStringPath))
                    lightCone ??=
                ((ILightCone)Activator.CreateInstance(Type.GetType(Parent.LightConeStringPath)!, this, Parent.
                LightConeInitRank));
                return lightCone;
            }

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
                        IRelicSet relicSet = (IRelicSet)Activator.CreateInstance(Type.GetType(keyValrelic.Key)!, this, keyValrelic.Value);
                        relics.Add(relicSet);

                    }
                }

                return relics;
            }
            set
            {

            }
        }







        public List<Skill> Skills { get; set; } = new List<Skill>();

        //all alive enemies
        public IEnumerable<Unit> GetAoeTargets()
        {
            return Parent.Enemies?.Where(x => x.IsAlive);
        }

        //enemies with weakness to ability and without shield
        public IEnumerable<Unit> GetWeaknessTargets()
        {
            return Parent.Enemies.Where(x => x.IsAlive
                                             && x.GetRes(Resource.ResourceType.Barrier).ResVal == 0
                                             && x.Fighter.Weaknesses.Any(x => x == Parent.Fighter.Element));
        }

        //alive friends
        public IEnumerable<Unit> GetFriends()
        {
            return Parent.Friends.Where(x => x.IsAlive);
        }

        
        public double Cost
        {
            get
            {
                int totalCost=0;
                switch (Path)
                {
                    case PathType.Hunt:
                        totalCost = 7 * (GetAoeTargets()?.Count()==1?2:1);//x2 if 1 target on battlefield
                        break;
                    case PathType.Destruction:
                        totalCost = 6 * (GetAoeTargets()?.Count()>=2?2:1);//x2 if 2+ targets on battlefield
                        break;
                    case PathType.Erudition:
                        totalCost = 5 * (GetAoeTargets()?.Count()>=3?3:1);//x3 if 3+ targets on battlefield
                        break;
                    case PathType.Nihility:
                        totalCost = 4;
                        break;
                    case PathType.Harmony:
                        totalCost = 3;
                        break;
                    case PathType.Preservation:
                        totalCost = 2;
                        break;
                    case PathType.Abundance:
                        totalCost = 1;
                        break;
                    default:
                        totalCost = 1;
                        break;

                }
                return totalCost;

            }
        }

        public UnitRole? Role {
            get
            {

                var unitsToSearch = Parent.ParentTeam.Units.Where(x=>x.IsAlive).OrderByDescending(x => x.Fighter.Cost).ThenByDescending(x=>x.Stats.Attack*x.Stats.CritChance*x.Stats.CritDmg).ToList();
                if (Parent == unitsToSearch.First())
                    return UnitRole.MainDPS;
                //if second on list then second dps
                else if (new List<PathType?>() { PathType.Hunt, PathType.Destruction,PathType.Erudition, PathType.Nihility }.Contains(Path)&&Parent==unitsToSearch.ElementAt(1) )
                {
                    return UnitRole.SecondDPS;
                }
                else if (new List<PathType?>() { PathType.Hunt, PathType.Destruction,PathType.Erudition}.Contains(Path)&&Parent==unitsToSearch.ElementAt(2) )
                {
                    return UnitRole.ThirdDPS;
                }
                //healer here
                else if (Path == PathType.Abundance)
                    return UnitRole.Healer;
                else
                    return UnitRole.Support;

            }
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
                         *Cast ability if ability not in queue
                         * and we can penetrate weakness
                         * or others cant do that.
                         * So basic opener through weakness>>  combat technique that not penetrate
                         */
                        if (Parent.ParentTeam.ParentSim.BeforeStartQueue.IndexOf(ability) ==
                            -1 //check for existing in queue 
                            && ((GetWeaknessTargets().Any() || ability.IgnoreWeakness) //We can penetrate shield 
                                || !GetFriends().Any(x => x != Parent
                                                           && ((((DefaultFighter)(x.Fighter)).GetWeaknessTargets().Any()
                                                           && x.Fighter.Abilities.Any(y =>
                                                               y.AbilityType == Ability.AbilityTypeEnm.Technique
                                                               && y.Attack)) || (x.Fighter.Abilities.Any(y =>
                                                               y.AbilityType == Ability.AbilityTypeEnm.Technique
                                                               && y.Attack && y.IgnoreWeakness)))
                                                           ) //or others cant penetrate  or otherc can ignore weaknesss
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
                               || !GetFriends().Any(x => GetWeaknessTargets().Any() && x.Fighter.Abilities.Any(y =>
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

        public virtual void Reset()
        {
            Mechanics.Reset();
            LightCone?.Reset();
            foreach (var relic in Relics)
            {
                relic.Reset();
            }
        }

        //Blade constructor
        public DefaultFighter(Unit parent)
        {
            Mechanics = new MechDictionary();
            Mechanics.Reset();
            Parent = parent;
            EventHandlerProc += DefaultFighter_HandleEvent;
            StepHandlerProc += DefaultFighter_HandleStep;
            //no way to get ascend traces from api :/
            Atraces = (ATracesEnm.A2 | ATracesEnm.A4 | ATracesEnm.A6);

            Ability defOpener;
            //Default Opener
            defOpener = new Ability(Parent)
            {
                AbilityType = Ability.AbilityTypeEnm.Technique
                ,
                Name = "Default opener"
                ,
                Element = Element
                ,
                ToughnessShred = 30
                ,
                AdjacentTargets = Ability.AdjacentTargetsEnm.All
                ,
                Attack = true
            };


            Abilities.Add(defOpener);

        }


        public virtual void DefaultFighter_HandleEvent(Event ent)
        {

            LightCone?.EventHandlerProc.Invoke(ent);
            foreach (IRelicSet relic in Relics)
            {
                relic.EventHandlerProc.Invoke(ent);
            }

        }
        public virtual void DefaultFighter_HandleStep(Step step)
        {
            if (step.StepType==Step.StepTypeEnm.StartCombat ||step.StepType==Step.StepTypeEnm.FinishCombat  )
            {
               Reset();
            }
            LightCone?.StepHandlerProc.Invoke(step);
            foreach (IRelicSet relic in Relics)
            {
                relic.StepHandlerProc.Invoke(step);
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
