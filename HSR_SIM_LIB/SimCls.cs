using System;
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

namespace HSR_SIM_LIB
{/// <summary>
/// Combat simulation class
/// </summary>
    public class SimCls
    {
        Scenario currentScenario;

        Step currentStep = null;
        int currentFightStep = 0;
        private List<Resource> resources = null;


        List<Unit> hostileParty;
        List<Unit> party;

        CombatFight currentFight = null;

        /// <summary>
        /// Return from scenario if no fight. or hostileParty if fight active
        /// </summary>
        public List<Unit> HostileParty
        {
            get
            {
                if (CurrentFight == null)
                {
                    List<Unit> nextEnemys = new List<Unit>();
                    List<Unit> nextEnemysDistinct = new List<Unit>();
                    //gather enemys from all waves
                    foreach (Wave wave in nextFight.Waves)
                    {
                        nextEnemys.AddRange(wave.Units);

                    }
                    //get distinct
                    foreach (Unit unit in nextEnemys.DistinctBy(x => x.Name))
                    {
                        nextEnemysDistinct.Add(unit);
                    }
                    return nextEnemysDistinct;
                }

                else
                {
                    if (hostileParty == null)
                        hostileParty = new List<Unit>();
                    return hostileParty;
                }

            }
            set => hostileParty = value;
        }

        public Step CurrentStep { get => currentStep; set => currentStep = value; }
        internal Scenario CurrentScenario { get => currentScenario; set => currentScenario = value; }
        public List<Step> steps = new List<Step>();
        public List<Step> Steps { get => steps; set => steps = value; }
        public List<Unit> Party { get => party; set => party = value; }
        internal CombatFight CurrentFight { get => currentFight; set => currentFight = value; }
        public int CurrentFightStep { get => currentFightStep; set => currentFightStep = value; }
        /// <summary>
        /// Do enter combat on next step proc
        /// </summary>
        public bool DoEnterCombat { get; internal set; }

        public List<Resource> Resources
        {
            get
            {//create all resources
                if (resources == null)
                {
                    resources = new List<Resource>();
                    foreach (string name in Enum.GetNames<ResourceType>())
                    {
                        Resource res = new Resource();
                        res.ResType = (ResourceType)System.Enum.Parse(typeof(ResourceType), name);
                        res.ResVal = 0;
                        resources.Add(res);
                    }
                }
                return resources;
            }
            set => resources = value;
        }

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
        public List<Unit> getCombatUnits(List<Unit> units)
        {
            List<Unit> res = new List<Unit>();
            foreach (Unit unit in units)
            {
                Unit newUnit = (Unit)unit.Clone();
                res.Add(newUnit);
                newUnit.InitToCombat();
            }
            return res;
        }

        /// <summary>
        /// Get resource By Type
        /// </summary>
        public Resource GetRes(ResourceType rt)
        {
            return Resources.Where(resource => resource.ResType == rt).First();
        }

        /// <summary>
        /// prepare things to combat simulation
        /// </summary>
        public void Prepare()
        {

            Party = getCombatUnits(CurrentScenario.Party);

            GetRes(ResourceType.TP).ResVal = 5;
            GetRes(ResourceType.SP).ResVal = 5;



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
            foreach (Condition condition in ((Ability)essence).ExecuteWhen)
            {
                bool condRes = true;
                bool? currentValue = null;
                List<KeyValuePair<String, bool?>> localSrch = new List<KeyValuePair<string, bool?>>();


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
                if (String.Equals(check.Value, "self", StringComparison.OrdinalIgnoreCase))
                    res = (BeforeStartQueue.IndexOf(essence as Ability) >= 0);
                else
                    throw new NotImplementedException();

            }
            else if (check.CheckType == CheckTypeEnm.FindTarget)
            {
                if (String.Equals(check.Value, "party", StringComparison.OrdinalIgnoreCase))
                    res = ExecuteCheckList(check, new List<CheckEssence>(Party), ((Ability)essence).Parent);
                else if (String.Equals(check.Value, "Hostiles", StringComparison.OrdinalIgnoreCase))
                {
                    if (essence is Ability)
                    {
                        res = ExecuteCheckList(check, new List<CheckEssence>(HostileParty), ((Ability)essence).Parent);
                    }
                    else if (essence is Unit)
                    {
                        res = ExecuteCheckList(check, new List<CheckEssence>(HostileParty), ((Unit)essence));
                    }
                    else
                        throw new NotImplementedException();

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
                res = ExecuteCheckList(check, new List<CheckEssence>(((Unit)essence).Abilities));
            }
            else if (check.CheckType == CheckTypeEnm.AbilityType)
            {
                res = ((Ability)essence).AbilityType == (AbilityTypeEnm)System.Enum.Parse(typeof(AbilityTypeEnm), check.Value);
            }
            else if (check.CheckType == CheckTypeEnm.WeaknessType)
            {
                foreach (ElementEnm weakness in ((Unit)essence).Weaknesses)
                {
                    ElementEnm? findVal;
                    if (check.Value == "@CasterElement")
                    {
                        findVal = caster.Element;
                    }
                    else if (check.Value == "@PartyUnitElement")
                    {
                        findVal = caster.Element;
                    }
                    else
                        findVal = (ElementEnm)System.Enum.Parse(typeof(ElementEnm), check.Value);

                    if (weakness == findVal)
                    {
                        res = true;
                        break;
                    }
                }
            }
            else if (check.CheckType == CheckTypeEnm.AbilityCostType)
            {
                res = ((Ability)essence).CostType == (ResourceType)System.Enum.Parse(typeof(ResourceType), check.Value);
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
                res = ((Event)essence).Type == (EventType)System.Enum.Parse(typeof(EventType), check.Value);
            }
            else if (check.CheckType == CheckTypeEnm.ResourceCheck)
            {
                if (String.Equals(check.Value, "party", StringComparison.OrdinalIgnoreCase))
                {
                    res = ExecuteCheckList(check, new List<CheckEssence>(Resources));
                }
                else
                    throw new NotImplementedException();

            }
            else if (check.CheckType == CheckTypeEnm.ResourceType)
            {
                res = ((Resource)essence).ResType == (ResourceType)System.Enum.Parse(typeof(ResourceType), check.Value);
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
            Step newStep = new Step(this);
            if (CurrentStep == null)
            {
                //simulation preparations
                Prepare();
                newStep.StepType = StepTypeEnm.SimInit;
            }
            //buff before fight
            if (newStep.StepType == StepTypeEnm.Iddle && DoEnterCombat == false && CurrentFight == null) newStep.TechniqueWork();

            //enter the combat
            if (newStep.StepType == StepTypeEnm.Iddle && DoEnterCombat == true) newStep.LoadBattleWork();

            //load the wave
            if (currentFight != null && currentFight.CurrentWave == null)
            {
                //fight is over
                if (currentFight.CurrentWaveCnt == currentFight.ReferenceFight.Waves.Count)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    newStep.StepType = StepTypeEnm.StartWave;
                    newStep.Events.Add(new Event { Type = EventType.StartWave });
                }

            }

            //if we doing somethings then need proced the events
            if (newStep.StepType != StepTypeEnm.Iddle)
            {
                CurrentStep = newStep;
                Steps.Add(CurrentStep);
                newStep.ProcEvents();
            }

            return newStep;

        }


    }

}
