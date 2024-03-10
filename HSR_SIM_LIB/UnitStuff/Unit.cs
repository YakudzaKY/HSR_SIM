using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;
using HSR_SIM_LIB.Utils.Utils;
using static HSR_SIM_LIB.Utils.Constant;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.Skills.PassiveBuff;
using static HSR_SIM_LIB.Utils.Formula;

namespace HSR_SIM_LIB.UnitStuff;

/// <summary>
///     Unit class. Stats skills etc
/// </summary>
public class Unit : CloneClass
{
    public enum ElementEnm
    {
        None,
        Wind,
        Physical,
        Fire,
        Ice,
        Lightning,
        Quantum,
        Imaginary
    }

    public enum LivingStatusEnm
    {
        Alive,
        WaitingForFollowUp,
        Defeated
    }

    public enum TypeEnm
    {
        Special,
        Npc,
        Character
    }

    private List<DamageBoostRec> baseDamageBoost; //Elemental damage boost list

    private IFighter fighter;
    private Bitmap portrait;
    private List<Resource> resources;
    private UnitStats stats;

    public bool IsAlive => LivingStatus != LivingStatusEnm.Defeated;

    public LivingStatusEnm LivingStatus { get; set; } = LivingStatusEnm.Alive;

    public Team ParentTeam { get; set; }

    public IFighter Fighter
    {
        get =>
            fighter ??= (IFighter)Activator.CreateInstance(Type.GetType(FighterClassName, true)!, this);
        private set => fighter = value;
    }

    public Bitmap Portrait
    {
        get =>
            portrait ??= GraphicsCls.ResizeBitmap(Utl.LoadBitmap(UnitType + "\\" + Name), PortraitSize.Height,
                PortraitSize.Width);
        set => portrait = value;
    }

    public List<AppliedBuff> AppliedBuffs { get; set; } = [];

    //always active buffs
    public List<PassiveBuff> PassiveBuffs { get; set; } = [];

    public string Name { get; set; } = string.Empty;

    public string PrintName => ParentTeam is null ? "" : "[" + ParentTeam.Units.IndexOf(this) + "]" + Name;

    public UnitStats Stats
    {
        get => stats ??= new UnitStats(this);
        set => stats = value;
    }


    public int Level { get; set; } = 1;

    private List<Resource> Resources
    {
        get => resources ??= new List<Resource>();
        set => resources = value;
    }


    private List<DamageBoostRec> BaseDamageBoost
    {
        get
        {
            return
                baseDamageBoost ??= [];
        }
        set => baseDamageBoost = value;
    }

    public TypeEnm UnitType { get; set; }
    public List<Skill> Skills { get; private set; } = [];

    public Team EnemyTeam
    {
        get
        {
            return ParentTeam?.ParentSim.Teams
                .FirstOrDefault(x => x != ParentTeam && x.TeamType != Team.TeamTypeEnm.Special);
        }
    }

    /// <summary>
    ///     Enemies
    /// </summary>
    /// <returns></returns>
    public List<Unit> Enemies
    {
        get
        {
            //next fight units
            if (ParentTeam.ParentSim.CurrentFight == null)
            {
                List<Unit> nextEnemies = [];
                //gather enemies from all waves
                if (ParentTeam.ParentSim.NextFight != null)
                    foreach (var wave in ParentTeam.ParentSim.NextFight.Waves)
                        nextEnemies.AddRange(wave.Units);
                return nextEnemies.DistinctBy(x => x.Name).ToList();
            }

            //return first other team
            return ParentTeam.ParentSim.Teams
                .First(x => x != ParentTeam && x.TeamType != Team.TeamTypeEnm.Special).Units;
        }
    }

    /// <summary>
    ///     Get Friends List
    /// </summary>
    /// <returns></returns>
    public List<Unit> Friends => ParentTeam.Units;


    public int Rank { get; set; }

    /// <summary>
    ///     Init strings for class load after copying the object
    /// </summary>
    public string LightConeStringPath { get; set; }

    public int LightConeInitRank { get; set; }

    public List<KeyValuePair<string, int>> RelicsClasses { get; set; } = new();
    public string FighterClassName { get; set; }

