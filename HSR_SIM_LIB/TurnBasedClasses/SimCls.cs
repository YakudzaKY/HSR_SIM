using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.Skills.ReadyBuffs;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.TurnBasedClasses.Step;
using static HSR_SIM_LIB.UnitStuff.Unit;
using static HSR_SIM_LIB.UnitStuff.Team;
using static HSR_SIM_LIB.TurnBasedClasses.CombatFight;
using static HSR_SIM_LIB.Skills.Buff;

namespace HSR_SIM_LIB.TurnBasedClasses;

/// <summary>
///     Combat simulation class
/// </summary>
public class SimCls : ICloneable
{
    public delegate void EventHandler(Event ent);

    public delegate void StepHandler(Step step);

    public List<PreLaunchOption> PreLaunch;

    //ForgottenHall Cycles

    public List<Step> steps = new();

    /// <summary>
    ///     constructor
    /// </summary>
    public SimCls()
    {
        EventHandlerProc += HandleEvent;
        StepHandlerProc += HandleStep;
    }

    public IFighter.EventHandler EventHandlerProc { get; set; }
    public IFighter.StepHandler StepHandlerProc { get; set; }

    public Worker Parent { get; set; }

    public List<Team> Teams { get; } = new();


    public IEnumerable<Unit> AllUnits
    {
        get
        {
            IEnumerable<Unit> units = new List<Unit>();
            foreach (var team in Teams)
                if (team.Units != null)
                    units = units.Concat(team.Units);
            return units;
        }
        set => throw new NotImplementedException();
    }


    public Step CurrentStep { get; set; }
    internal Scenario CurrentScenario { get; set; }

    public List<Step> Steps
    {
        get => steps;
        set => steps = value;
    }

    internal CombatFight CurrentFight { get; set; } = null;
    public int CurrentFightStep { get; set; } = 0;

    /// <summary>
    ///     Do enter combat on next step proc
    /// </summary>
    public bool DoEnterCombat { get; internal set; }

    public List<Ability> BeforeStartQueue { get; set; } = new();

    public Fight NextFight
    {
        get
        {
            if (CurrentScenario.Fights.Count >= CurrentFightStep + 1)
                return CurrentScenario.Fights[CurrentFightStep];
            return null;
        }
    }

    public Team PartyTeam
    {
        get { return Teams.First(x => x.controledTeam); }
    }

    public Team HostileTeam
    {
        get { return Teams.First(x => x.controledTeam == false && x.TeamType == TeamTypeEnm.UnitPack); }
    }


    public Team SpecialTeam
    {
        get { return Teams.First(x => x.TeamType == TeamTypeEnm.Special); }
    }

    //Total action value per run
    public double TotalAv { get; set; } = 0;

    public object Clone()
    {
        return MemberwiseClone();
    }

    public void HandleZeroHp(Event ent)
    {
        //HP reduced to 0
        if (ent.RealVal != 0 && ent.TargetUnit.GetRes(ResourceType.HP).ResVal == 0)
        {
            if (!UnitDefeatHandled(ent, ent.TargetUnit))
                ent.ChildEvents.Add(new Defeat(ent.ParentStep, ent.Source, ent.SourceUnit)
                {
                    TargetUnit = ent.TargetUnit,
                });

            else
                ent.ChildEvents.Add(new DefeatHandled(ent.ParentStep, ent.Source, ent.SourceUnit)
                {
                    TargetUnit = ent.TargetUnit,
                });

        }
    }

    private bool UnitDefeatHandled(Event ent, Unit target)
    {
        Ability battleRes = target.ParentTeam.Units.OrderByDescending(x => x == target).Select(x =>
            x.Fighter.Abilities.FirstOrDefault(y => y.FollowUpPriority == Ability.PriorityEnm.DefeatHandler && y.Available() && y.FollowUpQueueAvailable() && y.GetTargets(target, y.TargetType, Ability.AbilityCurrentTargetEnm.AbilityMain)
                .Contains(target))).MaxBy(y => y is not null);
        if (battleRes != null)
        {
            battleRes.FollowUpTargets.Add(new KeyValuePair<Unit, Unit>(target, ent.SourceUnit));
        }

        return battleRes != null;
    }

