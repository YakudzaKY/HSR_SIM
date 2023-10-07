using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static HSR_SIM_LIB.Resource;

namespace HSR_SIM_LIB
{/// <summary>
/// Combat simulation class
/// </summary>
    public class SimCls
    {
        Scenario currentScenario;

        Step currentStep =null;
        int currentFightStep = 0;
        private List<Resource> resources= null;


        List<Unit> party;

        SimFight currentFight = null;


        public Step CurrentStep { get => currentStep; set => currentStep = value; }
        internal Scenario CurrentScenario { get => currentScenario; set => currentScenario = value; }
        public List<Step> steps= new List<Step>();
        public List<Step> Steps { get => steps; set => steps = value; }
        public List<Unit> Party { get => party; set => party = value; }
        internal SimFight CurrentFight { get => currentFight; set => currentFight = value; }
        public int CurrentFightStep { get => currentFightStep; set => currentFightStep = value; }
        /// <summary>
        /// Do enter combat on next step proc
        /// </summary>
        public bool DoEnterCombat { get; internal set; }
       
        public List<Resource> Resources {
            get 
            {//create all resources
                if (resources == null)
                {
                    resources = new List<Resource>();
                    foreach (string name in Enum.GetNames<ResourceType>())
                    {
                        Resource res =new Resource();
                        res.ResType = (ResourceType)System.Enum.Parse(typeof(ResourceType), name);
                        res.ResVal = 0;
                        resources.Add(res);
                    }
                } 
                return resources;
            }
            set => resources = value; }

        private List<Ability> beforeStartQueue = new List<Ability>();
        public List<Ability> BeforeStartQueue { get => beforeStartQueue; set => beforeStartQueue = value; }

        private Fight nextFight;
        public Fight NextFight { 
            get
            {
                //try search next fight
                if (nextFight == null)
                {
                    if (CurrentFightStep < CurrentScenario.Fights.Count)
                        nextFight= CurrentScenario.Fights[CurrentFightStep + 1];
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
        /// Convert unit list to combat units list
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        public List<Unit> getCombatUnits(List<Unit> units)
        {
            List<Unit> res = new List<Unit>(units);
            foreach (Unit unit in res)
            {
                unit.InitToCombat();               
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
    }
}
