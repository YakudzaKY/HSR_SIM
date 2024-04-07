using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HSR_SIM_LIB.Content;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;
using Microsoft.VisualBasic.CompilerServices;
using static HSR_SIM_LIB.Utils.Constant;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.Utils.Formula;

namespace HSR_SIM_LIB.UnitStuff;

/// <summary>
///     Unit class. Stats skills etc
/// </summary>
public class Unit : CloneClass
{
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

    /// <summary>
    ///     Is Elite flag. Need for some weakness break calculations
    /// </summary>
    public bool IsEliteUnit { get; set; }

    /// <summary>
    ///     native weaknesses defined by profile
    /// </summary>
    public List<ElementEnm> NativeWeaknesses { get; set; } 


    /// <summary>
    ///     native resists defined by profile
    /// </summary>
    public List<Resist> NativeResists { get; set; } 

    /// <summary>
    ///     native debuff resists defined by profile
    /// </summary>
    public List<DebuffResist> NativeDebuffResists { get; set; } 


    private List<DamageBoostRec> baseDamageBoost; //Elemental damage boost list

    private Bitmap portrait;
    private UnitStats stats;
    private IFighter fighter;

    public bool IsAlive => LivingStatus != LivingStatusEnm.Defeated;

    public LivingStatusEnm LivingStatus { get; set; } = LivingStatusEnm.Alive;

    public Team ParentTeam { get; set; }

