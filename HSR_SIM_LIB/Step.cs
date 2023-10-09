using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.Event;
using static HSR_SIM_LIB.Resource;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Step-time simulator unit
    /// </summary>
    public class Step
    {
        private StepTypeEnm stepType;
        private Unit actor; //who the actor
        private Ability actorAbility;
        private SimCls parent;

        public StepTypeEnm StepType { get => stepType; set => stepType = value; }
        public Unit Actor { get => actor; set => actor = value; }
        public Ability ActorAbility { get => actorAbility; set => actorAbility = value; }
        public List<Event> Events { get => events; set => events = value; }

        private List<Event> events = new List<Event>();

        /// <summary>
        /// Get text description of step
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public string GetStepDescription()
        {
            string res;
            if (StepType == StepTypeEnm.SimInit)
                res = "sim was initialized";
            else if (StepType == StepTypeEnm.TechniqueUse)
                res = Actor.Name + " used " + ActorAbility.Name;
            else if (StepType == StepTypeEnm.StartCombat)
                res = "Starting the combat!";
            else if (StepType == StepTypeEnm.StartWave)
                res = "Wave " + parent.CurrentFight.CurrentWaveCnt.ToString();
            else if (StepType == StepTypeEnm.Iddle)
                res = "Iddle step(scenario completed?)";
            else
                throw new NotImplementedException();
            return res;

        }

        public Step(SimCls parent)
        {
            StepType = StepTypeEnm.Iddle;
            this.parent = parent;
        }
        //Step have events
        public enum StepTypeEnm
        {
             SimInit//on scenario load and combat init
            ,Iddle//on Iddle, nothing to_do
            ,TechniqueUse
            ,StartCombat//on fight starts
            ,StartWave//on wave starts
        }

        /// <summary>
        /// Proc all events in step. No random here or smart thinking. Do it on DoSomething...
        /// </summary>
        public void ProcEvents( bool revert = false)
        {
            //for all events saved in step
            List<Event> events = new List<Event>();            
            events.AddRange(Events);
            //rollback changes from the end of list
            if (revert)
                events.Reverse();

            foreach (Event ent in events)
            {
                if (ent.Type == EventType.PartyResourceDrain)//SP or technical points
                {
                    parent.GetRes(ent.ResType).ResVal -= revert ? -1 : 1;
                }
                else if (ent.Type == EventType.CombatStartSkillQueue)//party buffs or opening
                    if (!revert)
                        parent.BeforeStartQueue.Add(ActorAbility);
                    else
                        parent.BeforeStartQueue.Remove(ActorAbility);
                else if (ent.Type == EventType.EnterCombat)//entering combat
                    parent.DoEnterCombat = !revert;
                else if (ent.Type == EventType.StartCombat)//Loading combat
                {
                    if (!revert)
                    {
                        parent.DoEnterCombat = false;
                        parent.CurrentFight = new CombatFight(parent.CurrentScenario.Fights[parent.CurrentFightStep]);
                        parent.CurrentFightStep += 1;
                    }
                    else
                    {
                        parent.DoEnterCombat = true;
                        parent.CurrentFight = null;
                        parent.CurrentFightStep -= 1;
                    }

                }
                else if (ent.Type == EventType.StartWave)//Loading wave
                {
                    if (!revert)
                    {
                        parent.CurrentFight.CurrentWaveCnt += 1;
                        parent.CurrentFight.CurrentWave = parent.CurrentFight.ReferenceFight.Waves[parent.CurrentFight.CurrentWaveCnt-1];                        
                        parent.HostileParty = parent.getCombatUnits(parent.CurrentFight.CurrentWave.Units);                        
                        
                    }
                    else
                    {                    
                        parent.HostileParty.Clear();
                        parent.CurrentFight.CurrentWaveCnt -= 1;
                        if (parent.CurrentFight.CurrentWaveCnt>0)
                            parent.CurrentFight.CurrentWave = parent.CurrentFight.ReferenceFight.Waves[parent.CurrentFight.CurrentWaveCnt - 1];
                   
                    }

                }
                else
                    throw new NotImplementedException();
            }
            events.Clear();
            events = null;
        }
        /// <summary>
        /// Party have res to cast abilitiy
        /// if res not found then false
        /// </summary>
        /// <returns></returns>
        private bool PartyHaveRes(Ability ability)
        {
            foreach (Resource res in parent.Resources)
            {
                if (res.ResType == ability.CostType)
                    return res.ResVal >= ability.Cost;
            }
            return false;
        }

        //Cast all techniques before fights starts
        public void TechniqueWork()
        {
            List<Ability> abilities = new List<Ability>();
            //gather all abilities from party alive members
            foreach (Unit unit in parent.Party.Where(partyMember => partyMember.IsAlive))
            {
                abilities.AddRange(unit.Abilities.Where(ability => ability.AbilityType == Ability.AbilityTypeEnm.Technique));
            }

            var orderedAbilities = from ability in abilities
                                   where PartyHaveRes(ability)
                                   orderby ability.Events.Exists(ent => ent.Type == EventType.EnterCombat) ascending//non combat first
                                       , ability.Cost descending//start from Hight cost abilities
                                   select ability;
            foreach (Ability ability in orderedAbilities)
            {
                //If conditions are ok then cast ability
                if (parent.FullFiledConditions(ability))
                {
                    ExecuteTechnique(ability);
                    return;
                }
            }
            abilities.Clear();
            abilities = null;
            orderedAbilities = null;          

        }

        /// <summary>
        /// Execute the technique
        /// </summary>
        public void ExecuteTechnique( Ability ability)
        {
            Events.AddRange(ability.Events);//copy events from ability reference
            if (ability.CostType != ResourceType.nil)
                Events.Add(new Event() { Type = EventType.PartyResourceDrain, ResType = ability.CostType, ResourceValue = ability.Cost });//energy drain
            Actor = ability.Parent;//WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.Parent but can change in future)
            ActorAbility = ability;//WAT ABILITY is casting
            StepType = StepTypeEnm.TechniqueUse;
        }

        /// <summary>
        /// load battle step activation
        /// </summary>
        /// <param name="step"></param>
        public void LoadBattleWork()
        {
            if (parent.CurrentFight == null)
            {
                Events.Add(new Event() { Type = EventType.StartCombat });
                StepType = StepTypeEnm.StartCombat;
            }

        }
    }
}
