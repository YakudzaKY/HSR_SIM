using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.Resource;

namespace HSR_SIM_LIB
{
    public class Team
    {
        private List<Resource> resources;
        public SimCls ParentSim { get; set; }
        public bool controledTeam;
        private List<Unit> units = null;
        public TeamTypeEnm TeamType { get; set; }


        public List<Unit> Units
        {
            get => units;
        }

        public Team(SimCls parent)
        {
            ParentSim=parent;
            GetRes(ResourceType.TP).ResVal = 5;
            GetRes(ResourceType.SP).ResVal = 5;
            
        }
        /// <summary>
        /// unbind units from team
        /// </summary>
        public void UnBindUnits()
        {
            foreach (var unit in Units)
            {
                unit.ParentTeam = null;
            }
            Units.Clear();
            units=null;
        }

        //bind units to team
        public void BindUnits(List<Unit> bindUnits)
        {
            units=bindUnits;
            foreach (var unit in Units)
            {
                unit.ParentTeam = this;
            }
            
        }

        /// <summary>
        /// Party have res to cast ability
        /// if res not found then false
        /// </summary>
        /// <returns></returns>
        public bool HaveRes(Ability ability)
        {
            foreach (Resource res in Resources)
            {
                if (res.ResType == ability.CostType)
                    return res.ResVal >= ability.Cost;
            }
            return false;
        }

        public Resource GetRes(ResourceType rt)
        {
            return Resources.Where(resource => resource.ResType == rt).First();
        }
        public List<Resource> Resources
        {
            get
            {//create all resources
                if (resources == null)
                {
                    resources = new List<Resource>();
                    foreach (string name in Enum.GetNames<ResourceType>())
                    {
                        Resource res = new()
                        {
                            ResType = (ResourceType)System.Enum.Parse(typeof(ResourceType), name, true),
                            ResVal = 0
                        };
                        resources.Add(res);
                    }
                }
                return resources;
            }
            set => resources = value;
        }

        public enum TeamTypeEnm
        {
            UnitPack,
            Special

        }
    }
}
