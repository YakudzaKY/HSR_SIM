using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.Fighters;
using static HSR_SIM_LIB.UnitStuff.Resource;

namespace HSR_SIM_LIB.UnitStuff;

public class Team(SimCls parent) : CloneClass
{
    public enum TeamTypeEnm
    {
        UnitPack,
        Special
    }

    public bool ControlledTeam;
    private List<Resource> resources;

    public SimCls ParentSim { get; set; } = parent;
    public TeamTypeEnm TeamType { get; set; }

    public string Name => (ControlledTeam ? "Player: " : "") + TeamType;
    public List<Unit> Units { get; set; } = [];

    public double TeamAggro
    {
        get { return Units.Where(x => x.IsAlive).Sum(unit => unit.Aggro(ent:null).Result); }
    }

    public List<Resource> Resources
    {
        get
        {
            //create all resources
            if (resources != null) return resources;
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
        var oldRes = newClone.Resources;
        newClone.Resources = [];
        foreach (var res in oldRes)
        {
            Resource newRes =(Resource)res.Clone();
            newRes.Parent = newClone;
            newClone.Resources.Add(newRes);
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
        unit.Fighter = null;
        Units.RemoveAt(ndx);
        ParentSim.CalcBuffer.Reset(unit);
        return ndx;
    }


    /// <summary>
    ///     bind unit to team
    /// </summary>
    public void BindUnit(Unit unit, int ndx)
    {
        unit.ParentTeam = this;
        Units.Insert(ndx, unit);
        unit.Init();

    }

    /// <summary>
    ///     unbind units from team
    /// </summary>
    public void UnBindUnits()
    {
        var unitsToUnbind = new List<Unit>();
        unitsToUnbind.AddRange(Units);
        foreach (var unit in unitsToUnbind) UnBindUnit(unit);
        //Units.Clear(); disable this clear coz need save team into event field
        //Units = null;
    }

    //bind units to team
    public void BindUnits(List<Unit> bindUnits)
    {
        foreach (var unit in bindUnits) BindUnit(unit, Units.Count);
    }


    public Resource GetRes(ResourceType rt)
    {
        return Resources.First(resource => resource.ResType == rt);
    }
}