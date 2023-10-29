using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.TurnBasedClasses.Events.Event;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.TurnBasedClasses.Step;
using static HSR_SIM_LIB.UnitStuff.Unit;
using static HSR_SIM_LIB.UnitStuff.Team;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Skills;
using static HSR_SIM_LIB.TurnBasedClasses.CombatFight;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using static HSR_SIM_LIB.Skills.Effect;
using static HSR_SIM_LIB.Skills.Mod;

namespace HSR_SIM_LIB.TurnBasedClasses
{/// <summary>
/// Combat simulation class
/// </summary>
    public class SimCls : ICloneable
    {
        Scenario currentScenario;

        Step currentStep = null;
        int currentFightStep = 0;
        public delegate void EventHandler(Event ent);
        public delegate void StepHandler(Step step);
        public IFighter.EventHandler EventHandlerProc { get; set; }
        public IFighter.StepHandler StepHandlerProc { get; set; }

        public Worker Parent { get; set; }

        public List<Team> Teams { get; } = new List<Team>();



        //ForgottenHall Cycles

        CombatFight currentFight = null;



        public IEnumerable<Unit> AllUnits
        {
            get
            {
                IEnumerable<Unit> units = new List<Unit>();
                foreach (Team team in Teams)
                {
                    if (team.Units != null)
                        units = units.Concat(team.Units);
                }
                return units;

            }
            set { throw new NotImplementedException(); }
        }


        public Step CurrentStep { get => currentStep; set => currentStep = value; }
        internal Scenario CurrentScenario { get => currentScenario; set => currentScenario = value; }
        public List<Step> steps = new();
        public List<Step> Steps { get => steps; set => steps = value; }
        internal CombatFight CurrentFight { get => currentFight; set => currentFight = value; }
        public int CurrentFightStep { get => currentFightStep; set => currentFightStep = value; }
        /// <summary>
        /// Do enter combat on next step proc
        /// </summary>
        public bool DoEnterCombat { get; internal set; }



        private List<Ability> beforeStartQueue = new();
        public List<Ability> BeforeStartQueue { get => beforeStartQueue; set => beforeStartQueue = value; }

        private Fight nextFight;
        public Fight NextFight
        {
            get
            {
                //try search next fight
                if (nextFight == null)
                {
                    if (CurrentFightStep < CurrentScenario.Fights.Count)
                        nextFight = CurrentScenario.Fights[CurrentFightStep];
                }

                return nextFight;
            }
        }