    public void HandleEvent(Event ent)
    {
        if (ent is StartWave)
            foreach (var unit in AllUnits)
                ent.ChildEvents.Add(new UnitEnteringBattle(ent.ParentStep, this, unit) { TargetUnit = unit });

        if (ent is DamageEventTemplate)
        {
            var toDispell = new List<Buff>();
            toDispell.AddRange(ent.TargetUnit.Buffs.Where(x =>
                x.Effects.Any(y => y is EffShield && y.Value <= 0)));
            //dispell zero shields
            foreach (var mod in toDispell) ent.DispelMod(mod, true);

            HandleZeroHp(ent);
        }

        if (ent is ResourceDrain)
        {
            HandleZeroHp(ent);

            //THG reduced tp 0
            if (((ResourceDrain)ent).ResType == ResourceType.Toughness && ent.RealVal != 0 &&
                ent.TargetUnit.GetRes(((ResourceDrain)ent).ResType).ResVal == 0)
            {
                //temporary give back the THG for calculation break damage. will be reduce at the end
                ent.TargetUnit.GetRes(((ResourceDrain)ent).ResType).ResVal += (double)ent.RealVal;
                ToughnessBreak shieldBrkEvent = new(ent.ParentStep, ent.Source, ent.SourceUnit)
                {
                    TargetUnit = ent.TargetUnit
                };
                shieldBrkEvent.Val = FighterUtils.CalculateShieldBrokeDmg(shieldBrkEvent);
                ent.ChildEvents.Add(shieldBrkEvent);


                //reduce THG  again
                ent.TargetUnit.GetRes(((ResourceDrain)ent).ResType).ResVal -= (double)ent.RealVal;
            }
        }

        //next handlers 
        foreach (var unit in AllUnits)
        {
            unit.Fighter.EventHandlerProc.Invoke(ent);
            foreach (var mod in unit.Buffs.Where(x => x.EventHandlerProc != null)) mod.EventHandlerProc?.Invoke(ent);
        }
    }

    public virtual void HandleStep(Step step)
    {
        foreach (var unit in AllUnits)
        {
            unit.Fighter.StepHandlerProc.Invoke(CurrentStep);
            foreach (var mod in unit.Buffs.Where(x => x.StepHandlerProc != null)) mod.StepHandlerProc?.Invoke(step);
        }
    }


    /// <summary>
    ///     Get unit list and clone to new combat unit list
    /// </summary>
    /// <param name="units"></param>
    /// <returns></returns>
    public static List<Unit> GetCombatUnits(List<Unit> units)
    {
        List<Unit> res = new();
        foreach (var unit in units)
        {
            var newUnit = (Unit)unit.Clone();
            newUnit.Reference = unit;
            res.Add(newUnit);
            newUnit.Init();
        }

        return res;
    }

    /// <summary>
    ///     Get resource By Type
    /// </summary>
    /// <summary>
    ///     prepare things to combat simulation
    /// </summary>
    public void Prepare()
    {
        Team team;

        //main team
        team = new Team(this);
        team.BindUnits(GetCombatUnits(CurrentScenario.Party));
        team.TeamType = TeamTypeEnm.UnitPack;
        team.controledTeam = true;
        Teams.Add(team);
        /*
         * pre launch options
         */
        //load TP
        team.GetRes(ResourceType.TP).ResVal = PreLaunch.FirstOrDefault(x => x.OptionType == PreLaunchOption.PreLaunchOptionEnm.SetTp)?.Value ?? 0;
        //load energy
        foreach (Unit unit in team.Units)
        {
            unit.CurrentEnergy = unit.Stats.BaseMaxEnergy * PreLaunch.FirstOrDefault(x => x.OptionType == PreLaunchOption.PreLaunchOptionEnm.SetEnergy)?.Value ?? 0;
        }


        //Special
        team = new Team(this);
        team.BindUnits(GetCombatUnits(CurrentScenario.SpecialUnits));
        team.TeamType = TeamTypeEnm.Special;
        Teams.Add(team);

        //enemy team
        team = new Team(this)
        {
            TeamType = TeamTypeEnm.UnitPack
        };
        Teams.Add(team);
    }


