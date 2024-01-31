using System;
using System.Collections.Generic;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.UnitStuff;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.UnitStuff.Unit;

namespace HSR_SIM_LIB.Skills;

/// <summary>
///     Ability class
/// </summary>
public class Ability : CloneClass
{
    public delegate bool DCanUsePrc();
    public enum AbilityCurrentTargetEnm
    {
        AbilityAdjacent,
        AbilityMain
    }

    public enum AbilityTypeEnm
    {
        None = 0,
        Basic = 1,
        Ability = 2,
        Ultimate = 4,
        Technique = 8,
        FollowUpAction = 16
    }

    public enum AdjacentTargetsEnm
    {
        Random,
        None,
        Blast,
        One,
        All
    }

    public enum PriorityEnm
    {
        DefeatHandler,//top 1
        High,
        Medium,
        Ultimate,
        Low
    }

    public enum TargetTypeEnm
    {
        Self,
        Friend,
        Enemy
    }


    public Ability(IFighter parent)
    {
        Parent = parent;
        if (Element == ElementEnm.None)
            Element = parent.Element;
        FollowUpQueueAvailable ??= DefaultAbilityQueueAvailable;
    }


    public ElementEnm Element { get; set; }

    public AbilityTypeEnm AbilityType { get; set; }

    public IFighter Parent { get; set; }

    public string Name { get; internal set; }
    public List<Event> Events { get; set; } = new();
    public double Cost { get; set; } = 0;
    public ResourceType? CostType { get; set; }
    public TargetTypeEnm TargetType { get; set; } = TargetTypeEnm.Enemy;
    public AdjacentTargetsEnm AdjacentTargets { get; set; } = AdjacentTargetsEnm.None;

    /// <summary>
    /// Technique attack shred toughness by default without dmg
    /// </summary>
    public bool Attack
    {
        get { return Events.Any(x => x is DirectDamage or ToughnessShred); }
    }
    public bool IgnoreWeakness { get; set; }

    public int Cooldown { get; init; } = 0;
    public int CooldownTimer { get; set; } = 0;
    public bool EndTheTurn { get; set; } = true; //If ability used - end the turn

    public PriorityEnm Priority { get; set; } = PriorityEnm.Low;

    public DCanUsePrc Available { get; init; } = DefaultAbilityAvailable;

    /// <summary>
    /// do we pref cast this or not
    /// </summary>
    public DCanUsePrc IWannaUseIt { get; init; } = DefaultAbilityWannaUse;
    public int SpGain { get; set; } = 0;
    public List<KeyValuePair<Unit,Unit>> FollowUpTargets { get; set; }=new ();// key=target unit ,value=Queued by unit
    public DCanUsePrc FollowUpQueueAvailable { get; init; } 

    //reset ability params
    public void OnEnteringBattle()
    {
        CooldownTimer = 0;
        foreach (Event ent in Events)
        {
            ent.OnEnteringBattle();
        }
    }


    //default all abilities are ok
    public static bool DefaultAbilityAvailable()
    {
        return true;
    }

    
    //default all abilities we wanna cast
    public static bool DefaultAbilityWannaUse()
    {
        return true;
    }


    
    //default followup queue available if target list empty
    public  bool DefaultAbilityQueueAvailable()
    {
        return !FollowUpTargets.Any();
    }

    //get targets by event from ability
    public IEnumerable<Unit> GetTargets(Unit target, TargetTypeEnm? eventTargetType,
        AbilityCurrentTargetEnm? currTargetType)
    {
        IEnumerable<Unit> res = null;

        if (eventTargetType == null)
            eventTargetType = TargetType;
        if (currTargetType == null)
        {
            if (AdjacentTargets == AdjacentTargetsEnm.All)
                currTargetType = AbilityCurrentTargetEnm.AbilityAdjacent;
            else
                currTargetType = AbilityCurrentTargetEnm.AbilityMain;
        }

        if (eventTargetType == TargetTypeEnm.Self)
        {
            if (currTargetType == AbilityCurrentTargetEnm.AbilityMain)
                res = new[] { Parent.Parent };
            else
                throw new NotImplementedException();
        }
        else
        {
            if (currTargetType == AbilityCurrentTargetEnm.AbilityMain)
            {
                res = new[] { target };
            }
            else if (currTargetType == AbilityCurrentTargetEnm.AbilityAdjacent)
            {
                if (eventTargetType == TargetTypeEnm.Friend)
                    res = GetAffectedTargets(Parent.Parent, true);
                else
                    res = GetAffectedTargets(target, true);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        return res;
    }

    /// <summary>
    ///     Get units affected by ability
    /// </summary>
    /// <returns></returns>
    public List<Unit> GetAffectedTargets(Unit unit, bool onlyAdjacent = false)
    {
        var res = new List<Unit>();
        if (AdjacentTargets == AdjacentTargetsEnm.Blast)
        {
            if (!onlyAdjacent)
                res.Add(unit);
            var party = new List<Unit>();
            party.AddRange(unit.GetTargetsForUnit(TargetTypeEnm.Friend));
            var leftTarget = party.LastOrDefault(x => party.IndexOf(x) < party.IndexOf(unit));
            if (leftTarget != null)
                res.Add(leftTarget);
            var rightTarget = party.FirstOrDefault(x => party.IndexOf(x) > party.IndexOf(unit));
            if (rightTarget != null)
                res.Add(rightTarget);
        }
        else if (AdjacentTargets == AdjacentTargetsEnm.One)
        {
            if (!onlyAdjacent)
                res.Add(unit);
            var party = new List<Unit>();
            party.AddRange(unit.GetTargetsForUnit(TargetTypeEnm.Friend));
            var leftTarget = party.LastOrDefault(x => party.IndexOf(x) < party.IndexOf(unit));
            if (leftTarget != null)
            {
                res.Add(leftTarget);
            }
            else
            {
                var rightTarget = party.FirstOrDefault(x => party.IndexOf(x) > party.IndexOf(unit));
                if (rightTarget != null)
                    res.Add(rightTarget);
            }
        }
        else if (AdjacentTargets == AdjacentTargetsEnm.All)
        {
            if (unit != null)
                res.AddRange(Parent.Parent.GetTargetsForUnit(TargetTypeEnm.Friend));
            else
                res.AddRange(Parent.Parent.GetTargetsForUnit(TargetTypeEnm.Enemy));
        }
        else
        {
            throw new NotImplementedException();
        }

        return res;
    }
}