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
using static HSR_SIM_LIB.Skills.ConditionBuff;

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
        if (Resources.All(x => x.ResType != rt))
            Resources.Add(new Resource(this) { ResType = rt, ResVal = 0 });
        return Resources.First(resource => resource.ResType == rt);
    }

    public double AllDmgBoost(Event ent = null)
    {
        return GetBuffSumByType(typeof(EffAllDamageBoost), ent: ent);
    }

    public double AdditiveShieldBonus(Event ent = null)
    {
        return GetBuffSumByType(typeof(EffAdditiveShieldBonus), ent: ent);
    }

    public double PrcShieldBonus(Event ent = null)
    {
        return GetBuffSumByType(typeof(EffPrcShieldBonus), ent: ent);
    }

    public double ResistsPenetration(ElementEnm elem, Event ent = null)
    {
        return GetBuffSumByType(typeof(EffElementalPenetration), elem, ent);
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

        res += GetBuffSumByType(typeof(EffElementalResist), elem, ent);
        res += GetBuffSumByType(typeof(EffAllDamageResist), ent: ent);
        return res;
    }

    public double GetDebuffResists(Type debuff, Event ent = null)
    {
        double res = 0;
        if (Fighter.DebuffResists.Any(x => x.Debuff == debuff))
            res += Fighter.DebuffResists.First(x => x.Debuff == debuff).ResistVal;

        res += GetBuffSumByType(typeof(EffDebufResist), ent: ent);
        return res;
    }

    public double DotBoost(Event ent = null)
    {
        return GetBuffSumByType(typeof(EffDoTBoost), ent: ent);
    }

    public double DefIgnore(Event ent = null)
    {
        return GetBuffSumByType(typeof(EffDefIgnore), ent: ent);
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
        return GetElemBoost(elem).Value + GetBuffSumByType(typeof(EffElementalBoost), elem, ent);
    }


    /// <summary>
    ///     Get total stat by Buffs by type
    /// </summary>
    /// <returns></returns>
    public List<KeyValuePair<Buff, List<Effect>>> GetBuffEffectsByType(Type srchBuffType, ElementEnm? elem = null, Event ent = null, List<ConditionBuff> excludeCondBuff = null, Buff.BuffType? buffType = null)
    {
        List<KeyValuePair<Buff, List<Effect>>> res = new();

        List<Buff> buffList = new();
        //get all mods to check
        buffList.AddRange(Buffs.Where(x => buffType is null || x.Type == buffType));
        foreach (var pmode in GetConditionBuffs(this, srchBuffType, excludeCondBuff, ent: ent).Where(x => buffType is null || x.AppliedBuff.Type == buffType))
            buffList.Add(pmode.AppliedBuff);

        foreach (var mod in buffList)
        {
            List<Effect> effectList = new();
            effectList.AddRange(mod.Effects.Where(y => y.GetType() == srchBuffType
                                                       && (y is not EffElementalTemplate eft || eft.Element == elem)
                                                       && (y is not EffAbilityTypeBoost efAbility ||
                                                           (ent?.ParentStep.ActorAbility != null && efAbility.AbilityType ==
                                                               ent.ParentStep.ActorAbility.AbilityType))));
            if (effectList.Count > 0)
            {
                res.Add(new KeyValuePair<Buff, List<Effect>>(mod, effectList));
            }
        }



        return res;
    }

    /// <summary>
    ///     Get total stat by Buffs by type
    /// </summary>
    /// <returns></returns>
    public double GetBuffSumByType(Type srchBuffType, ElementEnm? elem = null, Event ent = null, List<ConditionBuff> excludeCondBuff = null, Buff.BuffType? buffType = null)
    {
        double res = 0;
        List<KeyValuePair<Buff, List<Effect>>> effList = GetBuffEffectsByType(srchBuffType, elem, ent, excludeCondBuff, buffType);

        foreach (var kp in effList)
        {
            foreach (var effect in kp.Value)
            {
                double finalValue;
                if (effect.CalculateValue != null)
                    finalValue = (double)effect.CalculateValue(ent);
                else
                    finalValue = (double)effect.Value;

                res += finalValue * (effect.StackAffectValue ? kp.Key.Stack : 1);
            }
        }


        return res;
    }



    public IEnumerable<PassiveBuff> GetConditionBuffToUnit(List<PassiveBuff> passiveBuffs, List<ConditionBuff> conditionBuffs, Unit targetForBuff, Type effTypeToSearch, List<ConditionBuff> excludeCondBuff = null, Event ent = null)
    {
        IEnumerable<PassiveBuff> res =
        (from cmod in passiveBuffs
                .Where(x => x.AppliedBuff.Effects.Any(y => effTypeToSearch == null || y.GetType() == effTypeToSearch))
                .Concat(conditionBuffs.Where(z => z.AppliedBuff.Effects.Any(y => effTypeToSearch == null || y.GetType() == effTypeToSearch)
                                                  && (excludeCondBuff == null || !excludeCondBuff.Contains(z))))
         where cmod.UnitIsAffected(this) && (cmod is not ConditionBuff mod || mod.Truly(targetForBuff, excludeCondBuff, ent: ent))
         select cmod);
        return res;
    }
    /// <summary>
    ///     search avalable for unit condition mods(planars self or ally)
    /// </summary>
    /// <returns></returns>
    public List<PassiveBuff> GetConditionBuffs(Unit targetForBuff, Type effTypeToSearch, List<ConditionBuff> excludeCondBuff = null, Event ent = null)
    {
        List<PassiveBuff> res = new();
        if (ParentTeam == null)
            return res;

        foreach (var unit in ParentTeam.ParentSim.AllUnits.Where(x => x.IsAlive))
        {
            if (unit.fighter.ConditionBuffs.Concat(unit.fighter.PassiveBuffs).Any())
                res.AddRange(GetConditionBuffToUnit(unit.fighter.PassiveBuffs, unit.fighter.ConditionBuffs, targetForBuff, effTypeToSearch, excludeCondBuff, ent: ent));
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
    public int ApplyBuff(Event ent, Buff buff)
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
            else
                //refresh effect values for bigger numbers
                foreach (Effect eff in buff.Effects)
                {
                    Effect findExistingEff = srchBuff.Effects.FirstOrDefault(x => eff.GetType() == x.GetType());
                    if (srchBuff.EffectStackingType == Buff.EffectStackingTypeEnm.PickMax)
                        findExistingEff.Value = Math.Max(findExistingEff.Value ?? 0, eff.Value ?? 0);
                    else if (srchBuff.EffectStackingType == Buff.EffectStackingTypeEnm.Plus)
                        findExistingEff.Value += eff.Value ?? 0;
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

            else
                srchBuff = buff;
            srchBuff.Stack = Math.Min(srchBuff.MaxStack, srchBuff.Stack);
            res = srchBuff.Stack;
            srchBuff.Reference = buff.Reference ?? buff;
            srchBuff.Owner = this;
            Buffs.Add(srchBuff);
        }
        this.ResetCondition(ConditionBuff.ConditionCheckParam.Buff);
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
        this.ResetCondition(ConditionBuff.ConditionCheckParam.Buff);
    }


    public void AddStack(Buff mod, int stackCount = 1)
    {
        if (Buffs.Any(x => x.Reference == (mod.Reference ?? mod)))
        {
            var foundBuff = Buffs.First(x => x.Reference == (mod.Reference ?? mod));
            foundBuff.Stack += stackCount;
        }
    }

    public int GetStacks(Buff mod)
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
        return GetBuffSumByType(typeof(EffElementalVulnerability), attackElem, ent);
    }

    public double GetAllDamageVulnerability(Event ent = null)
    {
        return GetBuffSumByType(typeof(EffAllDamageVulnerability), ent: ent);
    }

    public double GetDotVulnerability(Event ent = null)
    {
        return GetBuffSumByType(typeof(EffDoTVulnerability), ent: ent);
    }

    /// <summary>
    ///     https://honkai-star-rail.fandom.com/wiki/DMG_Reduction
    /// </summary>
    /// <returns></returns>
    public double GetDamageReduction(Unit targetForCondition = null, Event ent = null)
    {
        double res = 1;

        //MODS
        if (Buffs != null)
            foreach (var mod in Buffs)
                foreach (var effect in mod.Effects.Where(x => x is EffDamageReduction))
                    for (var i = 0; i < mod.Stack; i++)
                        res *= 1 - effect.Value ?? 0;
        //Condition
        foreach (var pmod in GetConditionBuffs(targetForCondition, typeof(EffDamageReduction), ent: ent))
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
        return GetBuffSumByType(typeof(EffAbilityTypeBoost), ent: ent);
    }

    public double GetOutgoingHealMultiplier(Event ent)
    {
        return 1 + Stats.HealRate + GetBuffSumByType(typeof(EffOutgoingHealingPrc), ent: ent);
    }

    public double GetIncomingHealMultiplier(Event ent)
    {
        return 1 + GetBuffSumByType(typeof(EffIncomeHealingPrc), ent: ent);
    }

    public double GetCritRate(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return Stats.CritChance + GetBuffSumByType(typeof(EffCritPrc), ent: ent, excludeCondBuff: excludeCondBuff);
    }

    public double GetCritDamage(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return Stats.CritDmg + GetBuffSumByType(typeof(EffCritDmg), ent: ent, excludeCondBuff: excludeCondBuff);
    }

    public double GetMaxHp(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return Stats.BaseMaxHp * (1 + GetBuffSumByType(typeof(EffMaxHpPrc), ent: ent, excludeCondBuff: excludeCondBuff) + Stats.MaxHpPrc) +
               GetBuffSumByType(typeof(EffMaxHp), ent: ent, excludeCondBuff: excludeCondBuff) + Stats.MaxHpFix;
    }

    public double GetActionValue(Event ent)
    {
        return GetBaseActionValue(ent) *
               (1 - GetBuffSumByType(typeof(EffAdvance), ent: ent) + GetBuffSumByType(typeof(EffDelay), ent: ent)) -
               Stats.PerformedActionValue;
    }

    public double GetBaseActionValue(Event ent)
    {
        return GetInitialBaseActionValue(ent) - GetBuffSumByType(typeof(EffReduceBAV), ent: ent);
    }

    public double GetHpPrc(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return GetRes(ResourceType.HP).ResVal / GetMaxHp(ent, excludeCondBuff);
    }

    public double GetSpeed(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        return Stats.BaseSpeed * (1 + GetBuffSumByType(typeof(EffSpeedPrc), ent: ent, excludeCondBuff: excludeCondBuff) + Stats.SpeedPrc) +
               GetBuffSumByType(typeof(EffSpeed), ent: ent, excludeCondBuff: excludeCondBuff) +
               Stats.SpeedFix;
    }

    public void ResetCondition(ConditionCheckParam chkPrm)
    {
        foreach (ConditionBuff cb in this.Fighter.ConditionBuffs.Where(x => x.Condition.CondtionParam == chkPrm))
        {
            cb.NeedRecalc = true;
        }
    }
    private double GetInitialBaseActionValue(Event ent)
    {
        return Stats.LoadedBaseActionValue ?? 10000 / GetSpeed(ent);
    }


    public double GetEffectRes(Event ent)
    {
        return GetBuffSumByType(typeof(EffEffectResPrc), ent: ent) + Stats.EffectResPrc + Stats.BaseEffectRes +
               GetBuffSumByType(typeof(EffEffectRes), ent: ent);
    }

    public double GetEffectHit(Event ent)
    {
        return GetBuffSumByType(typeof(EffEffectHitPrc), ent: ent) + Stats.EffectHitPrc + Stats.BaseEffectHit +
               GetBuffSumByType(typeof(EffEffectHit), ent: ent);
    }

    public double EnergyRegenPrc(Event ent)
    {
        return 1 + GetBuffSumByType(typeof(EffEnergyRatePrc), ent: ent) + Stats.BaseEnergyResPrc + Stats.BaseEnergyRes;
    }

    public double GetDef(Event ent)
    {
        return Stats.BaseDef * (1 + GetBuffSumByType(typeof(EffDefPrc), ent: ent) - ent.SourceUnit?.DefIgnore(ent) ?? 0 + Stats.DefPrc) +
               GetBuffSumByType(typeof(EffDef), ent: ent);
    }

    public double GetAggro(Event ent)
    {
        if (IsAlive)
            return Stats.BaseAggro * (1 + GetBuffSumByType(typeof(EffBaseAgrroPrc), ent: ent)) *
                   (1 + GetBuffSumByType(typeof(EffAgrroPrc), ent: ent));
        return 0;
    }

    public double GetAttack(Event ent)
    {
        return Stats.BaseAttack * (1 + GetBuffSumByType(typeof(EffAtkPrc), ent: ent) + Stats.AttackPrc) +
               GetBuffSumByType(typeof(EffAtk), ent: ent) + Stats.AttackFix;
    }

    public double GetBreakDmg(Event ent)
    {
        return GetBuffSumByType(typeof(EffBreakDmgPrc), ent: ent) + Stats.BreakDmgPrc;
    }

    public List<Unit.ElementEnm> GetWeaknesses(Event ent, List<ConditionBuff> excludeCondBuff = null)
    {
        List<Unit.ElementEnm> res = new();
        //add native weakness to result
        res.AddRange(Fighter.NativeWeaknesses);
        //grab weakness from debuffs
        var elems = GetBuffEffectsByType(typeof(EffWeaknessImpair), ent: ent, excludeCondBuff: excludeCondBuff).Select(x => x.Value);
        foreach (var Lst in elems)
        {
            res.AddRange(Lst.Select(x => ((EffWeaknessImpair)x).Element));
        }
        return res;
    }

    public record DamageBoostRec
    {
        public ElementEnm ElemType;
        public double Value;
    }
}