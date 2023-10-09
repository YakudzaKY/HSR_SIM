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
        public SimCls Parent { get; }

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
                res = "Wave " + Parent.CurrentFight.CurrentWaveCnt.ToString();
            else if (StepType == StepTypeEnm.Idle)
                res = "Idle step(scenario completed?)";
            else if (StepType == StepTypeEnm.ExecuteStartQueue)
                res = "Executed " + Actor.Name+ " " + ActorAbility.Name;
            else
                throw new NotImplementedException();
            return res;

        }

        public Step(SimCls parent)
        {
            StepType = StepTypeEnm.Idle;
            Parent = parent;
        }
        //Step have events
        public enum StepTypeEnm
        {
            SimInit//on scenario load and combat init
            , Idle//on Idle, nothing to_do
            , TechniqueUse
            , StartCombat//on fight starts
            , StartWave//on wave starts
            , ExecuteStartQueue
        }

        /// <summary>
        /// Proc all events in step. No random here or smart thinking. Do it on DoSomething...
        /// </summary>
        public void ProcEvents(bool revert = false)
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
                    Parent.GetRes(ent.ResType).ResVal -= revert ? -1 : 1;
                }
                else if (ent.Type == EventType.CombatStartSkillQueue)//party buffs or opening
                    if (!revert)
                        Parent.BeforeStartQueue.Add(ActorAbility);
                    else
                        Parent.BeforeStartQueue.Remove(ActorAbility);
                else if (ent.Type == EventType.CombatStartSkillDeQueue)//DEQUEUE party buffs or opening
                    if (!revert)
                        Parent.BeforeStartQueue.Remove(ActorAbility);
                    else
                        Parent.BeforeStartQueue.Add(ActorAbility);
                else if (ent.Type == EventType.EnterCombat)//entering combat
                    Parent.DoEnterCombat = !revert;
                else if (ent.Type == EventType.StartCombat)//Loading combat
                {
                    if (!revert)
                    {
                        Parent.DoEnterCombat = false;
                        Parent.CurrentFight = new CombatFight(Parent.CurrentScenario.Fights[Parent.CurrentFightStep]);
                        Parent.CurrentFightStep += 1;
                    }
                    else
                    {
                        Parent.DoEnterCombat = true;
                        Parent.CurrentFight = null;
                        Parent.CurrentFightStep -= 1;
                    }

                }
                else if (ent.Type == EventType.ModActionValue) //Loading wave
                {
                    throw new NotImplementedException();
                }
                else if (ent.Type == EventType.StartWave)//Loading wave
                {
                    if (!revert)
                    {
                        Parent.CurrentFight.CurrentWaveCnt += 1;
                        Parent.CurrentFight.CurrentWave = Parent.CurrentFight.ReferenceFight.Waves[Parent.CurrentFight.CurrentWaveCnt - 1];
                        Parent.HostileParty = Parent.getCombatUnits(Parent.CurrentFight.CurrentWave.Units);
                        //set start action value
                        foreach (Unit unit in Parent.AllUnits)
                        {
                            unit.Stats.ResetAV();
                        }
                        
                    }
                    else
                    {
                        Parent.HostileParty.Clear();
                        Parent.CurrentFight.CurrentWaveCnt -= 1;
                        if (Parent.CurrentFight.CurrentWaveCnt > 0)
                            Parent.CurrentFight.CurrentWave = Parent.CurrentFight.ReferenceFight.Waves[Parent.CurrentFight.CurrentWaveCnt - 1];

                    }

                }
                else if (ent.Type == EventType.Mod) //Apply mod
                {

                    foreach (var mod in ent.Mods)
                    {
                        if (mod.Target == Mod.ModTarget.Party)
                        {
                            foreach (var unit in Parent.Party)
                                if (!revert)
                                    unit.ApplyMod(mod);
                                else
                                    unit.RemoveMod(mod);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
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
            foreach (Resource res in Parent.Resources)
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
            foreach (Unit unit in Parent.Party.Where(partyMember => partyMember.IsAlive))
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
                if (Parent.FullFiledConditions(ability))
                {
                    ExecuteTechnique(ability);
                    return;
                }
            }
            abilities.Clear();
            abilities = null;
            orderedAbilities = null;

        }

        //Cast all techniques before fights starts
        public void ExecuteAbilityFromQueue()
        {
            StepType = StepTypeEnm.ExecuteStartQueue;
            List<Ability> abilities = new List<Ability>();
            Ability fromQ = Parent.BeforeStartQueue.First();
            Events.AddRange(fromQ.Events.Where(x => x.OnStepType == StepType));//copy events from ability reference
            Events.Add(new Event() { Type = EventType.CombatStartSkillDeQueue, AbilityValue = fromQ });//energy drain
            Actor = fromQ.Parent;//WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.Parent but can change in future)
            ActorAbility = fromQ;//WAT ABILITY is casting

            abilities.Clear();
            abilities = null;
            fromQ = null;

        }

        /// <summary>
        /// Execute the technique
        /// </summary>
        public void ExecuteTechnique(Ability ability)
        {
            StepType = StepTypeEnm.TechniqueUse;
            Events.AddRange(ability.Events.Where(x => x.OnStepType == StepType));//copy events from ability reference
            if (ability.CostType != ResourceType.nil)
                Events.Add(new Event() { Type = EventType.PartyResourceDrain, ResType = ability.CostType, ResourceValue = ability.Cost });//energy drain
            Actor = ability.Parent;//WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.Parent but can change in future)
            ActorAbility = ability;//WAT ABILITY is casting

        }

        /// <summary>
        /// load battle step activation
        /// </summary>
        /// <param name="step"></param>
        public void LoadBattleWork()
        {
            if (Parent.CurrentFight == null)
            {
                Events.Add(new Event() { Type = EventType.StartCombat });
                StepType = StepTypeEnm.StartCombat;
            }

        }
    }
}
