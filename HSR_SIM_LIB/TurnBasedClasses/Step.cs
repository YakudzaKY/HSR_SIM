using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.UnitStuff.Resource;

namespace HSR_SIM_LIB.TurnBasedClasses;

/// <summary>
///     Step-time simulator unit
/// </summary>
public class Step
{
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
        ExecuteAbilityFromQueue,
        UnitTurnSelected,
        UnitTurnStarted,
        UnitTurnEnded,
        UnitFollowUpAction,
        FinishCombat,
        UnitTurnContinued,
        EndWave
    }

    public Step(SimCls parent)
    {
        StepType = StepTypeEnm.Idle;
        Parent = parent;
    }


    public IEnumerable<Unit> TargetsHit =>
        (from p in Events
            where p is DirectDamage
            select p.TargetUnit)
        .Distinct();

    public SimCls Parent { get; }

    public StepTypeEnm StepType { get; set; }

    public Unit Actor { get; set; }

    public Ability ActorAbility { get; set; }

    public List<Event> Events { get; set; } = new();

    public List<Event> ProceedEvents { get; set; } = new();

    public List<Event> QueueEvents { get; set; }


    public bool TriggersHandled { get; set; } = false;

    public Unit Target { get; set; }

    /// <summary>
    ///     Get text description of step
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
            res = "Wave " + Parent.CurrentFight.CurrentWaveCnt;
        else if (StepType == StepTypeEnm.EndWave)
            res = "Wave completed";
        else if (StepType == StepTypeEnm.Idle)
            res = "Idle step(scenario completed?)";
        else if (StepType == StepTypeEnm.ExecuteAbilityFromQueue)
            res = "Executed " + Actor.Name + " " + ActorAbility.Name;
        else if (StepType == StepTypeEnm.UnitTurnSelected)
            res = $"{Actor.Name:s} turn next";
        else if (StepType == StepTypeEnm.UnitTurnStarted)
            res = $"{Actor.Name:s} turn start" + (ActorAbility != null ? $" with {ActorAbility.Name}" : "");
        else if (StepType == StepTypeEnm.UnitTurnEnded)
            res = $"{Actor.Name:s} finish the turn";
        else if (StepType == StepTypeEnm.UnitTurnContinued)
            res = $"{Actor.Name:s} continue the turn" +
                  (ActorAbility != null ? $" with {ActorAbility.Name}" : "");
        else if (StepType == StepTypeEnm.UnitFollowUpAction)
            res = $"{Actor.Name:s} FOLLOW UP" + (ActorAbility != null ? $" with {ActorAbility.Name}" : "");
        else
            throw new NotImplementedException();
        return res;
    }


    //get next event
    private Event GetNextEvent(bool revert)
    {
        Event res;
        if (!revert)
            res = Events.FirstOrDefault(x => !ProceedEvents.Contains(x));
        else
            res = Events.LastOrDefault(x => !ProceedEvents.Contains(x));

        return res;
    }

    /// <summary>
    ///     Proc all events in step. No random here or smart thinking. Do it on DoSomething...
    /// </summary>
    public void ProcEvents(bool revert = false, bool replay = false)
    {
        //for all events saved in step
        ProceedEvents = new List<Event>();
        var ent = GetNextEvent(revert);
        //while because new events can occur by procs
        while (ent != null)
        {
            ent.ProcEvent(revert);
            ent = GetNextEvent(revert);
        }


        if (!replay)
            Events = ProceedEvents;
    }


    //Cast all techniques before fights starts
    public void TechniqueWork(Team whosTeam)
    {
        Ability someThingToCast = null;
        foreach (var unit in whosTeam.Units.Where(partyMember => partyMember.IsAlive).OrderBy(x => x.Fighter.Role))
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
    ///     Execute one ability
    /// </summary>
    /// <param name="ability"></param>
    public void ExecuteAbility(Ability ability, Unit target = null)
    {
        Actor = ability.Parent
            .Parent; //WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.ParentStep but can change in future)
        Target = target;
        if (target != null && !target.IsAlive)
            throw new Exception($"{target.Name} is dead. cant use {ability.Name} ");
        ActorAbility = ability; //WAT ABILITY is casting
        //set ability on CD
        ability.CooldownTimer = ability.Cooldown;

        //set ability to start execute
        Events.Add(new ExecuteAbilityStart(this, ability, ability.Parent.Parent)
            { TargetUnit = target });

        //res gaining
        if (ability.SpGain > 0)
            Events.Add(new PartyResourceGain(this, ability.Parent, ability.Parent.Parent)
            {
                ResType = ResourceType.SP, TargetUnit = ability.Parent.Parent, Val = ability.SpGain
            });

        //res spending
        if (ability.CostType == ResourceType.TP || ability.CostType == ResourceType.SP)
        {
            //TP wasted before
            if (ability.CostType != ResourceType.TP)
                Events.Add(new PartyResourceDrain(this, ability.Parent, ability.Parent.Parent)
                    { ResType = (ResourceType)ability.CostType, Val = ability.Cost});
        }
        else if (ability.CostType != null)
        {
            Events.Add(new ResourceDrain(this, ability, ability.Parent.Parent)
            {
                TargetUnit = Actor, ResType = (ResourceType)ability.CostType, Val = ability.Cost
            });
        }

        //clone events by targets
        foreach (var ent in ability.Events.Where(x => x.OnStepType == StepType || x.OnStepType == null))
            //check ratio cooldown
            if (!ent.IsReady)
            {
                ent.ReduceRatioCounter();
            }
            else
            {
                ent.SetMaxCounter();
                if (ent.CalculateTargets != null || ent.TargetUnit == null) //need set targets
                {
                    var targetsUnits =
                        ent.CalculateTargets != null
                            ? ent.CalculateTargets()
                            : ability.GetTargets(target, ent.TargetType, ent.CurrentTargetType);
                    foreach (var unit in targetsUnits) CloneEvent(ability, ent, unit);
                }
                else
                {
                    CloneEvent(ability, ent, null);
                }
            }

        if (ability.AbilityType == AbilityTypeEnm.Technique)
            Events.Add(new CombatStartSkillDeQueue(this, null, ability.Parent.Parent)
            {
                ParentStep = this
            });
        //set ability to finish  execute
        Events.Add(new ExecuteAbilityFinish(this, ability, ability.Parent.Parent)
            { TargetUnit = target });
    }

    /// <summary>
    ///     create weakness shred and directDamage events
    /// </summary>
    /// <param name="ability"></param>
    /// <param name="ent"></param>
    /// <param name="target"></param>
    private void CloneEvent(Ability ability, Event ent, Unit target)
    {
        var unitEnt = (Event)ent.Clone();
        unitEnt.Reference = ent;
        unitEnt.ParentStep = this;
        unitEnt.TargetUnit = target ?? ent.TargetUnit;
        Events.Add(unitEnt);
    }


    //Cast all techniques before fights starts
    public void ExecuteAbilityFromQueue()
    {
        StepType = StepTypeEnm.ExecuteAbilityFromQueue;
        var fromQ = Parent.BeforeStartQueue.First();
        ExecuteAbility(fromQ);
    }

    /// <summary>
    ///     Execute the technique
    /// </summary>
    public void ExecuteTechniqueUse(Ability ability)
    {
        StepType = StepTypeEnm.ExecuteTechnique;
        if (ability.AbilityType == AbilityTypeEnm.Technique)
        {
            Events.Add(new CombatStartSkillQueue(this, null, ability.Parent.Parent)
            {
                ParentStep = this
            });
            if (ability.Attack)
                Events.Add(new EnterCombat(this, null, ability.Parent.Parent)
                {
                    ParentStep = this
                });
        }

        foreach (var ent in ability.Events.Where(x => x.OnStepType == StepType))
        {
            var unitEnt = (Event)ent.Clone();
            unitEnt.ParentStep = this;
            Events.Add(unitEnt);
        }

        if (ability.CostType == ResourceType.TP || ability.CostType == ResourceType.SP)
            Events.Add(new PartyResourceDrain(this, null, ability.Parent.Parent)
                { ResType = (ResourceType)ability.CostType, Val = ability.Cost });
        else if (ability.CostType != null)
            Events.Add(new ResourceDrain(this, null, ability.Parent.Parent)
                { ResType = (ResourceType)ability.CostType, Val = ability.Cost });

        Actor = ability.Parent
            .Parent; //WHO CAST THE ABILITY for some simple things save the parent( still can use ActorAbility.ParentStep but can change in future)
        ActorAbility = ability; //WAT ABILITY is casting
    }

    /// <summary>
    ///     load battle step activation
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
    ///     Do some folow up actions and unltimates
    /// </summary>
    public bool FollowUpActions()
    {
        //all alive and NO-cced units
        foreach (var prio in Enum.GetValues(typeof(PriorityEnm)).Cast<PriorityEnm>())
        foreach (var unit in Parent.AllUnits.Where(x => !x.Controlled && x.IsAlive))
        foreach (var ability in unit.Fighter.Abilities.Where(x => x.Priority == prio &&
                                                                  (x.AbilityType == AbilityTypeEnm.FollowUpAction ||
                                                                   x.AbilityType == AbilityTypeEnm.Ultimate) &&
                                                                  x.Available()))
        {
            StepType = StepTypeEnm.UnitFollowUpAction;
            Actor = unit;
            ActorAbility = ability;
            ExecuteAbility(ability, Actor.Fighter.GetBestTarget(ability));
            return true;
        }

        return false;
    }

    public bool Actions()
    {
        return false;
    }
}