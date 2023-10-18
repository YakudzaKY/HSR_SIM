﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static HSR_SIM_LIB.Ability;
using static HSR_SIM_LIB.Check;
using static HSR_SIM_LIB.Event;
using static HSR_SIM_LIB.Resource;
using static HSR_SIM_LIB.Step;
using static HSR_SIM_LIB.Unit;
using static HSR_SIM_LIB.Team;
using System.Xml.Linq;
using HSR_SIM_LIB.Fighters;

namespace HSR_SIM_LIB
{/// <summary>
/// Combat simulation class
/// </summary>
    public class SimCls
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
                    units=units.Concat(team.Units);
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
            Team team ;

            //main team
            team = new Team(this);
            team.BindUnits(GetCombatUnits(CurrentScenario.Party));
            team.TeamType = Team.TeamTypeEnm.UnitPack;
            team.controledTeam = true;
            Teams.Add(team);


            //Special
            team = new Team(this);
            team.BindUnits(GetCombatUnits(CurrentScenario.SpecialUnits));
            team.TeamType = Team.TeamTypeEnm.Special;
            Teams.Add(team);

            //enemy team
            team = new Team(this)
            {
                TeamType = Team.TeamTypeEnm.UnitPack
            };
            Teams.Add(team);


        }

        /// <summary>
        /// Check conditions are ok for use ability
        /// </summary>
        /// <param name="essence"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool FullFiledConditions(CheckEssence essence)
        {
            bool res = true;//Total search res 
            string nullReplacer = "%mandatory%";
            Dictionary<String, bool?> orGroupsRes = new Dictionary<String, bool?>();
            foreach (Condition condition in essence.ExecuteWhen)
            {
                bool condRes = true;
                bool? currentValue = null;
                List<KeyValuePair<String, bool?>> localSrch = new();


                localSrch.AddRange(orGroupsRes.Where(x => x.Key == (condition.OrGroup ?? nullReplacer)));
                //search exists keyval
                if (localSrch.Count == 0)
                {
                    orGroupsRes.Add(condition.OrGroup ?? nullReplacer, null);//default true
                }
                currentValue = orGroupsRes[condition.OrGroup ?? nullReplacer];
                //if some mandatory false then no reason to do other mandatory
                //OR if some ORGROUP true then no reason to do other checks from this group
                if ((condition.OrGroup == null && currentValue != false)
                  || (condition.OrGroup != null && currentValue != true))
                {
                    condRes = ExecuteCondition(condition, essence);
                    orGroupsRes[condition.OrGroup ?? nullReplacer] = condRes;
                }
                localSrch.Clear();
                localSrch = null;


            }
            //check results All groups should be TRUE
            foreach (KeyValuePair<String, bool?> checkRes in orGroupsRes)
            {
                if (checkRes.Value == false)
                {
                    res = false;
                    break;
                }
            }
            orGroupsRes.Clear();
            orGroupsRes = null;
            return res;

        }

        /// <summary>
        /// Execute every Check in condition. If one false then other false
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="essence"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool ExecuteCondition(Condition condition, CheckEssence essence)
        {
            bool res = true;
            foreach (Check check in condition.Checks)
            {
                res = ExecuteCheck(check, essence);
                if (!res)
                {
                    break;
                }
            }
            return res;
        }
        /// <summary>
        /// Execute one check
        /// </summary>
        /// <param name="check"></param>
        /// <param name="essence"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool ExecuteCheck(Check check, CheckEssence essence, Unit caster = null)
        {
            bool res = false;
            if (check.CheckType == CheckTypeEnm.CombatStartSkillQueue)
            {
                if (String.Equals(check.Value, TargetTypeEnm.Self.ToString(), StringComparison.OrdinalIgnoreCase))
                    res = (BeforeStartQueue.IndexOf(essence as Ability) >= 0);
                else
                    throw new NotImplementedException();

            }
            else if (check.CheckType == CheckTypeEnm.FindTarget)
            {
                TargetTypeEnm targetType = (TargetTypeEnm)System.Enum.Parse(typeof(TargetTypeEnm), check.Value,true);
                if (essence is Ability ability)
                {
                    res = ExecuteCheckList(check, new List<CheckEssence>(ability.Parent.GetTargets(targetType)), ability.Parent);
                }
                else if (essence is Unit unit)
                {
                    res = ExecuteCheckList(check, new List<CheckEssence>(unit.GetTargets(targetType)), unit);
                }
                else if (essence is Event)
                {
                    throw new NotImplementedException();
                    //res = ExecuteCheckList(check, new List<CheckEssence>(HostileParty), ((Event)essence).AbilityValue.Parent);
                }
                else
                    throw new NotImplementedException();


            }
            else if (check.CheckType == CheckTypeEnm.CheckTarget)
            {

                if (essence is Event @event)
                {
                    res = ExecuteCheckList(check, new List<CheckEssence>() { @event.TargetUnit }, @event.AbilityValue.Parent);
                }
                else
                    throw new NotImplementedException();




            }
            else if (check.CheckType == CheckTypeEnm.Alive)
            {
                res = ((Unit)essence).IsAlive == bool.Parse(check.Value);

            }
            else if (check.CheckType == CheckTypeEnm.HaveSkill)
            {
                res = ExecuteCheckList(check, new List<CheckEssence>(((Unit)essence).Fighter.Abilities));
            }
            else if (check.CheckType == CheckTypeEnm.AbilityType)
            {
                res = ((Ability)essence).AbilityType == (AbilityTypeEnm)System.Enum.Parse(typeof(AbilityTypeEnm), check.Value,true);
            }
            else if (check.CheckType == CheckTypeEnm.WeaknessType)
            {
                foreach (ElementEnm weakness in ((Unit)essence).Fighter.Weaknesses)
                {
                    ElementEnm? findVal;
                    if (check.Value == "@CasterElement")
                    {
                        findVal = caster.Fighter.Element;
                    }
                    else if (check.Value == "@PartyUnitElement")
                    {
                        findVal = caster.Fighter.Element;
                    }
                    else
                        findVal = (ElementEnm)System.Enum.Parse(typeof(ElementEnm), check.Value, true);

                    if (weakness == findVal)
                    {
                        res = true;
                        break;
                    }
                }
            }
            else if (check.CheckType == CheckTypeEnm.AbilityCostType)
            {
                res = ((Ability)essence).CostType == (ResourceType)System.Enum.Parse(typeof(ResourceType), check.Value, true);
            }
            else if (check.CheckType == CheckTypeEnm.AbilityCost)
            {
                res = ((Ability)essence).Cost >= int.Parse(check.Value);
            }
            else if (check.CheckType == CheckTypeEnm.HaveEvent)
            {
                res = ExecuteCheckList(check, new List<CheckEssence>(((Ability)essence).Events));
            }
            else if (check.CheckType == CheckTypeEnm.EventType)
            {
                res = ((Event)essence).Type == (EventType)System.Enum.Parse(typeof(EventType), check.Value, true);
            }
            else if (check.CheckType == CheckTypeEnm.ResourceCheck)
            {
                if (String.Equals(check.Value, "party", StringComparison.OrdinalIgnoreCase))
                {
                    res = ExecuteCheckList(check, new List<CheckEssence>(((Ability)essence).Parent.ParentTeam.Resources));
                }
                else
                    throw new NotImplementedException();

            }
            else if (check.CheckType == CheckTypeEnm.ResourceType)
            {
                res = ((Resource)essence).ResType == (ResourceType)System.Enum.Parse(typeof(ResourceType), check.Value, true);
            }
            else if (check.CheckType == CheckTypeEnm.ResourceQuantity)
            {
                res = ((Resource)essence).ResVal >= int.Parse(check.Value);
            }
            else
                throw new NotImplementedException();

            return check.Clause ? res : !res; ;
        }
        /// <summary>
        /// Search for only one essence with ALL checks
        /// </summary>
        /// <param name="check"></param>
        /// <param name="essences"></param>
        /// <param name="caster"></param>
        /// <returns></returns>
        private bool ExecuteCheckList(Check check, List<CheckEssence> essences, Unit caster = null)
        {
            bool res = false;
            //gather all essences
            foreach (CheckEssence essence in essences)
            {
                bool checkAreOk = true;

                foreach (Check innerCheck in check.InnerChecks)
                {
                    checkAreOk = ExecuteCheck(innerCheck, essence, caster);
                    //if one fail then all check false
                    if (!checkAreOk)
                        break;
                }

                //if find good item then stop search 
                if (checkAreOk)
                {
                    res = true;
                    break;
                }

            }
            return res;
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
                    newStep.Events.Add(new Event(newStep) { Type = EventType.StartWave });
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
                if (CurrentStep.Parent?.HostileTeam?.Units !=null)
                foreach (Unit unit in CurrentStep.Parent.HostileTeam.Units)
                    unit.Fighter.StepHandlerProc.Invoke(CurrentStep);
            }

            CurrentStep.ProcEvents();



            return CurrentStep;

        }


    }

}
