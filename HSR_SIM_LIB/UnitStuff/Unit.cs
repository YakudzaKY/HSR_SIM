using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Skills.EffectList;
using HSR_SIM_LIB.TurnBasedClasses.Events;
using HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Utils.Constant;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.Skills.Ability;

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

    public enum TypeEnm
    {
        Special,
        NPC,
        Character
    }

    private List<DamageBoostRec> baseDamageBoost; //Elemental damage boost list

    private IFighter fighter;
    public bool IsAlive = true;
    private Bitmap portrait;
    private List<Resource> resources;
    private UnitStats stats;


    public Team ParentTeam { get; set; } = null;

    public IFighter Fighter
    {
        get =>
            fighter ??= (IFighter)Activator.CreateInstance(Type.GetType(FighterClassName)!, this);
        set => fighter = value;
    }

    public Bitmap Portrait
    {
        get =>
            portrait ??= GraphicsCls.ResizeBitmap(Utl.LoadBitmap(UnitType + "\\" + Name), PortraitSize.Height,
                PortraitSize.Width);
        set => portrait = value;
    }

    public List<Buff> Buffs { get; set; } = new();

    public string Name { get; set; } = string.Empty;

    public UnitStats Stats
    {
        get => stats ??= new UnitStats(this);
        set => stats = value;
    }


    public Unit Reference { get; internal set; }
    public int Level { get; set; } = 1;

    public List<Resource> Resources
    {
        get => resources ??= new List<Resource>();
        set => resources = value;
    }


    public List<DamageBoostRec> BaseDamageBoost
    {
        get
        {
            return
                baseDamageBoost ??= new List<DamageBoostRec>();
        }
        set => baseDamageBoost = value;
    }

    public TypeEnm UnitType { get; set; }
    public List<Skill> Skills { get; set; } = new();

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
                List<Unit> nextEnemys = new();
                List<Unit> nextEnemysDistinct = new();
                //gather enemys from all waves
                if (ParentTeam.ParentSim.NextFight != null)
                    foreach (var wave in ParentTeam.ParentSim.NextFight.Waves)
                        nextEnemys.AddRange(wave.Units);

                //get distinct
                foreach (var unit in nextEnemys.DistinctBy(x => x.Name)) nextEnemysDistinct.Add(unit);

                return nextEnemysDistinct;
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
        get { return Buffs.Any(x => x.CrowdControl); }
    }

    public double CurrentEnergy
    {
        get => GetRes(ResourceType.Energy).ResVal;
        set => GetRes(ResourceType.Energy).ResVal = value;
    }

    /// <summary>
    ///     Prepare to combat
    /// </summary>
    public void InitToCombat()
    {
        GetRes(ResourceType.HP).ResVal = GetMaxHp(null);
        GetRes(ResourceType.Toughness).ResVal = Stats.MaxToughness;
        Fighter = null;
    }

    /// <summary>
    ///     Get resource By Type
    /// </summary>
    public Resource GetRes(ResourceType rt)
    {
        if (!Resources.Any(x => x.ResType == rt))
            Resources.Add(new Resource { ResType = rt, ResVal = 0 });
        return Resources.First(resource => resource.ResType == rt);
    }

    public double AllDmgBoost(Event ent = null)
    {
        return GetModsByType(typeof(EffAllDamageBoost), ent: ent);
    }

    public double AdditiveShieldBonus(Event ent = null)
    {
        return GetModsByType(typeof(EffAdditiveShieldBonus), ent: ent);
    }

    public double PrcShieldBonus(Event ent = null)
    {
        return GetModsByType(typeof(EffPrcShieldBonus), ent: ent);
    }

    public double ResistsPenetration(ElementEnm elem, Event ent = null)
    {
        return GetModsByType(typeof(EffElementalPenetration), elem, ent);
    }

    /// <summary>
    ///     https://honkai-star-rail.fandom.com/wiki/Damage_RES
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="targetForCondition"></param>
    /// <returns></returns>
    public double GetResists(ElementEnm elem, Event ent = null)
    {
        double res = 0;
        if (Fighter.Resists.Any(x => x.ResistType == elem))
            res += Fighter.Resists.First(x => x.ResistType == elem).ResistVal;

        res += GetModsByType(typeof(EffElementalResist), elem, ent);
        res += GetModsByType(typeof(EffAllDamageResist), ent: ent);
        return res;
    }

    public double GetDebuffResists(Type debuff, Event ent = null)
    {
        double res = 0;
        if (Fighter.DebuffResists.Any(x => x.Debuff == debuff))
            res += Fighter.DebuffResists.First(x => x.Debuff == debuff).ResistVal;

        res += GetModsByType(typeof(EffDebufResist), ent: ent);
        return res;
    }

    public double DotBoost(Event ent = null)
    {
        return GetModsByType(typeof(EffDoTBoost), ent: ent);
    }

    public double DefIgnore(Event ent = null)
    {
        return GetModsByType(typeof(EffDefIgnore), ent: ent);
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
        return GetElemBoost(elem).Value + GetModsByType(typeof(EffElementalBoost), elem, ent);
    }

    /// <summary>
    ///     Get total stat by Buffs by type
    /// </summary>
    /// <returns></returns>
    public double GetModsByType(Type srchBuffType, ElementEnm? elem = null, Event ent = null, List<ConditionBuff> excludeCondBuff = null, Buff.BuffType? buffType = null)
    {
        double res = 0;
        List<Buff> conditionsToCheck = new();
        //get all mods to check
        conditionsToCheck.AddRange(Buffs.Where(x => buffType is null || x.Type == buffType));
        foreach (var pmode in GetConditionBuffs(this, srchBuffType, excludeCondBuff).Where(x => buffType is null || x.AppliedBuff.Type == buffType))
            conditionsToCheck.Add(pmode.AppliedBuff);


        foreach (var mod in conditionsToCheck)
            foreach (var effect in mod.Effects.Where(y => y.GetType() == srchBuffType
                                                          && (y is not EffElementalTemplate eft || eft.Element == elem)
                                                          && (y is not EffAbilityTypeBoost efAbility ||
                                                              (ent?.AbilityValue != null && efAbility.AbilityType ==
                                                                  ent.AbilityValue.AbilityType))
                     )
                    )
            {
                double finalValue;
                if (effect.CalculateValue != null)
                    finalValue = (double)effect.CalculateValue(ent);
                else
                    finalValue = (double)effect.Value;

                //ent?.ParentStep.Parent.Parent?.LogDebug($"{buff.AbilityValue?.Name} {buff.Caster.Name} add to val = { finalValue}");
                res += finalValue * (effect.StackAffectValue ? mod.Stack : 1);
            }

        return res;
    }

    public IEnumerable<PassiveBuff> GetConditionBuffToUnit(List<PassiveBuff> passiveBuffs, List<ConditionBuff> conditionBuffs, Unit targetForBuff, Type effTypeToSearch, List<ConditionBuff> excludeCondBuff = null)
    {
        IEnumerable<PassiveBuff> res =
        (from cmod in passiveBuffs
                .Where(x => x.AppliedBuff.Effects.Any(x => effTypeToSearch == null || x.GetType() == effTypeToSearch))
                .Concat(conditionBuffs.Where(y => excludeCondBuff == null || !excludeCondBuff.Contains(y)))
         where ((cmod.Target is Unit && cmod.Target == this)
                || (cmod.Target is Team && cmod.Target == ParentTeam)
                || (cmod.Target is TargetTypeEnm && cmod.Parent.GetTargetsForUnit((TargetTypeEnm)cmod.Target).Contains(this)))
               && (cmod is not ConditionBuff mod || mod.Truly(targetForBuff, excludeCondBuff))
         select cmod);
        return res;
    }
    /// <summary>
    ///     search avalable for unit condition mods(planars self or ally)
    /// </summary>
    /// <returns></returns>
    public List<PassiveBuff> GetConditionBuffs(Unit targetForBuff, Type effTypeToSearch, List<ConditionBuff> excludeCondBuff = null)
    {
        List<PassiveBuff> res = new();
        if (ParentTeam == null)
            return res;


        foreach (var unit in ParentTeam.ParentSim.AllUnits.Where(x => x.IsAlive))
        {
            if (unit.fighter.ConditionBuffs.Concat(unit.fighter.PassiveBuffs).Any())
                res.AddRange(GetConditionBuffToUnit(unit.fighter.PassiveBuffs, unit.fighter.ConditionBuffs, targetForBuff, effTypeToSearch, excludeCondBuff));
            if (unit.Fighter is DefaultFighter unitFighter)
            {
                //LC
                if (unitFighter.LightCone != null)
                    res.AddRange(GetConditionBuffToUnit(unitFighter.LightCone.PassiveMods, unitFighter.LightCone.ConditionMods, targetForBuff, effTypeToSearch, excludeCondBuff));
                //GEAR
                foreach (var relic in unitFighter.Relics)
                    res.AddRange(GetConditionBuffToUnit(relic.PassiveMods, relic.ConditionMods, targetForBuff, effTypeToSearch, excludeCondBuff));
            }
        }


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

    /// <summary>
    ///     Search buff by ref. and add stack or reset duration.
    ///     If buff have no ref then search by itself
    ///     also search by SourceObject+effects
    ///
    /// return stacks applied
    /// </summary>
    /// <param name="buff"></param>
    /// <param name="abilityValue"></param>
    public int  ApplyBuff(Event ent, Buff buff)
    {
        int res = 0;
        var srchBuff = Buffs.FirstOrDefault(x =>
            ((x.Reference == (buff.Reference ?? buff) || (buff.SourceObject != null && buff.SourceObject == x.SourceObject))
             && (buff.UniqueUnit == null || x.UniqueUnit == buff.UniqueUnit))
            || (!string.IsNullOrEmpty(buff.UniqueStr) && string.Equals(x.UniqueStr, buff.UniqueStr)));


        foreach (var effect in buff.Effects) effect.BeforeApply(ent, buff);
        //find existing by ref, or by UNIQUE tag
        if (srchBuff != null)
        {
            //add stack
            int oldStacks = srchBuff.Stack;
            srchBuff.Stack = Math.Min(srchBuff.MaxStack, srchBuff.Stack + buff.Stack);
            res = srchBuff.Stack - oldStacks;
            if (srchBuff.EffectStackingType == Buff.EffectStackingTypeEnm.FullReplace)
            {
                srchBuff.Effects.Clear();
                srchBuff.Effects.AddRange(buff.Effects);
            }
            else
                //refresh effect values for bigger numbers
                foreach (Effect eff in buff.Effects)
                {
                    Effect findExistingEff = srchBuff.Effects.FirstOrDefault(x => eff.GetType() == x.GetType());
                    if (srchBuff.EffectStackingType == Buff.EffectStackingTypeEnm.PickMax)
                        findExistingEff.Value = Math.Max(findExistingEff.Value ?? 0, eff.Value ?? 0);
                    else if  (srchBuff.EffectStackingType == Buff.EffectStackingTypeEnm.Plus)
                        findExistingEff.Value+= eff.Value ?? 0;
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
        }
        else
        {
            //or add new
            if (!buff.DoNotClone)
            {
                //copy buff
                srchBuff = (Buff)buff.Clone();
                //if we have calced values then clone all effects(in the same order)
                if (buff.Effects.Any(x => x.DynamicValue))
                {
                    srchBuff.Effects = new List<Effect>();
                    //clone calced effects
                    foreach (Effect eff in buff.Effects)
                    {
                        if (eff.DynamicValue)
                            srchBuff.Effects.Add((Effect)eff.Clone());
                        else
                        {
                            srchBuff.Effects.Add(eff);
                        }
                    }
                }
                

            }

            else
                srchBuff = buff;
            srchBuff.Stack = Math.Min(srchBuff.MaxStack, srchBuff.Stack);
            res = srchBuff.Stack;
            srchBuff.Reference = buff.Reference ?? buff;
            srchBuff.Owner = this;
            Buffs.Add(srchBuff);
        }

        foreach (var effect in srchBuff.Effects) effect.OnApply(ent, srchBuff);
        //reset duration
        srchBuff.IsOld = false; //renew the flag
        srchBuff.DurationLeft = srchBuff.BaseDuration;
        return res;
    }

    /// <summary>
    ///     remove buff by ref
    /// </summary>
    /// <param name="mod"></param>
    public void RemoveBuff(Event ent, Buff mod)
    {
        if (Buffs.Any(x => x.Reference == (mod.Reference ?? mod)))
        {
            var foundBuff = Buffs.First(x => x.Reference == (mod.Reference ?? mod));
            foreach (var effect in foundBuff.Effects) effect.BeforeRemove(ent, foundBuff);
            Buffs.Remove(foundBuff);
            foreach (var effect in foundBuff.Effects) effect.OnRemove(ent, foundBuff);
        }
    }


    public void AddStack( Buff mod, int stackCount=1)
    {
        if (Buffs.Any(x => x.Reference == (mod.Reference ?? mod)))
        {
            var foundBuff = Buffs.First(x => x.Reference == (mod.Reference ?? mod));
            foundBuff.Stack += stackCount;
        }
    }

    public int GetStacks( Buff mod)
    {
        if (Buffs.Any(x => x.Reference == (mod.Reference ?? mod)))
        {
            var foundBuff = Buffs.First(x => x.Reference == (mod.Reference ?? mod));
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
    /// </summary>
    /// <param name="attackElem"></param>
    /// <param name="targetForCondition"></param>
    /// <returns></returns>
    public double GetElemVulnerability(ElementEnm attackElem, Event ent = null)
    {
        return GetModsByType(typeof(EffElementalVulnerability), attackElem, ent);
    }

    public double GetAllDamageVulnerability(Event ent = null)
    {
        return GetModsByType(typeof(EffAllDamageVulnerability), ent: ent);
    }

    public double GetDotVulnerability(Event ent = null)
    {
        return GetModsByType(typeof(EffDoTVulnerability), ent: ent);
    }

    /// <summary>
    ///     https://honkai-star-rail.fandom.com/wiki/DMG_Reduction
    /// </summary>
    /// <returns></returns>
    public double GetDamageReduction(Unit targetForCondition = null)
    {
        double res = 1;

        //MODS
        if (Buffs != null)
            foreach (var mod in Buffs)
                foreach (var effect in mod.Effects.Where(x => x is EffDamageReduction))
                    for (var i = 0; i < mod.Stack; i++)
                        res *= 1 - effect.Value ?? 0;
        //Condition
        foreach (var pmod in GetConditionBuffs(targetForCondition, typeof(EffDamageReduction)))
            foreach (var effect in pmod.AppliedBuff.Effects.Where(x => x is EffDamageReduction))
                for (var i = 0; i < pmod.AppliedBuff.Stack; i++)
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
    ///     ability by type damage amplifier
    /// </summary>
    /// <param name="entAbilityValue"></param>
    /// <param name="ent"></param>
    /// <returns></returns>
    public double GetAbilityTypeMultiplier(Event ent = null)
    {
        return GetModsByType(typeof(EffAbilityTypeBoost), ent: ent);
    }

    public double GetOutgoingHealMultiplier(Event ent)
    {
        return 1 + Stats.HealRate + GetModsByType(typeof(EffOutgoingHealingPrc), ent: ent);
    }

    public double GetIncomingHealMultiplier(Event ent)
    {
        return 1 + GetModsByType(typeof(EffIncomeHealingPrc), ent: ent);
    }

    public double GetCritRate(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return Stats.CritChance + GetModsByType(typeof(EffCritPrc), ent: ent, excludeCondBuff: excludeCondBuff);
    }

    public double GetCritDamage(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return Stats.CritDmg + GetModsByType(typeof(EffCritDmg), ent: ent, excludeCondBuff: excludeCondBuff);
    }

    public double GetMaxHp(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return Stats.BaseMaxHp * (1 + GetModsByType(typeof(EffMaxHpPrc), ent: ent, excludeCondBuff: excludeCondBuff) + Stats.MaxHpPrc) +
               GetModsByType(typeof(EffMaxHp), ent: ent, excludeCondBuff: excludeCondBuff) + Stats.MaxHpFix;
    }

    public double GetActionValue(Event ent)
    {
        return GetBaseActionValue(ent) *
               (1 - GetModsByType(typeof(EffAdvance), ent: ent) + GetModsByType(typeof(EffDelay), ent: ent)) -
               Stats.PerformedActionValue;
    }

    public double GetBaseActionValue(Event ent)
    {
        return GetInitialBaseActionValue(ent) - GetModsByType(typeof(EffReduceBAV), ent: ent);
    }

    public double GetHpPrc(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return GetRes(ResourceType.HP).ResVal / GetMaxHp(ent, excludeCondBuff);
    }

    public double GetSpeed(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return Stats.BaseSpeed * (1 + GetModsByType(typeof(EffSpeedPrc), ent: ent, excludeCondBuff: excludeCondBuff) -
                   GetModsByType(typeof(EffReduceSpdPrc), ent: ent, excludeCondBuff: excludeCondBuff) + Stats.SpeedPrc) +
               GetModsByType(typeof(EffSpeed), ent: ent, excludeCondBuff: excludeCondBuff) +
               Stats.SpeedFix;
    }

    private double GetInitialBaseActionValue(Event ent)
    {
        return Stats.LoadedBaseActionValue ?? 10000 / GetSpeed(ent);
    }


    public double GetEffectRes(Event ent)
    {
        return GetModsByType(typeof(EffEffectResPrc), ent: ent) + Stats.EffectResPrc + Stats.BaseEffectRes +
               GetModsByType(typeof(EffEffectRes), ent: ent);
    }

    public double GetEffectHit(Event ent)
    {
        return GetModsByType(typeof(EffEffectHitPrc), ent: ent) + Stats.EffectHitPrc + Stats.BaseEffectHit +
               GetModsByType(typeof(EffEffectHit), ent: ent);
    }

    public double EnergyRegenPrc(Event ent)
    {
        return 1 + GetModsByType(typeof(EffEnergyRatePrc), ent: ent) + Stats.BaseEnergyResPrc + Stats.BaseEnergyRes;
    }

    public double GetDef(Event ent)
    {
        return Stats.BaseDef * (1 + GetModsByType(typeof(EffDefPrc), ent: ent) + Stats.DefPrc) +
               GetModsByType(typeof(EffDef), ent: ent);
    }

    public double GetAggro(Event ent)
    {
        if (IsAlive)
            return Stats.BaseAggro * (1 + GetModsByType(typeof(EffBaseAgrroPrc), ent: ent)) *
                   (1 + GetModsByType(typeof(EffAgrroPrc), ent: ent));
        return 0;
    }

    public double GetAttack(Event ent)
    {
        return Stats.BaseAttack * (1 + GetModsByType(typeof(EffAtkPrc), ent: ent) + Stats.AttackPrc) +
               GetModsByType(typeof(EffAtk), ent: ent) + Stats.AttackFix;
    }

    public double GetBreakDmg(Event ent)
    {
        return GetModsByType(typeof(EffBreakDmgPrc), ent: ent) + Stats.BreakDmgPrc;
    }

    public record DamageBoostRec
    {
        public ElementEnm ElemType;
        public double Value;
    }
}