using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.TurnBasedClasses;
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
    }

    public SimCls ParentSim { get; set; }
    public TeamTypeEnm TeamType { get; set; }


    public List<Unit> Units { get; set; } = new();

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
                foreach (var resourceType in new[] { ResourceType.SP, ResourceType.TP })
                {
                    Resource res = new(this)
                    {
                        ResType = resourceType,
                        ResVal = 0
                    };
                    resources.Add(res);
                }
            }

            return resources;
        }
        set => resources = value;
    }

    public override object Clone()
    {
        var newClone = (Team)MemberwiseClone();
        //clone teams
        var oldUnits = newClone.Units;
        newClone.Units = new List<Unit>();
        foreach (var unit in oldUnits)
        {
            var newUnit = (Unit)unit.Clone();
            if (newUnit.ParentTeam != null)
                newUnit.ParentTeam = newClone;
            newClone.Units.Add(newUnit);
        }

        return newClone;
    }

    public void ResetRoles()
    {
        foreach (var unit in Units) unit.Fighter.Role = null;
    }


    /// <summary>
    ///     unbind unit from team
    /// </summary>
    public int UnBindUnit(Unit unit)
    {
        int ndx;
        ndx = Units.IndexOf(unit);
        unit.ParentTeam = null;
        Units.RemoveAt(ndx);
        return ndx;
    }


    /// <summary>
    ///     bind unit to team
    /// </summary>
    public void BindUnit(Unit unit, int ndx)
    {
        unit.ParentTeam = this;
        Units.Insert(ndx, unit);
    }

    /// <summary>
    ///     unbind units from team
    /// </summary>
    public void UnBindUnits()
    {
        var UnitsToUnbind = new List<Unit>();
        UnitsToUnbind.AddRange(Units);
        foreach (var unit in UnitsToUnbind) UnBindUnit(unit);
        //Units.Clear(); disable this clear coz need save team into event field
        //Units = null;
    }

    //bind units to team
    public void BindUnits(List<Unit> bindUnits)
    {
        foreach (var unit in bindUnits) BindUnit(unit, Units.Count);
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