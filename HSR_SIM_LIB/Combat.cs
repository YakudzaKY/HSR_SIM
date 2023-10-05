using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace HSR_SIM_LIB
{/// <summary>
/// Combat simulation class
/// </summary>
    public class Combat
    {
        Scenario currentScenario;

        int currentStep=0;
        int currentFightStep = 0;
        int sp = 0;//Skill points
        int tp = 0;// tehcnique pounts

        public int CurrentStep { get => currentStep; set => currentStep = value; }
        internal Scenario CurrentScenario { get => currentScenario; set => currentScenario = value; }
        public List<CombatUnit> Party { get => party; set => party = value; }
        internal CombatFight CurrentFight { get => currentFight; set => currentFight = value; }
        public int Tp { get => tp; set => tp = value; }
        public int Sp { get => sp; set => sp = value; }

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

        public int CurrentFightStep { get => currentFightStep; set => currentFightStep = value; }

        List<CombatUnit> party;

        CombatFight currentFight=null;
        
        /// <summary>
        /// construcotor
        /// </summary>
        public Combat()
        {
            init();
        }

        /// <summary>
        /// initialization
        /// </summary>
        private void init()
        {
            CurrentStep = 0;
            CurrentFight = null;

        }


        /// <summary>
        /// Convert unit list to combat units list
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        public List<CombatUnit> getCombatUnits(List<Unit> units)
        {
            List<CombatUnit> res= new List<CombatUnit> ();
            foreach (Unit unit in units)
            {   
                res.Add(new CombatUnit(unit));
            }
            return res;
        }

        /// <summary>
        /// prepare things to combat simulation
        /// </summary>
        public void Prepare()
        {
            init();
            Party = getCombatUnits(CurrentScenario.Party);
            Tp= Constant.MaxTp;

            //clone scenario party into current party

        }
    }
}
