using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.TurnBasedClasses.Events.Event;
using static HSR_SIM_LIB.UnitStuff.Resource;

namespace HSR_SIM_LIB.TurnBasedClasses
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

        public StepTypeEnm StepType
        {
            get => stepType;
            set => stepType = value;
        }

        public Unit Actor
        {
            get => actor;
            set => actor = value;
        }

        public Ability ActorAbility
        {
            get => actorAbility;
            set => actorAbility = value;
        }

        public List<Event> Events
        {
            get => events;
            set => events = value;
        }

        public List<Event> ProceedEvents
        {
            get => proceedEvents;
            set => proceedEvents = value;
        }

        public List<Event> QueueEvents { get; set; }


        public bool TriggersHandled { get; set; } = false;
        private List<Event> proceedEvents = new();
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
            else if (StepType == StepTypeEnm.ExecuteTechnique)
                res = Actor.Name + " used " + ActorAbility.Name;
            else if (StepType == StepTypeEnm.StartCombat)
                res = "Starting the combat!";
            else if (StepType == StepTypeEnm.FinishCombat)
                res = "Finish the combat!";
            else if (StepType == StepTypeEnm.StartWave)
                res = "Wave " + Parent.CurrentFight.CurrentWaveCnt.ToString();
            else if (StepType == StepTypeEnm.EndWave)
                res = "Wave completed";
            else if (StepType == StepTypeEnm.Idle)
                res = "Idle step(scenario completed?)";
            else if (StepType == StepTypeEnm.ExecuteAbility)
                res = "Executed " + Actor.Name + " " + ActorAbility.Name;
            else if (StepType == StepTypeEnm.UnitTurnSelected)
                res = $"{Actor.Name:s} turn next";
            else if (StepType == StepTypeEnm.UnitTurnStarted)
                res = $"{Actor.Name:s} turn start" + ((ActorAbility != null) ? $" with {ActorAbility.Name}" : "");
            else if (StepType == StepTypeEnm.UnitTurnEnded)
                res = $"{Actor.Name:s} finish the turn";
            else if (StepType == StepTypeEnm.UnitTurnContinued)
                res = $"{Actor.Name:s} continue the turn" +
                      ((ActorAbility != null) ? $" with {ActorAbility.Name}" : "");
            else if (StepType == StepTypeEnm.UnitFollowUpAction)
                res = $"{Actor.Name:s} FOLLOW UP" + ((ActorAbility != null) ? $" with {ActorAbility.Name}" : "");
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
            SimInit //on scenario load and combat init
            ,
            Idle //on Idle, nothing to_do
            ,
            ExecuteTechnique,
            StartCombat //on fight starts
            ,
            StartWave //on wave starts
            ,
            ExecuteAbility,
            UnitTurnSelected,
            UnitTurnStarted,
            UnitTurnEnded,
            UnitFollowUpAction,
            FinishCombat,
            UnitTurnContinued,
            EndWave
        }


        //get next event
        private Event GetNextEvent(bool revert)
        {
            Event res;
            if (!revert)
                res = Events.FirstOrDefault(x => !proceedEvents.Contains(x));
            else
                res = Events.LastOrDefault(x => !proceedEvents.Contains(x));

            return res;
        }

        /// <summary>
        /// Proc all events in step. No random here or smart thinking. Do it on DoSomething...
        /// </summary>
        public void ProcEvents(bool revert = false, bool replay = false)
        {
            //for all events saved in step
            proceedEvents = new List<Event>();
            Event ent = GetNextEvent(revert);
            //while because new events can occur by procs
            while (ent != null)
            {
                ent.ProcEvent(revert);
                ent = GetNextEvent(revert);
            }


            if (!replay)
                Events = proceedEvents;
        }


        //Cast all techniques before fights starts
        public void TechniqueWork(Team whosTeam)
        {
            Ability someThingToCast = null;
            foreach (Unit unit in whosTeam.Units.Where(partyMember => partyMember.IsAlive).OrderBy(x => x.Fighter.Role))
            {
                someThingToCast = unit.Fighter.ChoseAbilityToCast(this);
                if (someThingToCast != null)
                {
                    ExecuteTechniqueUse(someThingToCast);
                    break;
                }

            }

        }
        /// <summary>
        /// Execute one ability
        /// </summary>
        /// <param name="ability"></param>
        public void ExecuteAbility(Ability ability, Unit target = null)
        {
            Actor = ability.Parent.Parent;//WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.Parent but can change in future)
            Target = target;
            if (target != null && !target.IsAlive)
                throw new Exception($"{target.Name} is dead. cant use {ability.Name} ");
            ActorAbility = ability;//WAT ABILITY is casting
            //set ability on CD
            ability.CooldownTimer = ability.Cooldown;

            //res gaining
            if (ability.SpGain > 0)
            {
                Events.Add(new PartyResourceGain(this, ability.Parent, ability.Parent.Parent) { ResType = Resource.ResourceType.SP, TargetUnit = ability.Parent.Parent, Val = ability.SpGain, AbilityValue = ability });
            }

            //res spending
            if (ability.CostType == ResourceType.TP || ability.CostType == ResourceType.SP)
            {
                //TP wasted before
                if (ability.CostType != ResourceType.TP)
                    Events.Add(new PartyResourceDrain(this, ability.Parent, ability.Parent.Parent) { ResType = (ResourceType)ability.CostType, Val = ability.Cost, AbilityValue = ability });
            }
            else if (ability.CostType != null)
                Events.Add(new ResourceDrain(this, ability, ability.Parent.Parent) { TargetUnit = Actor, ResType = (ResourceType)ability.CostType, Val = ability.Cost, AbilityValue = ability });

            //Energy regen
            if (ability.EnergyGain > 0)
                Events.Add(new EnergyGain(this, ability, ability.Parent.Parent) { Val = ability.EnergyGain, TargetUnit = ability.Parent.Parent, AbilityValue = ability });

            //clone events by targets
            foreach (Event ent in ability.Events.Where(x => x.OnStepType == StepType || x.OnStepType == null))
            {
                if (ent.CalculateTargets != null || ent.TargetUnit == null) //need set targets
                {
                    IEnumerable<Unit> targetsUnits =
                        (ent.CalculateTargets != null) ? ent.CalculateTargets() : ability.GetTargets(target, ent.TargetType, ent.CurentTargetType);
                    foreach (Unit unit in targetsUnits)
                    {
                        CreateDamagesEvents(ability,ent,unit);
                    }
                }
                else
                {
                    CreateDamagesEvents(ability,ent,null);
                }

            }




            if (ability.AbilityType == Ability.AbilityTypeEnm.Technique)
            {
                Events.Add(new CombatStartSkillDeQueue(this, null, ability.Parent.Parent)
                {

                    ParentStep = this,
                    AbilityValue = ability

                });
            }
        }

        public Unit Target { get; set; }

        /// <summary>
        /// create weakness shred and directDamage events
        /// </summary>
        /// <param name="ability"></param>
        /// <param name="ent"></param>
        /// <param name="target"></param>
        private void CreateDamagesEvents(Ability ability, Event ent, Unit target)
        {
            Event unitEnt = (Event)ent.Clone();
            unitEnt.ParentStep = this;
            unitEnt.TargetUnit = target??ent.TargetUnit;
            if (ability.EnergyGive > 0)
                Events.Add(new EnergyGain(this, ability, ability.Parent.Parent) { Val = ability.EnergyGive, TargetUnit = unitEnt.TargetUnit, AbilityValue = ability });
            Events.Add(unitEnt);
          
        }



        //Cast all techniques before fights starts
        public void ExecuteAbilityFromQueue()
        {
            StepType = StepTypeEnm.ExecuteAbility;
            Ability fromQ = Parent.BeforeStartQueue.First();
            ExecuteAbility(fromQ);
        }

        /// <summary>
        /// Execute the technique
        /// </summary>
        public void ExecuteTechniqueUse(Ability ability)
        {
            StepType = StepTypeEnm.ExecuteTechnique;
            if (ability.AbilityType == Ability.AbilityTypeEnm.Technique)
            {
                Events.Add(new CombatStartSkillQueue(this, null, ability.Parent.Parent)
                {

                    ParentStep = this,
                    AbilityValue = ability

                });
                if (ability.Attack)
                    Events.Add(new EnterCombat(this, null, ability.Parent.Parent)
                    {
                        ParentStep = this,
                        AbilityValue = ability

                    });

            }

            foreach (Event ent in ability.Events.Where(x => x.OnStepType == StepType))
            {

                Event unitEnt = (Event)ent.Clone();
                unitEnt.ParentStep = this;
                Events.Add(unitEnt);


            }

            if (ability.CostType == ResourceType.TP || ability.CostType == ResourceType.SP)
            {
                Events.Add(new PartyResourceDrain(this, null, ability.Parent.Parent) { ResType = (ResourceType)ability.CostType, Val = ability.Cost });
            }
            else if (ability.CostType != null)
                Events.Add(new ResourceDrain(this, null, ability.Parent.Parent) { ResType = (ResourceType)ability.CostType, Val = ability.Cost });

            Actor = ability.Parent.Parent;//WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.Parent but can change in future)
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
                Events.Add(new StartCombat(this, null, null));
                StepType = StepTypeEnm.StartCombat;
            }

        }



        /// <summary>
        /// Do some folow up actions and unltimates
        /// </summary>
        public bool FollowUpActions()
        {
            //all alive and NO-cced units
            foreach (Ability.PriorityEnm prio in Enum.GetValues(typeof(PriorityEnm)).Cast<PriorityEnm>())
            {
                foreach (Unit unit in this.Parent.AllUnits.Where(x => !x.Controlled && x.IsAlive))
                {

                    foreach (Ability ability in unit.Fighter.Abilities.Where(x => x.Priority == prio &&
                                 (x.AbilityType==Ability.AbilityTypeEnm.FollowUpAction || x.AbilityType==Ability.AbilityTypeEnm.Ultimate) && x.Available()))
                    {
                        StepType = StepTypeEnm.UnitFollowUpAction;
                        Actor = unit;
                        ActorAbility = ability;
                        ExecuteAbility(ability, Actor.Fighter.GetBestTarget(ability));
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Actions()
        {
            return false;
        }
    }
}