    public IFighter Fighter
    {
        get
        {
            return fighter ??= (IFighter)Activator.CreateInstance(Type.GetType(FighterClassName, true)!, this);
        }
        set => fighter = value;
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
    public List<PassiveBuff> PassiveBuffs { get; private set; } = [];

    public string Name { get; set; } = string.Empty;

    public string PrintName => ParentTeam is null ? "" : "[" + ParentTeam.Units.IndexOf(this) + "]" + Name;

    public UnitStats Stats
    {
        get => stats ??= new UnitStats(this);
        set => stats = value;
    }


    public int Level { get; set; } = 1;

    private List<Resource> Resources { get; set; } = [];


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

    public double UnitLvlMultiplier => FighterUtils.LvlMultiplier[Level];

    public double BleedEliteMultiplier => IsEliteUnit ? 0.07 : 0.16;

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

    /// <summary>
    ///     flag that unit is NPC
    /// </summary>
    public bool IsNpcUnit { get; set; }

    public override object Clone()
    {
        var newClone = (Unit)MemberwiseClone();
        newClone.Fighter = null;
        
        //clone resources
        var oldRes = newClone.Resources;
        newClone.Resources = [];
        foreach (var res in oldRes)
        {
            Resource newRes =(Resource)res.Clone();
            newRes.Parent = newClone;
            newClone.Resources.Add(newRes);
        }

        //clone Buffs
        var oldBuffs = newClone.AppliedBuffs;
        newClone.AppliedBuffs = [];
        foreach (var res in oldBuffs) newClone.AppliedBuffs.Add((AppliedBuff)res.Clone());
        //clone Buffs
        var oldPassives = newClone.PassiveBuffs;
        newClone.PassiveBuffs = [];
        foreach (var res in oldPassives) newClone.PassiveBuffs.Add((PassiveBuff)res.Clone());
        //clone Debuff Resists
        var oldNativeDebuffResists= newClone.NativeDebuffResists;
        newClone.NativeDebuffResists = [];
        foreach (var res in oldNativeDebuffResists) newClone.NativeDebuffResists.Add(res);
        //clone  Resists
        var oldNativeResists= newClone.NativeResists;
        newClone.NativeResists = [];
        foreach (var res in oldNativeResists) newClone.NativeResists.Add(res);
        //clone  Weakness
        var oldNativeWeakness= newClone.NativeWeaknesses;
        newClone.NativeWeaknesses = [];
        foreach (var res in oldNativeWeakness) newClone.NativeWeaknesses.Add(res);
        
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
        newClone.Stats.Parent = newClone;


        return newClone;
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
            Resources.Add(new Resource(this) { ResType = rt, ResVal = GetDefaultRes(rt) });
        return Resources.First(resource => resource.ResType == rt);
    }

    /// <summary>
    /// Set default value when resource got created
    /// </summary>
    /// <param name="rt">resource type</param>
    /// <returns></returns>
    private double GetDefaultRes(ResourceType rt)
    {
        switch (rt)
        {
            case ResourceType.HP:
                return this.MaxHp().Result;
            case ResourceType.Toughness:
                return Stats.MaxToughness;
            default:
                return 0;
            
        }
    }

    public double GetResVal(ResourceType rt, List<FormulaBuffer.DependencyRec> dependencyRecs = null,
        DynamicTargetEnm unitToCheck = DynamicTargetEnm.Attacker)
    {
        if (dependencyRecs != null)
            FormulaBuffer.MergeDependency(dependencyRecs,
                new FormulaBuffer.DependencyRec()
                    { Relation = unitToCheck, Stat = Condition.ConditionCheckParam.Resource });
        return GetRes(rt).ResVal;
    }


    public double GetNativeResists(ElementEnm elem)
    {
        
        double res = 0;
        if (NativeResists.Any(x => x.ResistType == elem))
            res += NativeResists.First(x => x.ResistType == elem).ResistVal;

        return res;
    }


    public double GetNativeDebuffResists(Type debuff)
    {
        double res = 0;
        if (NativeDebuffResists.Any(x => x.Debuff == debuff))
            res += NativeDebuffResists.First(x => x.Debuff == debuff).ResistVal;

        return res;
    }


    /// <summary>
    ///     Get elem  boost
    /// </summary>
    /// <param name="elem"></param>
    /// <returns></returns>
    public DamageBoostRec GetBaseElemBoost(ElementEnm elem)
    {
        if (BaseDamageBoost.All(x => x.ElemType != elem))
            BaseDamageBoost.Add(new DamageBoostRec { ElemType = elem, Value = 0 });
        return BaseDamageBoost.First(dmg => dmg.ElemType == elem);
    }

    public double GetBaseElemBoostVal(ElementEnm elem)
    {
        return GetBaseElemBoost(elem).Value;
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
    public List<KeyValuePair<Buff, List<Effect>>> GetBuffEffectsByType(Type effTypeToSearch,
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
                                                           efa.AbilityType.HasFlag(abilityType ??
                                                               throw new ArgumentNullException(nameof(abilityType))))
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
    /// <param name="outputEffects">List of effect used for val calculation</param>
    /// <param name="elem">search by element (for example Elemental Damage boost)</param>
    /// <param name="excludeCondition">Exclude passive buff for prevent recursion </param>
    /// <param name="buffType">Search by type: debuff,DoT,Buff</param>
    /// <param name="abilityType">search by ability type(for example Damage boost by ability)</param>
    /// <param name="dependencyRecs">List of dependencies</param>
    /// <param name="unitToCheck">unit for dependencyRecs param</param>
    /// <returns>double value(sum of effect values)</returns>
    public double GetBuffSumByType(Event ent = null,
        Type effTypeToSearch = null, List<EffectTraceRec> outputEffects = null, ElementEnm? elem = null,
        List<Condition> excludeCondition = null, Buff.BuffType? buffType = null,
        AbilityTypeEnm? abilityType = null, List<FormulaBuffer.DependencyRec> dependencyRecs = null,
        Formula.DynamicTargetEnm unitToCheck = DynamicTargetEnm.Attacker)
    {
        //save dependency from formula
        if (dependencyRecs != null && effTypeToSearch != null)
        {
            Effect instance = (Effect)Activator.CreateInstance(effTypeToSearch);

            FormulaBuffer.MergeDependency(dependencyRecs,
                new FormulaBuffer.DependencyRec()
                    { Relation = unitToCheck, Stat = instance!.ResetDependency });
        }

        double res = 0;
        var effList =
            GetBuffEffectsByType(effTypeToSearch, elem, ent, excludeCondition, buffType, abilityType);
        foreach (var kp in effList)
        foreach (var effect in kp.Value)
        {
            double finalValue = 0;
            //if condition\passive buff is target check then recalculate value
            if (effect.CalculateValue != null && kp.Key is PassiveBuff { IsTargetCheck: true })
            {
                if (effect.CalculateValue is Formula fm)
                {
                    effect.CalculateValue = (Formula)fm.Clone();
                    var newFrm = (Formula)effect.CalculateValue;
                    newFrm.EventRef = ent;
                    finalValue = newFrm.Result;
                }

                else if (effect.CalculateValue is Func<Event, Formula> fnc)
                {
                    effect.CalculateValue = fnc.Invoke(ent);
                    var newFrm = (Formula)effect.CalculateValue;
                    newFrm.EventRef = ent;
                    finalValue = newFrm.Result;
                }
            }
            else
                finalValue = (double)effect.Value;
            //multiply by stack

            if (kp.Key is AppliedBuff appliedBuff && effect.StackAffectValue)
                finalValue *= appliedBuff.Stack;
            res += finalValue;
            if (outputEffects != null && outputEffects.All(x => x.TraceEffect != effect))
                outputEffects.Add(new EffectTraceRec(kp.Key, effect, finalValue));
        }


        return res;
    }

    /// <summary>
    ///     the multiply of stats whose effects we found using the criteria
    /// 1 * (1- X)*(1-Y)...*(1-N)
    /// </summary>
    /// <param name="ent">Event ref for calculation</param>
    /// <param name="effTypeToSearch">Effect to search</param>
    /// <param name="outputEffects">List of effect used for val calculation</param>
    /// <param name="elem">search by element (for example Elemental Damage boost)</param>
    /// <param name="excludeCondition">Exclude passive buff for prevent recursion </param>
    /// <param name="buffType">Search by type: debuff,DoT,Buff</param>
    /// <param name="abilityType">search by ability type(for example Damage boost by ability)</param>
    /// <param name="dependencyRecs">List of dependencies</param>
    /// <param name="unitToCheck">unit for dependencyRecs param</param>
    /// <returns>double value(sum of effect values)</returns>
    public double GetBuffMultiplyByType(Event ent = null,
        Type effTypeToSearch = null, List<EffectTraceRec> outputEffects = null, ElementEnm? elem = null,
        List<Condition> excludeCondition = null, Buff.BuffType? buffType = null,
        AbilityTypeEnm? abilityType = null, List<FormulaBuffer.DependencyRec> dependencyRecs = null,
        Formula.DynamicTargetEnm unitToCheck = DynamicTargetEnm.Attacker)
    {
        double res = 1;
        var effList =
            GetBuffEffectsByType(effTypeToSearch, elem, ent, excludeCondition, buffType, abilityType);

        foreach (var kp in effList)
        foreach (var effect in kp.Value)
        {
            double finalValue = 0;
            //if condition\passive buff is target check then recalculate value
            if (effect.CalculateValue != null && kp.Key is PassiveBuff { IsTargetCheck: true })
            {
                if (effect.CalculateValue is Formula fm)
                {
                    effect.CalculateValue = (Formula)fm.Clone();
                    var newFrm = (Formula)effect.CalculateValue;
                    newFrm.EventRef = ent;
                    finalValue = newFrm.Result;
                }

                else if (effect.CalculateValue is Func<Event, Formula> fnc)
                {
                    effect.CalculateValue = fnc.Invoke(ent);
                    var newFrm = (Formula)effect.CalculateValue;
                    newFrm.EventRef = ent;
                    finalValue = newFrm.Result;
                }
            }
            else
                finalValue = (double)effect.Value;
            //multiply by stack

            if (kp.Key is AppliedBuff appliedBuff && effect.StackAffectValue)
                finalValue *= appliedBuff.Stack;
            res *= (1 - finalValue);
            if (outputEffects != null && outputEffects.All(x => x.TraceEffect != effect))
                outputEffects.Add(new EffectTraceRec(kp.Key, effect, finalValue));
            if (dependencyRecs != null && effect.ResetDependency != null)
                FormulaBuffer.MergeDependency(dependencyRecs,
                    new FormulaBuffer.DependencyRec()
                        { Relation = unitToCheck, Stat = (Condition.ConditionCheckParam)effect.ResetDependency });
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
                            && (excludeCondition == null || z.ApplyConditions is null ||
                                z.ApplyConditions.Any(cd => !excludeCondition.Contains(cd))))
            where passiveBuff.UnitIsAffected(this) &&
                  (passiveBuff.Truly(passiveBuff, targetForBuff, excludeCondition, ent))
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
            ((x.Reference == (appliedBuff.Reference ?? appliedBuff)
              || (!string.IsNullOrEmpty(appliedBuff.UniqueStr) && string.Equals(x.UniqueStr, appliedBuff.UniqueStr)))));


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
    ///     https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
    /// </summary>
    /// <returns></returns>
    public double GetBrokenMultiplier(List<FormulaBuffer.DependencyRec> dependencyRecs = null,
        DynamicTargetEnm unitToCheck = DynamicTargetEnm.Attacker)
    {
        if (dependencyRecs != null)
            FormulaBuffer.MergeDependency(dependencyRecs,
                new FormulaBuffer.DependencyRec()
                    { Relation = unitToCheck, Stat = Condition.ConditionCheckParam.Resource });

        if (GetRes(ResourceType.Toughness).ResVal > 0)
            return 0.9;
        return 1;
    }


    /// <summary>
    /// Reset Conditions that depends on stat
    /// </summary>
    /// <param name="chkPrm"></param>
    public void ResetCondition(object chkPrm)
    {
        if (chkPrm is Condition.ConditionCheckParam cprm)
            foreach (var cb in PassiveBuffs.Where(x => x.ApplyConditions != null)
                         .SelectMany(x => x.ApplyConditions.Where(y => y.ConditionParam == cprm)))
                cb.NeedRecalculate = true;
        ParentTeam?.ParentSim.CalcBuffer.Reset(this, chkPrm);
    }


    public List<ElementEnm> GetWeaknesses(Event ent, List<Condition> excludeCondition = null,
        List<FormulaBuffer.DependencyRec> dependencyRecs = null,
        DynamicTargetEnm unitToCheck = DynamicTargetEnm.Attacker)
    {
        if (dependencyRecs != null)
            FormulaBuffer.MergeDependency(dependencyRecs,
                new FormulaBuffer.DependencyRec()
                    { Relation = unitToCheck, Stat = Condition.ConditionCheckParam.Weakness });
        List<ElementEnm> res = new();
        //add native weakness to result
        res.AddRange(NativeWeaknesses);
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