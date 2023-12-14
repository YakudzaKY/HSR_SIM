using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.UnitStuff.Resource;

namespace HSR_SIM_LIB.UnitStuff;

public class Team : CloneClass
{
    public enum TeamTypeEnm
    {
        UnitPack,
        Special
    }

    public bool controledTeam;
    private List<Resource> resources;

    public Team(SimCls parent)
    {
        ParentSim = parent;
        GetRes(ResourceType.TP).ResVal = Constant.MaxTp;
    }

    public void ResetRoles()
    {
        foreach (Unit unit in this.Units)
        {
            unit.Fighter.Role = null;
        }
    }
    public SimCls ParentSim { get; set; }
    public TeamTypeEnm TeamType { get; set; }


    public List<Unit> Units { get; private set; }

    public double TeamAggro
    {
        get
        {
            double res = 0;
            foreach (var unit in Units.Where(x => x.IsAlive))
                res += unit.GetAggro(null);
            return res;
        }
    }

    public List<Resource> Resources
    {
        get
        {
            //create all resources
            if (resources == null)
            {
                resources = new List<Resource>();
                foreach (var name in Enum.GetNames<ResourceType>())
                {
                    Resource res = new()
                    {
                        ResType = (ResourceType)Enum.Parse(typeof(ResourceType), name, true),
                        ResVal = 0
                    };
                    resources.Add(res);
                }
            }

            return resources;
        }
        set => resources = value;
    }

    /// <summary>
    ///     unbind units from team
    /// </summary>
    public void UnBindUnits()
    {
        foreach (var unit in Units) unit.ParentTeam = null;
        //Units.Clear(); disable this clear coz need save team into event field
        Units = null;
    }

    //bind units to team
    public void BindUnits(List<Unit> bindUnits)
    {
        Units = bindUnits;
        foreach (var unit in Units) unit.ParentTeam = this;
    }

    /// <summary>
    ///     Party have res to cast ability
    ///     if res not found then false
    /// </summary>
    /// <returns></returns>
    public bool HaveRes(Ability ability)
    {
        foreach (var res in Resources)
            if (res.ResType == ability.CostType)
                return res.ResVal >= ability.Cost;
        return false;
    }

    public Resource GetRes(ResourceType rt)
    {
        return Resources.Where(resource => resource.ResType == rt).First();
    }
}