using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.UnitStuff;
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
            , ExecuteAbility
            
        }



        /// <summary>
        /// Proc all events in step. No random here or smart thinking. Do it on DoSomething...
        /// </summary>
        public void ProcEvents(bool revert = false,bool replay=false)
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
            foreach (Unit unit in whosTeam.Units.Where(partyMember => partyMember.IsAlive))
            {
                someThingToCast = unit.Fighter.ChooseAbilityToCast(this);
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
        public void ExecuteAbility(Ability ability)
        {
            Actor = ability.Parent;//WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.Parent but can change in future)
            ActorAbility = ability;//WAT ABILITY is casting



            foreach (Event ent in ability.Events.Where(x => x.OnStepType == StepType))
            {
                if (ent.CalculateTargets != null)
                    foreach (Unit unit in ent.CalculateTargets())
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
            //tougness shred
            if (ability.ToughnessShred != 0 || ability.CalculateToughnessShred != null)
            {
                if (ability.TargetType == Ability.TargetTypeEnm.Hostiles)
                {
                    foreach (Unit unit in ((DefaultFighter)ability.Parent.Fighter).GetWeaknessTargets())
                    {
                        double? shredVal = 0;
                        if (ability.CalculateToughnessShred != null)
                        {
                            shredVal = ability.CalculateToughnessShred(unit);
                        }
                        else
                        {
                            shredVal = ability.ToughnessShred;
                        }


                        Events.Add(new Event(null,null)
                        {
                            ParentStep = this,
                            Type = EventType.ResourceDrain,
                            TargetUnit = unit,
                            ResType = ResourceType.Toughness,
                            Val = shredVal,
                            AbilityValue = ability
                        });
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }

            }


            //update mods
            foreach (Event ent in Events.Where(x => x.Type == EventType.Mod))
            {

                List<Mod> newMods = new();

                //calculated mods
                foreach (Mod mod in ent.Mods.Where(x => x.CalculateTargets != null))
                {
                    foreach (Unit unit in mod.CalculateTargets())
                    {
                        Mod newMod = (Mod)mod.Clone();
                        newMod.RefMod = mod.RefMod ?? mod;
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

            if (ability.AbilityType == Ability.AbilityTypeEnm.Technique)
            {
                Events.Add(new Event(this,null)
                {
                    Type = EventType.CombatStartSkillDeQueue,
                    ParentStep = this,
                    AbilityValue = ability

                });
            }
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
                Events.Add(new Event(this, null)
                {
                    Type = EventType.CombatStartSkillQueue,
                    ParentStep = this,
                    AbilityValue = ability

                });
                if (ability.Attack)
                    Events.Add(new Event(this,null)
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
                Events.Add(new Event(this,null) { Type = EventType.PartyResourceDrain, ResType = ability.CostType, Val = ability.Cost });
            }
            else if (ability.CostType != ResourceType.nil)
                Events.Add(new Event(this,null) { Type = EventType.ResourceDrain, ResType = ability.CostType, Val = ability.Cost });

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
                Events.Add(new Event(this, null) { Type = EventType.StartCombat });
                StepType = StepTypeEnm.StartCombat;
            }

        }
    }
}
