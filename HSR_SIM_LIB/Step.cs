using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters;
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
        public bool TriggersHandled { get; set; } = false;
        private List<Event> events = new();

        /// <summary>
        /// Get text description of step
        /// </summary>
        /// <param name="step"></param>
        /// <returns></returns>
        public string GetDescription()
        {
            string res;
            if (StepType == StepTypeEnm.SimInit)
                res = "sim was initialized";
            else if (StepType == StepTypeEnm.ExecuteAbilityUse)
                res = Actor.Name + " used " + ActorAbility.Name;
            else if (StepType == StepTypeEnm.StartCombat)
                res = "Starting the combat!";
            else if (StepType == StepTypeEnm.StartWave)
                res = "Wave " + Parent.CurrentFight.CurrentWaveCnt.ToString();
            else if (StepType == StepTypeEnm.Idle)
                res = "Idle step(scenario completed?)";
            else if (StepType == StepTypeEnm.ExecuteStartQueue)
                res = "Executed " + Actor.Name + " " + ActorAbility.Name;
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
            , ExecuteAbilityUse
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
            List<Event> events = new();
            events.AddRange(Events);
            //rollback changes from the end of list
            if (revert)
                events.Reverse();

            foreach (Event ent in events)
                ent.ProcEvent(revert);

            events.Clear();
        }


        //Cast all techniques before fights starts
        public void TechniqueWork(Team whosTeam)
        {
            Ability someThingToCast = null;
            foreach (Unit unit in whosTeam.Units.Where(partyMember => partyMember.IsAlive))
            {
                someThingToCast = unit.Fighter.ChooseAbilityToCast(this);
                if (someThingToCast != null)
                {
                    ExecuteAbilityUse(someThingToCast);
                    break;
                }

            }

        }
        /// <summary>
        /// Execute one ability
        /// </summary>
        /// <param name="ability"></param>
        public void ExecuteAbility(Ability ability)
        {
            Actor = ability.Parent;//WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.Parent but can change in future)
            ActorAbility = ability;//WAT ABILITY is casting

            foreach (Event ent in ability.Events.Where(x => x.OnStepType == StepType))
            {
                if (ent.CalculateTargets != null)
                    foreach (Unit unit in ent.CalculateTargets(ent))
                    {
                        Event unitEnt = (Event)ent.Clone();
                        unitEnt.ParentStep = this;
                        unitEnt.TargetUnit = unit;
                        Events.Add(unitEnt);
                        
                    }
                else
                {
                    Event unitEnt = (Event)ent.Clone();
                    unitEnt.ParentStep = this;
                    Events.Add(unitEnt);
                }

            }
            //update mods
            foreach (Event ent in Events.Where(x => x.Type == EventType.Mod))
            {

                List<Mod> newMods = new();

                //calculated mods
                foreach (Mod mod in ent.Mods.Where(x => x.CalculateTargets != null))
                {
                    foreach (Unit unit in mod.CalculateTargets(ent))
                    {
                        Mod newMod = (Mod)mod.Clone();
                        newMod.RefMod = (mod.RefMod ?? mod);
                        newMod.TargetUnit = unit;
                        newMods.Add(newMod);
                    }
                }

                IEnumerable<Mod> oldList = ent.Mods.Where(x => x.CalculateTargets == null);

                //non calculated mods
                if (oldList.Any())
                {
                    newMods = (List<Mod>)newMods.Concat(oldList);
                }

                ent.Mods = newMods;
            }
        }
        //Cast all techniques before fights starts
        public void ExecuteAbilityFromQueue()
        {
            StepType = StepTypeEnm.ExecuteStartQueue;
            Ability fromQ = Parent.BeforeStartQueue.First();
            ExecuteAbility(fromQ);
        }

        /// <summary>
        /// Execute the technique
        /// </summary>
        public void ExecuteAbilityUse(Ability ability)
        {
            StepType = StepTypeEnm.ExecuteAbilityUse;
      

            foreach (Event ent in ability.Events.Where(x => x.OnStepType == StepType))
            {
   
                    Event unitEnt = (Event)ent.Clone();
                    unitEnt.ParentStep = this;
                    Events.Add(unitEnt);
                

            }

            if (ability.CostType == ResourceType.TP || ability.CostType == ResourceType.SP)
            {
                Events.Add(new Event(this) { Type = EventType.PartyResourceDrain, ResType = ability.CostType, Val = ability.Cost });
            }
            else if (ability.CostType != ResourceType.nil)
                Events.Add(new Event(this) { Type = EventType.ResourceDrain, ResType = ability.CostType, Val = ability.Cost });

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
                Events.Add(new Event(this) { Type = EventType.StartCombat });
                StepType = StepTypeEnm.StartCombat;
            }

        }
    }
}