    /// <summary>
    ///     Do next step by logic priority
    ///     No actions here or changes, fill only step.events and set step type
    ///     newStep- null of nothing to do
    /// </summary>
    public Step WorkIteration()
    {
        Step newStep = new(this);
        if (CurrentStep == null)
        {
            //simulation preparations
            Prepare();
            newStep.StepType = StepTypeEnm.SimInit;
        }
        //buff before fight
        else if (newStep.StepType == StepTypeEnm.Idle && DoEnterCombat == false && CurrentFight == null)
        {
            if (CurrentFightStep != CurrentScenario.Fights.Count)
                newStep.TechniqueWork(PartyTeam);
        }
        //enter the combat
        else if (newStep.StepType == StepTypeEnm.Idle && DoEnterCombat)
        {
            newStep.LoadBattleWork();
        }
        //load the wave
        else if (newStep.StepType == StepTypeEnm.Idle && CurrentFight is { CurrentWave: null })
        {
            //fight is over
            if (CurrentFight.CurrentWaveCnt == CurrentFight.ReferenceFight.Waves.Count)
            {
                newStep.StepType = StepTypeEnm.FinishCombat;
                newStep.Events.Add(new FinishCombat(newStep, this, null));
            }
            else
            {
                newStep.StepType = StepTypeEnm.StartWave;
                newStep.Events.Add(new StartWave(newStep, this, null));
            }
        }

        //Execute start fight skill queue
        else if (newStep.StepType == StepTypeEnm.Idle && CurrentFight?.CurrentWave != null &&
                 BeforeStartQueue.Count > 0)
        {
            newStep.ExecuteAbilityFromQueue();
        }
        //check wave complete
        else if (newStep.StepType == StepTypeEnm.Idle && CurrentFight?.CurrentWave != null &&
                 (PartyTeam.Units.All(x => !x.IsAlive) || HostileTeam.Units.All(x => !x.IsAlive)))
        {
            //party dead. finish sim
            if (PartyTeam.Units.All(x => !x.IsAlive)) return newStep;

            //go next wave
            CurrentFight.Turn = null;
            newStep.StepType = StepTypeEnm.EndWave;
            newStep.Events.Add(new EndWave(newStep, this, null));
        }
        else if (newStep.StepType == StepTypeEnm.Idle && CurrentFight.Turn == null) //set who wanna move
        {
            if (!newStep.FollowUpActions())
            {
                newStep.StepType = StepTypeEnm.UnitTurnSelected;
                //clear dead units in enemy team
                foreach (Unit unit in HostileTeam.Units.Where(x => !x.IsAlive))
                {
                    newStep.Events.Add(new UnbindUnit(newStep, unit, unit) { TargetUnit = unit });
                }

                //get first by AV unit
                CurrentFight.Turn = new TurnR
                {
                    Actor = CurrentFight.AllAliveUnits.OrderBy(x => x.GetCurrentActionValue(null)).First(),
                    TurnStage = newStep.StepType
                };
                newStep.Actor = CurrentFight.Turn.Actor;
                double reduceAv = CurrentFight.Turn.Actor.GetCurrentActionValue(null);
                if (reduceAv < 0)
                    reduceAv = 0;
                newStep.Events.Add(new ModActionValue(newStep, this, null)
                { Val = reduceAv });
                //set all Buffs are "old"
                foreach (var mod in CurrentFight.Turn.Actor.Buffs) mod.IsOld = true;
                //dot proc
                foreach (var dot in CurrentFight.Turn.Actor.Buffs.Where(x => x.Type == BuffType.Dot || x.IsEarlyProc()))
                    dot.Proceed(newStep);
            }
        }
        else if (newStep.StepType == StepTypeEnm.Idle &&
                 CurrentFight.Turn.TurnStage is StepTypeEnm.UnitTurnSelected or StepTypeEnm.UnitTurnContinued)
        {
            //try follow up actions before target do something
            if (!newStep.FollowUpActions())
            {
                newStep.StepType = StepTypeEnm.UnitTurnStarted;
                newStep.Actor = CurrentFight.Turn.Actor;
                CurrentFight.Turn.TurnStage = newStep.StepType;

                //if alive and has no cc
                if (CurrentFight.Turn.Actor.IsAlive && !CurrentFight.Turn.Actor.Controlled)
                {
                    //restore toughness 
                    if (CurrentFight.Turn.Actor.Stats.MaxToughness > 0 &&
                        CurrentFight.Turn.Actor.GetRes(ResourceType.Toughness).ResVal == 0)
                    {
                        Event gainThg = new ResourceGain(newStep, this, CurrentFight.Turn.Actor)
                        {
                            TargetUnit = CurrentFight.Turn.Actor,
                            ResType = ResourceType.Toughness,
                            Val = CurrentFight.Turn.Actor.Stats.MaxToughness
                        };
                        newStep.Events.Add(gainThg);
                    }

                    var chooseAbility = CurrentFight.Turn.Actor.Fighter.ChoseAbilityToCast(newStep);
                    if (chooseAbility != null)
                    {
                        newStep.ExecuteAbility(chooseAbility,
                            CurrentFight.Turn.Actor.Fighter.GetBestTarget(chooseAbility));
                        //reset turn
                        if (!chooseAbility.EndTheTurn)
                        {
                            newStep.StepType = StepTypeEnm.UnitTurnContinued;
                            CurrentFight.Turn.TurnStage = newStep.StepType;
                        }
                    }
                }
            }
        }
        else if (newStep.StepType == StepTypeEnm.Idle && CurrentFight.Turn.TurnStage == StepTypeEnm.UnitTurnStarted)
        {
            //try follow up actions before target do something.
            //follow up actions disabled at NPC turn end
            if (CurrentFight.Turn.Actor.Fighter is DefaultNPCFighter || !newStep.FollowUpActions())
            {
                newStep.StepType = StepTypeEnm.UnitTurnEnded;
                newStep.Actor = CurrentFight.Turn.Actor;
                newStep.Events.Add(new ResetAV(newStep, this, null)
                { TargetUnit = CurrentFight.Turn.Actor });

                //reset CD
                foreach (var ability in CurrentFight.Turn.Actor.Fighter.Abilities.Where(x => x.CooldownTimer > 0))
                    ability.CooldownTimer -= 1;


                //remove buffs
                foreach (var dot in CurrentFight.Turn.Actor.Buffs.Where(x =>
                             x.Type != BuffType.Dot && !x.IsEarlyProc())) dot.Proceed(newStep);
                CurrentFight.Turn = null;
            }
        }


        //if we doing somethings then need proced the events
        CurrentStep = newStep;
        Steps.Add(CurrentStep);


        if (!CurrentStep.TriggersHandled)
        {
            CurrentStep.TriggersHandled = true;
            StepHandlerProc?.Invoke(CurrentStep);
        }

        CurrentStep.ProcEvents();


        return CurrentStep;
    }
}