    //if unit under control
    public bool Controlled
    {
        get { return AppliedBuffs.Any(x => x.CrowdControl); }
    }

    public double CurrentEnergy
    {
        get => GetRes(ResourceType.Energy).ResVal;
        set => GetRes(ResourceType.Energy).ResVal = value;
    }

    public override object Clone()
    {
        var newClone = (Unit)MemberwiseClone();
        //clear fighter
        newClone.fighter = null;
        //clone resources
        var oldRes = newClone.Resources;
        newClone.Resources = [];
        foreach (var res in oldRes) newClone.Resources.Add((Resource)res.Clone());

        //clone Buffs
        var oldBuffs = newClone.AppliedBuffs;
        newClone.AppliedBuffs = [];
        foreach (var res in oldBuffs) newClone.AppliedBuffs.Add((AppliedBuff)res.Clone());
        //clone Buffs
        var oldPassives = newClone.PassiveBuffs;
        newClone.PassiveBuffs = [];
        foreach (var res in oldPassives) newClone.PassiveBuffs.Add((PassiveBuff)res.Clone());

        //clone Skills
        var oldSkills = newClone.Skills;
        newClone.Skills = new List<Skill>();
        foreach (var res in oldSkills) newClone.Skills.Add((Skill)res.Clone());

        //clone BaseDamageBoost
        var oldBaseDmgBoost = newClone.BaseDamageBoost;
        newClone.BaseDamageBoost = [];
        foreach (var res in oldBaseDmgBoost) newClone.BaseDamageBoost.Add(res with { });

        //clone Stats
        //var oldStats = newClone.Stats;
        newClone.Stats = (UnitStats)newClone.Stats.Clone();


        return newClone;
    }

    /// <summary>
    ///     Prepare to combat
    /// </summary>
    public void Init()
    {
        GetRes(ResourceType.HP).ResVal = GetMaxHp(null);
        GetRes(ResourceType.Toughness).ResVal = Stats.MaxToughness;
        Fighter = null;
    }

    //call when unit enter battle
    public void OnEnteringBattle()
    {
        foreach (var ability in Fighter.Abilities) ability.OnEnteringBattle();
    }

    /// <summary>
    ///     Get resource By Type
    /// </summary>
    public Resource GetRes(ResourceType rt)
    {
        if (Resources.All(x => x.ResType != rt))
            Resources.Add(new Resource(this) { ResType = rt, ResVal = 0 });
        return Resources.First(resource => resource.ResType == rt);
    }