        public Team PartyTeam
        {
            get
            {
                return Teams.First(x => x.controledTeam == true);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Team HostileTeam
        {
            get
            {
                return Teams.First(x => x.controledTeam == false && x.TeamType == TeamTypeEnm.UnitPack);
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public virtual void HandleEvent(Event ent)
        {
            if (ent is StartWave)
                foreach (Unit unit in AllUnits)
                {
                    ent.ChildEvents.Add(new UnitEnteringBattle(ent.ParentStep, this, unit) { TargetUnit = unit });
                }

            if (ent is DamageEventTemplate)
            {
                List<Mod> toDispell = new List<Mod>();
                toDispell.AddRange(ent.TargetUnit.Mods.Where(x =>
                    x.Effects.Any(y => y.EffType == Effect.EffectType.Shield && y.Value <= 0)));
                //dispell zero shields
                foreach (Mod mod in toDispell)
                {
                    ent.DispelMod(mod, true);

                }

                //HP reduced to 0
                if (ent.RealVal != 0 && ent.TargetUnit.GetRes(Resource.ResourceType.HP).ResVal == 0)
                {

                    Defeat defeatEvent = new(ent.ParentStep, ent.Source, ent.SourceUnit)
                    {

                        AbilityValue = ent.AbilityValue,
                        TargetUnit = ent.TargetUnit

                    };
                    //remove all buffs and debuffs
                    defeatEvent.RemovedMods.AddRange(ent.TargetUnit.Mods);
                    ent.ChildEvents.Add(defeatEvent);


                }


            }

            if (ent is ResourceDrain)
            {

                //HP reduced to 0
                if (((ResourceDrain)ent).ResType == Resource.ResourceType.HP && ent.RealVal != 0 && ent.TargetUnit.GetRes(((ResourceDrain)ent).ResType).ResVal == 0)
                {

                    Defeat defeatEvent = new(ent.ParentStep, ent.Source, ent.SourceUnit)
                    {

                        AbilityValue = ent.AbilityValue,
                        TargetUnit = ent.TargetUnit

                    };
                    //remove all buffs and debuffs
                    defeatEvent.RemovedMods.AddRange(ent.TargetUnit.Mods);
                    ent.ChildEvents.Add(defeatEvent);



                }

                //THG reduced tp 0
                if (((ResourceDrain)ent).ResType == Resource.ResourceType.Toughness && ent.RealVal != 0 && ent.TargetUnit.GetRes(((ResourceDrain)ent).ResType).ResVal == 0)
                {

                    //temporary give back the THG for calculation break damage. will be reduce at the end
                    ent.TargetUnit.GetRes(((ResourceDrain)ent).ResType).ResVal += (double)ent.RealVal;
                    ShieldBreak shieldBrkEvent = new(ent.ParentStep, ent.Source, ent.SourceUnit)
                    {

                        AbilityValue = ent.AbilityValue,
                        TargetUnit = ent.TargetUnit

                    };
                    shieldBrkEvent.Val = FighterUtils.CalculateShieldBrokeDmg(shieldBrkEvent);
                    ent.ChildEvents.Add(shieldBrkEvent);

                    ModActionValue delayAV = new(ent.ParentStep, ent.Source, ent.SourceUnit) { AbilityValue = ent.AbilityValue, TargetUnit = ent.TargetUnit, Val = -ent.TargetUnit.Stats.BaseActionValue * 0.25 };//default delay
                    ent.ChildEvents.Add(delayAV);
                    // https://honkai-star-rail.fandom.com/wiki/Toughness need implement additional effects
                    switch (ent.AbilityValue.Element)
                    {
                        case Unit.ElementEnm.Physical:
                            ent.TryDebuff(new Mod(ent.SourceUnit) {  DoNotClone = true, Type = ModType.Dot, BaseDuration = 2, Effects = new List<Effect>() { new Effect() { EffType = EffectType.Bleed, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } } }, 1.5);
                            break;
                        case Unit.ElementEnm.Fire:
                            ent.TryDebuff(new Mod(ent.SourceUnit) {  DoNotClone = true, Type = ModType.Dot, BaseDuration = 2, Effects = new List<Effect>() { new Effect() { EffType = EffectType.Burn, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } } }, 1.5);
                            break;
                        case Unit.ElementEnm.Ice:
                            ent.TryDebuff(new Mod(ent.SourceUnit) {  DoNotClone = true, Type = ModType.Debuff, BaseDuration = 1, Effects = new List<Effect>() { new Effect() { EffType = EffectType.Freeze, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } } }, 1.5);
                            break;
                        case Unit.ElementEnm.Lightning:
                            ent.TryDebuff(new Mod(ent.SourceUnit) { DoNotClone = true, Type = ModType.Dot, BaseDuration = 2, Effects = new List<Effect>() { new Effect() { EffType = EffectType.Shock, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } } }, 1.5);
                            break;
                        case Unit.ElementEnm.Wind:
                            ent.TryDebuff(new Mod(ent.SourceUnit) { DoNotClone = true, Type = ModType.Dot, BaseDuration = 2, MaxStack = 5, Effects = new List<Effect>() { new Effect() { EffType = EffectType.WindShear, CalculateValue = FighterUtils.CalculateShieldBrokeDmg } } }, 1.5);
                            break;
                        case Unit.ElementEnm.Quantum:
                            ent.TryDebuff(new Mod(ent.SourceUnit)
                            {
                                Dispellable = true,
                                DoNotClone = true,
                                Type = ModType.Debuff,
                                BaseDuration = 1,
                                MaxStack = 5,
                                Effects = new List<Effect>() {
                                        new Effect(){EffType=EffectType.Entanglement,CalculateValue = FighterUtils.CalculateShieldBrokeDmg}
                                        ,new Effect(){EffType=EffectType.Delay,Value = 0.20*(1+ent.SourceUnit.Stats.BreakDmg) ,StackAffectValue = false}
                                    }
                            }, 1.5);

                            break;
                        case Unit.ElementEnm.Imaginary:
                            ent.TryDebuff(new Mod(ent.SourceUnit)
                            {
                                Dispellable = true,
                                DoNotClone = true,
                                Type = ModType.Debuff,
                                BaseDuration = 1,
                                Effects = new List<Effect>() {
                                            new Effect(){EffType=EffectType.Imprisonment,CalculateValue = FighterUtils.CalculateShieldBrokeDmg}
                                            ,new Effect(){EffType=EffectType.Delay,Value = 0.30*(1+ent.SourceUnit.Stats.BreakDmg)}
                                            ,new Effect(){EffType=EffectType.ReduceSpdPrc,Value = 0.1}
                                        }
                            }, 1.5);
                            break;
                        default:
                            throw new NotImplementedException();

                    }
                    //reduce THG  again
                    ent.TargetUnit.GetRes(((ResourceDrain)ent).ResType).ResVal -= (double)ent.RealVal;

                }
            }


            //next handlers 
            foreach (Unit unit in AllUnits)
            {
                unit.Fighter.EventHandlerProc.Invoke(ent);
                foreach (Mod mod in unit.Mods.Where(x => x.EventHandlerProc != null))
                {
                    mod.EventHandlerProc?.Invoke(ent);
                }
            }


        }
        public virtual void HandleStep(Step step)
        {

            foreach (Unit unit in AllUnits)
            {
                unit.Fighter.StepHandlerProc.Invoke(CurrentStep);
                foreach (Mod mod in unit.Mods.Where(x => x.EventHandlerProc != null))
                {
                    mod.StepHandlerProc?.Invoke(step);
                }
            }
        }


        public Team SpecialTeam
        {
            get
            {
                return Teams.First(x => x.TeamType == TeamTypeEnm.Special);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// construcotor
        /// </summary>
        public SimCls()
        {
            EventHandlerProc += HandleEvent;
            StepHandlerProc += HandleStep;
        }




        /// <summary>
        /// Get unit list and clone to new combat unit list
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        public static List<Unit> GetCombatUnits(List<Unit> units)
        {
            List<Unit> res = new();
            foreach (Unit unit in units)
            {
                Unit newUnit = (Unit)unit.Clone();
                newUnit.Reference = unit;
                res.Add(newUnit);
                newUnit.InitToCombat();
            }
            return res;
        }

        /// <summary>
        /// Get resource By Type
        /// </summary>


        /// <summary>
        /// prepare things to combat simulation
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
        /// Do next step by logic priority
        /// No actions here or changes, fill only step.events and set step type
        /// newStep- null of nothing to do
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
                newStep.TechniqueWork(PartyTeam);

            //enter the combat
            else if (newStep.StepType == StepTypeEnm.Idle && DoEnterCombat == true)
                newStep.LoadBattleWork();

            //load the wave
            else if (newStep.StepType == StepTypeEnm.Idle && currentFight is { CurrentWave: null })
            {
                //fight is over
                if (currentFight.CurrentWaveCnt == currentFight.ReferenceFight.Waves.Count)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    newStep.StepType = StepTypeEnm.StartWave;
                    newStep.Events.Add(new StartWave(newStep, this, null));
                }

            }

            //Execute start fight skill queue
            else if (newStep.StepType == StepTypeEnm.Idle && currentFight?.CurrentWave != null && BeforeStartQueue.Count > 0)
            {
                newStep.ExecuteAbilityFromQueue();
            }
            else if (newStep.StepType == StepTypeEnm.Idle && currentFight.Turn == null)//set who wanna move
            {
                if (!newStep.FollowUpActions())
                {
                    newStep.StepType = StepTypeEnm.UnitTurnSelected;
                    //get first by AV unit
                    CurrentFight.Turn = new CombatFight.TurnR
                    {
                        Actor = CurrentFight.AllAliveUnits.OrderBy(x => x.Stats.ActionValue).First(),
                        TurnStage = newStep.StepType
                    };
                    newStep.Actor = CurrentFight.Turn.Actor;
                    newStep.Events.Add(new ModActionValue(newStep, this, null)
                    { Val = currentFight.Turn.Actor.Stats.ActionValue });
                    //set all Mods are "old"
                    foreach (var mod in currentFight.Turn.Actor.Mods)
                    {
                        mod.IsOld = true;
                    }
                    //dot proc
                    foreach (var dot in currentFight.Turn.Actor.Mods.Where(x => x.Type == Mod.ModType.Dot || x.IsEarlyProc()))
                    {
                        dot.Proceed(newStep);
                    }
                }
            }
            else if (newStep.StepType == StepTypeEnm.Idle && currentFight.Turn.TurnStage == StepTypeEnm.UnitTurnSelected)
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
                            { TargetUnit = CurrentFight.Turn.Actor, ResType = ResourceType.Toughness, Val = CurrentFight.Turn.Actor.Stats.MaxToughness };
                            newStep.Events.Add(gainThg);
                        }

                        Ability chooseAbility = CurrentFight.Turn.Actor.Fighter.ChoseAbilityToCast(newStep);
                        if (chooseAbility != null)
                        {
                            newStep.ExecuteAbility(chooseAbility, chooseAbility.GetBestTarget());
                            //reset turn
                            if (!chooseAbility.EndTheTurn)
                            {
                                newStep.StepType = StepTypeEnm.UnitTurnContinued;
                            }
                        }
                    }

                }


            }
            else if (newStep.StepType == StepTypeEnm.Idle && currentFight.Turn.TurnStage == StepTypeEnm.UnitTurnStarted)
            {
                //try follow up actions before target do something
                if (!newStep.FollowUpActions())
                {
                    if (!newStep.Actions())
                    {

                        newStep.StepType = StepTypeEnm.UnitTurnEnded;
                        newStep.Actor = CurrentFight.Turn.Actor;
                        newStep.Events.Add(new ResetAV(newStep, this, null)
                        { TargetUnit = CurrentFight.Turn.Actor });
                        //50% reduce av if frosted
                        if (CurrentFight.Turn.Actor.Controlled && CurrentFight.Turn.Actor.Mods.Any(x => x.Effects.Any(y => y.EffType == Effect.EffectType.Freeze)))
                            newStep.Events.Add(new ModActionValue(newStep, CurrentFight.Turn.Actor, CurrentFight.Turn.Actor) { AbilityValue = newStep.ActorAbility, TargetUnit = CurrentFight.Turn.Actor, Val = CurrentFight.Turn.Actor.Stats.BaseActionValue * 0.5 });




                        //remove buffs
                        foreach (var dot in currentFight.Turn.Actor.Mods.Where(x => x.Type != Mod.ModType.Dot && !x.IsEarlyProc()))
                        {
                            dot.Proceed(newStep);
                        }
                        CurrentFight.Turn = null;



                    }

                }
            }




            //if we doing somethings then need proced the events
            CurrentStep = newStep;
            Steps.Add(CurrentStep);

            //WHO WANNA MOVE STEP





            if (!CurrentStep.TriggersHandled)
            {
                CurrentStep.TriggersHandled = true;
                this.StepHandlerProc?.Invoke(CurrentStep);
            }

            CurrentStep.ProcEvents(false, false);



            return CurrentStep;

        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

}
