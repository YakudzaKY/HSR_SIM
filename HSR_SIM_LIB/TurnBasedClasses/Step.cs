using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.TurnBasedClasses.Event;
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

        public StepTypeEnm StepType { get => stepType; set => stepType = value; }
        public Unit Actor { get => actor; set => actor = value; }
        public Ability ActorAbility { get => actorAbility; set => actorAbility = value; }
        public List<Event> Events { get => events; set => events = value; }
        public List<Event> ProceedEvents { get => proceedEvents; set => proceedEvents = value; }
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
            else if (StepType == StepTypeEnm.StartWave)
                res = "Wave " + Parent.CurrentFight.CurrentWaveCnt.ToString();
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
                res = $"{Actor.Name:s} continue the turn" + ((ActorAbility != null) ? $" with {ActorAbility.Name}" : "");
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
            SimInit//on scenario load and combat init
            , Idle//on Idle, nothing to_do
            , ExecuteTechnique
            , StartCombat//on fight starts
            , StartWave//on wave starts
            , ExecuteAbility,
            UnitTurnSelected,
            UnitTurnStarted,
            UnitTurnEnded,
            UnitAction,
            UnitFollowUpAction,
            FinishCombat,
            UnitTurnContinued
        }



        /// <summary>
        /// Proc all events in step. No random here or smart thinking. Do it on DoSomething...
        /// </summary>
        public void ProcEvents(bool revert = false, bool replay = false)
        {
            //for all events saved in step
            List<Event> events = new();
            proceedEvents = new List<Event>();

            events.AddRange(Events);
            //rollback changes from the end of list
            if (revert)
                events.Reverse();

            foreach (Event ent in events)
                ent.ProcEvent(revert);

            events.Clear();
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
            ActorAbility = ability;//WAT ABILITY is casting


            foreach (Event ent in ability.Events.Where(x => x.OnStepType == StepType || x.OnStepType == null))
            {
                if (ent.CalculateTargets != null || ent.TargetUnit == null) //need set targets
                {
                    IEnumerable<Unit> targetsUnits =
                        (ent.CalculateTargets != null) ? ent.CalculateTargets() : ability.GetTargets(target, ent.TargetType, ent.CurentTargetType);
                    foreach (Unit unit in targetsUnits)
                    {
                        Event unitEnt = (Event)ent.Clone();
                        unitEnt.ParentStep = this;
                        unitEnt.TargetUnit = unit;
                        // shred toughness 
                        if ((unitEnt.Type == EventType.DirectDamage) &&
                            (ability.ToughnessShred != 0 || ability.CalculateToughnessShred != null))
                            UnitThgShred(unit, ability);
                        //then primary dmg
                        Events.Add(unitEnt);

                    }
                }
                else
                {

                    Event unitEnt = (Event)ent.Clone();
                    unitEnt.ParentStep = this;
                    if ((unitEnt.Type == EventType.DirectDamage) && (ability.ToughnessShred != 0 || ability.CalculateToughnessShred != null))
                        UnitThgShred(ent.TargetUnit, ability);
                    //then primary dmg
                    Events.Add(unitEnt);
                }

            }




            if (ability.AbilityType == Ability.AbilityTypeEnm.Technique)
            {
                Events.Add(new Event(this, null, ability.Parent.Parent)
                {
                    Type = EventType.CombatStartSkillDeQueue,
                    ParentStep = this,
                    AbilityValue = ability

                });
            }
        }

        private void UnitThgShred(Unit unit, Ability ability)
        {
            if (unit == null) return;
            if (ability==null) return;
            double? shredVal = 0;
            if (ability.CalculateToughnessShred != null)
            {
                shredVal = ability.CalculateToughnessShred(unit);
            }
            else
            {
                shredVal = ability.ToughnessShred;
            }

            if (unit.Fighter.Weaknesses.Any(x => x == (ability.Element)) || ability.IgnoreWeakness)
                Events.Add(new Event(null, null, ability.Parent.Parent)
                {
                    ParentStep = this,
                    Type = EventType.ResourceDrain,
                    TargetUnit = unit,
                    ResType = ResourceType.Toughness,
                    Val = shredVal,
                    AbilityValue = ability
                });
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
                Events.Add(new Event(this, null, ability.Parent.Parent)
                {
                    Type = EventType.CombatStartSkillQueue,
                    ParentStep = this,
                    AbilityValue = ability

                });
                if (ability.Attack)
                    Events.Add(new Event(this, null, ability.Parent.Parent)
                    {
                        Type = EventType.EnterCombat,
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
                Events.Add(new Event(this, null, ability.Parent.Parent) { Type = EventType.PartyResourceDrain, ResType = (ResourceType)ability.CostType, Val = ability.Cost });
            }
            else if (ability.CostType != null)
                Events.Add(new Event(this, null, ability.Parent.Parent) { Type = EventType.ResourceDrain, ResType = (ResourceType)ability.CostType, Val = ability.Cost });

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
                Events.Add(new Event(this, null, null) { Type = EventType.StartCombat });
                StepType = StepTypeEnm.StartCombat;
            }

        }

        /// <summary>
        /// Add event to step events
        /// </summary>
        /// <param name="ent">event</param>
        /// <param name="doProceed">Proceed event</param>
        /// <param name="revert">is revert ?</param>
        public void AddEvent(Event ent, bool doProceed = false, bool revert = false)
        {
            if (doProceed)
                ent.ProcEvent(revert);
            this.Events.Add(ent);
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
                                 x.AbilityType == Ability.AbilityTypeEnm.FollowUpAction && x.Available()))
                    {
                        StepType = StepTypeEnm.UnitFollowUpAction;
                        Actor = unit;
                        ActorAbility = ability;
                        ExecuteAbility(ability, unit);
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