    public double AllDmgBoost(Event ent = null)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAllDamageBoost));
    }

    public double AdditiveShieldBonus(Event ent = null)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAdditiveShieldBonus));
    }

    public double PrcShieldBonus(Event ent = null)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffPrcShieldBonus));
    }

    public double ResistsPenetration(ElementEnm elem, Event ent = null)
    {
        return GetBuffSumByType(ent, typeof(EffElementalPenetration), elem: elem);
    }

    /// <summary>
    ///     https://honkai-star-rail.fandom.com/wiki/Damage_RES
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="ent"></param>
    /// <returns>double value: sum of resists effects</returns>
    public double GetResists(ElementEnm elem, Event ent = null)
    {
        double res = 0;
        if (Fighter.Resists.Any(x => x.ResistType == elem))
            res += Fighter.Resists.First(x => x.ResistType == elem).ResistVal;

        res += GetBuffSumByType(ent, typeof(EffElementalResist), elem: elem);
        res += GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAllDamageResist));
        return res;
    }

    public double GetDebuffResists(Type debuff, Event ent = null)
    {
        double res = 0;
        if (Fighter.DebuffResists.Any(x => x.Debuff == debuff))
            res += Fighter.DebuffResists.First(x => x.Debuff == debuff).ResistVal;

        res += GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffDebufResist));
        return res;
    }

    public double DotBoost(Event ent = null)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffDoTBoost));
    }

    public double DefIgnore(Event ent = null)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffDefIgnore));
    }

    /// <summary>
    ///     Get elem  boost
    /// </summary>
    /// <param name="elem"></param>
    /// <returns></returns>
    public DamageBoostRec GetElemBoost(ElementEnm elem)
    {
        if (BaseDamageBoost.All(x => x.ElemType != elem))
            BaseDamageBoost.Add(new DamageBoostRec { ElemType = elem, Value = 0 });
        return BaseDamageBoost.First(dmg => dmg.ElemType == elem);
    }

    public double GetElemBoostValue(ElementEnm elem, Event ent = null)
    {
        return GetElemBoost(elem).Value + GetBuffSumByType(ent, typeof(EffElementalBoost), elem: elem);
    }


    /// <summary>
    ///     Get List of buffs that affect Unit by criteria
    /// </summary>
    /// <param name="effTypeToSearch">Effect to search</param>
    /// <param name="elem">search by element (for example Elemental Damage boost)</param>
    /// <param name="ent">Event ref for calculation</param>
    /// <param name="excludeCondition"></param>
    /// <param name="buffType">Search by type: debuff,DoT,Buff</param>
    /// <param name="abilityType">search by ability type(for example Damage boost by ability)</param>
    /// <returns>List of buffs that affect Unit</returns>
    private List<KeyValuePair<Buff, List<Effect>>> GetBuffEffectsByType(Type effTypeToSearch,
        ElementEnm? elem = null, Event ent = null, List<Condition> excludeCondition = null,
        Buff.BuffType? buffType = null, AbilityTypeEnm? abilityType = null)
    {
        List<KeyValuePair<Buff, List<Effect>>> res = new();

        List<Buff> buffList = new();
        //get all mods to check
        buffList.AddRange(AppliedBuffs.Where(x => buffType is null || x.Type == buffType));
        foreach (var passiveBuff in GetPassivesForUnit(this, effTypeToSearch, excludeCondition, ent, buffType))
            buffList.Add(passiveBuff);

        foreach (var mod in buffList)
        {
            List<Effect> effectList = new();
            effectList.AddRange(mod.Effects.Where(y => y.GetType() == effTypeToSearch
                                                       && (y is not EffElementalTemplate eft || eft.Element == elem)
                                                       && (y is not EffAbilityTypeBoost efa ||
                                                           efa.AbilityType == abilityType)
            ));
            if (effectList.Count > 0) res.Add(new KeyValuePair<Buff, List<Effect>>(mod, effectList));
        }


        return res;
    }

    /// <summary>
    ///     the sum of stats whose effects we found using the criteria
    /// </summary>
    /// <param name="ent">Event ref for calculation</param>
    /// <param name="effTypeToSearch">Effect to search</param>
    /// <param name="outputEffects">List of effect(!) DONT RENAME THIS PARAM. Formula class search it</param>
    /// <param name="elem">search by element (for example Elemental Damage boost)</param>
    /// <param name="excludeCondition">Exclude passive buff for prevent recursion </param>
    /// <param name="buffType">Search by type: debuff,DoT,Buff</param>
    /// <param name="abilityType">search by ability type(for example Damage boost by ability)</param>
    /// <returns>double value(sum of effect values)</returns>
    public double GetBuffSumByType(Event ent = null,
        Type effTypeToSearch = null, List<EffectTraceRec> outputEffects = null, ElementEnm? elem = null,
        List<Condition> excludeCondition = null, Buff.BuffType? buffType = null,
        AbilityTypeEnm? abilityType = null)
    {
        double res = 0;
        var effList =
            GetBuffEffectsByType(effTypeToSearch, elem, ent, excludeCondition, buffType, abilityType);

        foreach (var kp in effList)
        foreach (var effect in kp.Value)
        {

            double finalValue;
            //if condition\passive buff is target check then recalculate value
            if (effect.CalculateValue != null && kp.Key is PassiveBuff { IsTargetCheck: true })
                finalValue = (double)effect.CalculateValue(ent);
            else
                finalValue = (double)effect.Value;
            //multiply by stack
           
            if (kp.Key is AppliedBuff appliedBuff && effect.StackAffectValue)
                finalValue *= appliedBuff.Stack;
            res += finalValue;
            if (outputEffects != null && outputEffects.All(x => x.TraceEffect != effect))
                outputEffects.Add( new EffectTraceRec(kp.Key,effect,finalValue));
        }


        return res;
    }


    /// <summary>
    ///     Get list of buffs that affect unit
    /// </summary>
    /// <param name="passiveBuffs">List of buffs(can be from other unit)</param>
    /// <param name="targetForBuff">Target who will be checked for every buff</param>
    /// <param name="effTypeToSearch">effect type to search</param>
    /// <param name="excludeCondition"></param>
    /// <param name="ent">reference to Event</param>
    /// <param name="buffType"></param>
    /// <returns></returns>
    private IEnumerable<PassiveBuff> GetPassivesForUnitByList(List<PassiveBuff> passiveBuffs, Unit targetForBuff,
        Type effTypeToSearch,
        List<Condition> excludeCondition, Event ent, Buff.BuffType? buffType)
    {
        var res =
            from passiveBuff in passiveBuffs
                .Where(z => (z.Type == buffType || buffType is null) &&
                            z.Effects.Any(y => effTypeToSearch == null || y.GetType() == effTypeToSearch)
                            && (excludeCondition == null || !excludeCondition.Contains(z.WorkCondition)))
            where passiveBuff.UnitIsAffected(this) && (passiveBuff.WorkCondition?.Truly(passiveBuff,targetForBuff, excludeCondition, ent)??true)
            select passiveBuff;
        return res;
    }

    /// <summary>
    ///     search passives affecting unit unit (planar sphere self or ally)
    /// </summary>
    /// <returns></returns>
    public List<PassiveBuff> GetPassivesForUnit(Unit targetForBuff, Type effTypeToSearch,
        List<Condition> excludeCondition, Event ent, Buff.BuffType? buffType)
    {
        List<PassiveBuff> res = [];
        if (ParentTeam == null)
            return res;

        foreach (var unit in ParentTeam.ParentSim.AllUnits.Where(x => x.IsAlive))
            if (unit.PassiveBuffs.Any())
                res.AddRange(GetPassivesForUnitByList(unit.PassiveBuffs,
                    targetForBuff, effTypeToSearch, excludeCondition, ent, buffType));

        return res;
    }

    public static Color GetColorByElem(ElementEnm? elem)
    {
        return elem switch
        {
            ElementEnm.Wind => Color.MediumSpringGreen,
            ElementEnm.Fire => Color.OrangeRed,
            ElementEnm.Ice => Color.LightSkyBlue,
            ElementEnm.Imaginary => Color.Yellow,
            ElementEnm.Physical => Color.WhiteSmoke,
            ElementEnm.Lightning => Color.Violet,
            ElementEnm.Quantum => Color.SlateBlue,

            _ => Color.Red
        };
    }


    //   restore the buff
    public void RestoreBuff(Event ent, AppliedBuff appliedBuff)
    {
        foreach (var effect in appliedBuff.Effects) effect.BeforeApply(ent, appliedBuff);
        AppliedBuffs.Add(appliedBuff);
        ResetCondition(Condition.ConditionCheckParam.Buff);
        if (appliedBuff.Type != Buff.BuffType.Buff) ResetCondition(Condition.ConditionCheckParam.AnyDebuff);
        foreach (var effect in appliedBuff.Effects) effect.OnApply(ent, appliedBuff);
    }

    public int ApplyBuff(Event ent, AppliedBuff appliedBuff, out AppliedBuff appliedAppliedBuff)
    {
        int res;
        var foundBuff = AppliedBuffs.FirstOrDefault(x =>
            ((x.Reference == (appliedBuff.Reference ?? appliedBuff) ||
              (appliedBuff.SourceObject != null && appliedBuff.SourceObject == x.SourceObject))
             && (appliedBuff.UniqueUnit == null || x.UniqueUnit == appliedBuff.UniqueUnit))
            || (!string.IsNullOrEmpty(appliedBuff.UniqueStr) && string.Equals(x.UniqueStr, appliedBuff.UniqueStr)));


        foreach (var effect in appliedBuff.Effects) effect.BeforeApply(ent, appliedBuff);
        //find existing by ref, or by UNIQUE tag
        if (foundBuff != null)
        {
            //add stack
            var oldStacks = foundBuff.Stack;
            foundBuff.Stack = Math.Min(foundBuff.MaxStack,
                foundBuff.Stack + (appliedBuff.CalculateStacks?.Invoke(ent) ?? appliedBuff.Stack));
            res = foundBuff.Stack - oldStacks;
            if (foundBuff.EffectStackingType == AppliedBuff.EffectStackingTypeEnm.FullReplace)
            {
                foundBuff.Effects.Clear();
                //clone calculated effects
                foreach (var eff in appliedBuff.Effects)
                    if (eff.DynamicValue)
                        foundBuff.Effects.Add((Effect)eff.Clone());
                    else
                        foundBuff.Effects.Add(eff);
            }
            else
                //refresh effect values for bigger numbers
            {
                foreach (var eff in appliedBuff.Effects)
                {
                    var findExistingEff = foundBuff.Effects.First(x => eff.GetType() == x.GetType());
                    if (foundBuff.EffectStackingType == AppliedBuff.EffectStackingTypeEnm.PickMax)
                        findExistingEff.Value = Math.Max(findExistingEff.Value ?? 0, eff.Value ?? 0);
                    else if (foundBuff.EffectStackingType == AppliedBuff.EffectStackingTypeEnm.Plus)
                        findExistingEff.Value += eff.Value ?? 0;
                    else
                        throw new NotImplementedException();
                }
            }
        }
        else
        {
            //copy buff
            foundBuff = (AppliedBuff)appliedBuff.Clone();
            foundBuff.Stack = Math.Min(foundBuff.MaxStack, foundBuff.Stack);
            res = foundBuff.Stack;
            foundBuff.Reference = appliedBuff.Reference ?? appliedBuff;
            foundBuff.CarrierUnit = this;
            AppliedBuffs.Add(foundBuff);
        }

        ResetCondition(Condition.ConditionCheckParam.Buff);
        if (appliedBuff.Type != Buff.BuffType.Buff) ResetCondition(Condition.ConditionCheckParam.AnyDebuff);
        foreach (var effect in foundBuff.Effects) effect.OnApply(ent, foundBuff);
        //reset duration
        foundBuff.IsOld = false; //renew the flag
        foundBuff.DurationLeft = foundBuff.BaseDuration;
        appliedAppliedBuff = foundBuff;
        return res;
    }

    /// <summary>
    ///     remove buff by ref. Return buff was found or not
    /// </summary>
    /// <param name="ent">Event reference</param>
    /// <param name="appliedBuff">buff reference</param>
    public AppliedBuff RemoveBuff(Event ent, AppliedBuff appliedBuff)
    {
        AppliedBuff res = null;
        if (AppliedBuffs.Any(x => x.Reference == (appliedBuff.Reference ?? appliedBuff)))
        {
            var foundBuff = AppliedBuffs.First(x => x.Reference == (appliedBuff.Reference ?? appliedBuff));
            foreach (var effect in foundBuff.Effects) effect.BeforeRemove(ent, foundBuff);
            AppliedBuffs.Remove(foundBuff);
            foreach (var effect in foundBuff.Effects) effect.OnRemove(ent, foundBuff);
            res = foundBuff;
        }

        ResetCondition(Condition.ConditionCheckParam.Buff);
        if (appliedBuff.Type != Buff.BuffType.Buff) ResetCondition(Condition.ConditionCheckParam.AnyDebuff);

        return res;
    }


    public void AddStack(AppliedBuff appliedBuff, int stackCount = 1)
    {
        if (AppliedBuffs.Any(x => x.Reference == (appliedBuff.Reference ?? appliedBuff)))
        {
            var foundBuff = AppliedBuffs.First(x => x.Reference == (appliedBuff.Reference ?? appliedBuff));
            foundBuff.Stack += stackCount;
        }
    }

    public int GetStacks(AppliedBuff appliedBuff)
    {
        if (AppliedBuffs.Any(x => x.Reference == (appliedBuff.Reference ?? appliedBuff)))
        {
            var foundBuff = AppliedBuffs.First(x => x.Reference == (appliedBuff.Reference ?? appliedBuff));
            return foundBuff.Stack;
        }

        return 0;
    }

    public IEnumerable<Unit> GetTargetsForUnit(TargetTypeEnm? targetType)
    {
        if (targetType == TargetTypeEnm.Friend)
            return Friends.Where(x => x.IsAlive);
        if (targetType == TargetTypeEnm.Enemy)
            return Enemies?.Where(x => x.IsAlive) ?? new List<Unit>();
        throw new NotImplementedException();
    }

    /// <summary>
    ///     https://honkai-star-rail.fandom.com/wiki/Vulnerability
    ///     get target vulnerability by element
    /// </summary>
    /// <param name="attackElem">element of attack</param>
    /// <param name="ent">reference to event where is calculation</param>
    /// <returns>double value:vulnerability</returns>
    public double GetElemVulnerability(ElementEnm attackElem, Event ent = null)
    {
        return GetBuffSumByType(ent, typeof(EffElementalVulnerability), elem: attackElem);
    }

    /// <summary>
    ///     https://honkai-star-rail.fandom.com/wiki/Vulnerability
    ///     get target vulnerability from all sources of damage
    /// </summary>
    /// <param name="ent">reference to event where is calculation</param>
    /// <returns>double value:vulnerability</returns>
    public double GetAllDamageVulnerability(Event ent = null)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAllDamageVulnerability));
    }

    /// <summary>
    ///     https://honkai-star-rail.fandom.com/wiki/Vulnerability
    ///     get target vulnerability to Damage over Time
    /// </summary>
    /// <param name="ent">reference to event where is calculation</param>
    /// <returns>double value:vulnerability</returns>
    public double GetDotVulnerability(Event ent = null)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffDoTVulnerability));
    }

    /// <summary>
    ///     https://honkai-star-rail.fandom.com/wiki/DMG_Reduction
    /// </summary>
    /// <param name="ent">reference to event where is calculation</param>
    /// <returns></returns>
    public double GetDamageReduction(Event ent = null)
    {
        double res = 1;

        //MODS
        if (AppliedBuffs != null)
            foreach (var mod in AppliedBuffs)
            foreach (var effect in mod.Effects.Where(x => x is EffDamageReduction))
                for (var i = 0; i < mod.Stack; i++)
                    res *= 1 - effect.Value ?? 0;
        //Condition
        foreach (var passiveBuff in GetPassivesForUnit(this, typeof(EffDamageReduction), null, ent, null))
        foreach (var effect in passiveBuff.Effects.Where(x => x is EffDamageReduction))
            for (var i = 0; i < passiveBuff.Stack; i++)
                res *= 1 - effect.Value ?? 0;
        return res;
    }

    /// <summary>
    ///     https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
    /// </summary>
    /// <returns></returns>
    public double GetBrokenMultiplier()
    {
        if (GetRes(ResourceType.Toughness).ResVal > 0)
            return 0.9;
        return 1;
    }

    /// <summary>
    ///     get  abilityDamage multiplier by ability type
    /// </summary>
    /// <param name="ent">reference to event</param>
    /// <param name="ability">reference to ability</param>
    /// <returns></returns>
    public double GetAbilityTypeMultiplier(Event ent, Ability ability)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAbilityTypeBoost),
                   abilityType: ability.AbilityType)
               // plus follow up bonus for non follow up attacks in follow up step
               + (ability.AbilityType != AbilityTypeEnm.FollowUpAction &&
                  ent.ParentStep.StepType == Step.StepTypeEnm.UnitFollowUpAction
                   ? GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAbilityTypeBoost),
                       abilityType: AbilityTypeEnm.FollowUpAction)
                   : 0);
    }

    public double GetOutgoingHealMultiplier(Event ent)
    {
        return 1 + Stats.HealRate + GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffOutgoingHealingPrc));
    }

    public double GetIncomingHealMultiplier(Event ent)
    {
        return 1 + GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffIncomeHealingPrc));
    }

     
    public double GetCritRate(Event ent, List<Condition> excludeCondition = null)
    {
        return Stats.CritChance +
               GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffCritPrc), excludeCondition: excludeCondition);
    }

    public double GetCritDamage(Event ent, List<Condition> excludeCondition = null)
    {
        return Stats.CritDmg +
               GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffCritDmg), excludeCondition: excludeCondition);
    }

    public double GetMaxHp(Event ent, List<Condition> excludeCondition = null)
    {
        return Stats.BaseMaxHp *
               (1 + GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffMaxHpPrc), excludeCondition: excludeCondition) +
                Stats.MaxHpPrc) +
               GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffMaxHp), excludeCondition: excludeCondition) +
               Stats.MaxHpFix;
    }

    public double GetCurrentActionValue(Event ent)
    {
        return GetActionValue(ent) - Stats.PerformedActionValue;
    }

    public double GetActionValue(Event ent)
    {
        return GetBaseActionValue(ent) *
               (1 - GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAdvance)) +
                GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffDelay)));
    }

    private double GetBaseActionValue(Event ent)
    {
        return GetInitialBaseActionValue(ent) - GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffReduceBAV));
    }

    public double GetHpPrc(Event ent, List<Condition> excludeCondition = null)
    {
        return GetRes(ResourceType.HP).ResVal / GetMaxHp(ent, excludeCondition);
    }

    public double GetSpeed(Event ent, List<Condition> excludeCondition = null)
    {
        return Stats.BaseSpeed *
               (1 + GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffSpeedPrc), excludeCondition: excludeCondition) +
                Stats.SpeedPrc) +
               GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffSpeed), excludeCondition: excludeCondition) +
               Stats.SpeedFix;
    }

    public void ResetCondition(Condition.ConditionCheckParam chkPrm)
    {
        foreach (var cb in PassiveBuffs.Where(x => x.WorkCondition?.ConditionParam == chkPrm)) cb.WorkCondition.NeedRecalculate = true;
    }

    private double GetInitialBaseActionValue(Event ent)
    {
        return Stats.LoadedBaseActionValue ?? 10000 / GetSpeed(ent);
    }


    public double GetEffectRes(Event ent)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffEffectResPrc)) + Stats.EffectResPrc +
               Stats.BaseEffectRes +
               GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffEffectRes));
    }

    public double GetEffectHit(Event ent)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffEffectHitPrc)) + Stats.EffectHitPrc +
               Stats.BaseEffectHit +
               GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffEffectHit));
    }

    public double EnergyRegenPrc(Event ent)
    {
        return 1 + GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffEnergyRatePrc)) + Stats.BaseEnergyResPrc +
               Stats.BaseEnergyRes;
    }

    public double GetDef(Event ent)
    {
        return Stats.BaseDef * (1 + GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffDefPrc)) -
                                ent.SourceUnit?.DefIgnore(ent) ??
                                0 + Stats.DefPrc) +
               GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffDef));
    }

    public double GetAggro(Event ent)
    {
        if (IsAlive)
            return Stats.BaseAggro * (1 + GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffBaseAgrroPrc))) *
                   (1 + GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAgrroPrc)));
        return 0;
    }

    public double GetAttack(Event ent)
    {
        return Stats.BaseAttack *
               (1 + GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAtkPrc)) + Stats.AttackPrc) +
               GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffAtk)) + Stats.AttackFix;
    }
   

    public double GetBreakDmg(Event ent)
    {
        return GetBuffSumByType(ent: ent, effTypeToSearch: typeof(EffBreakDmgPrc)) + Stats.BreakDmgPrc;
    }

    public List<ElementEnm> GetWeaknesses(Event ent, List<Condition> excludeCondition = null)
    {
        List<ElementEnm> res = new();
        //add native weakness to result
        res.AddRange(Fighter.NativeWeaknesses);
        //grab weakness from debuffs
        var elems = GetBuffEffectsByType(typeof(EffWeaknessImpair), ent: ent, excludeCondition: excludeCondition)
            .Select(x => x.Value);
        foreach (var lst in elems) res.AddRange(lst.Select(x => ((EffWeaknessImpair)x).Element));

        return res;
    }

    public record DamageBoostRec
    {
        public ElementEnm ElemType;
        public double Value;
    }
}