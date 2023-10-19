using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.TurnBasedClasses.Event;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.TurnBasedClasses.Step;
using static HSR_SIM_LIB.UnitStuff.Unit;
using static HSR_SIM_LIB.UnitStuff.Team;
using System.Xml.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.UnitStuff;
using HSR_SIM_LIB.Skills;

namespace HSR_SIM_LIB.TurnBasedClasses
{/// <summary>
/// Combat simulation class
/// </summary>
    public class SimCls:ICloneable
    {
        Scenario currentScenario;

        Step currentStep = null;
        int currentFightStep = 0;

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



        private List<Ability> beforeStartQueue = new List<Ability>();
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
                    newStep.Events.Add(new Event(newStep,this) { Type = EventType.StartWave });
                }

            }

            //Execute start fight skill queue
            else if (newStep.StepType == StepTypeEnm.Idle && currentFight?.CurrentWave != null && BeforeStartQueue.Count > 0)
            {
                newStep.ExecuteAbilityFromQueue();
            }

            //if we doing somethings then need proced the events

            CurrentStep = newStep;
            Steps.Add(CurrentStep);


            if (!CurrentStep.TriggersHandled)
            {
                CurrentStep.TriggersHandled = true;
                //call handlers
                foreach (Unit unit in CurrentStep.Parent.PartyTeam.Units)
                    unit.Fighter.StepHandlerProc.Invoke(CurrentStep);
                if (CurrentStep.Parent?.HostileTeam?.Units != null)
                    foreach (Unit unit in CurrentStep.Parent.HostileTeam.Units)
                        unit.Fighter.StepHandlerProc.Invoke(CurrentStep);
            }

            CurrentStep.ProcEvents(false,false);



            return CurrentStep;

        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

}
