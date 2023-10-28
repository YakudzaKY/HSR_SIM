using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HSR_SIM_LIB.Fighters;
using HSR_SIM_LIB.Fighters.LightCones;
using static HSR_SIM_LIB.Utils.Constant;
using static HSR_SIM_LIB.UnitStuff.Resource;
using static HSR_SIM_LIB.Skills.Ability;
using static HSR_SIM_LIB.UnitStuff.Unit;
using HSR_SIM_LIB.TurnBasedClasses;
using HSR_SIM_LIB.Skills;
using HSR_SIM_LIB.Utils;
using HSR_SIM_LIB.Fighters.Relics;
using static HSR_SIM_LIB.Skills.Mod;
using static HSR_SIM_LIB.Skills.Effect;
using System.Reflection.Emit;
using HSR_SIM_LIB.TurnBasedClasses.Events;

namespace HSR_SIM_LIB.UnitStuff
{
    /// <summary>
    /// Unit class. Stats skills etc
    /// </summary>
    public class Unit : CloneClass
    {
        public Team ParentTeam { get; set; } = null;
        string name = string.Empty;
        int level = 1;
        Bitmap portrait = null;
        UnitStats stats = null;
        public bool IsAlive = true;//TODO учесть сброс всех бафов и триггеров когда сдыхает, чтоб доты не аффектились бафами

        public record DamageBoostRec
        {
            public ElementEnm ElemType;
            public double Value;
        }

        private List<DamageBoostRec> baseDamageBoost;//Elemental damage boost list

        private IFighter fighter = null;
        public IFighter Fighter
        {
            get =>
                fighter ??= ((IFighter)Activator.CreateInstance(Type.GetType(FighterClassName)!, this));
            set => fighter = value;
        }

        public Bitmap Portrait
        {
            get =>
                portrait ??= GraphicsCls.ResizeBitmap(Utl.LoadBitmap(UnitType.ToString() + "\\" + Name), PortraitSize.Height, PortraitSize.Width);
            set => portrait = value;
        }
        public List<Mod> Mods { get; set; } = new List<Mod>();
        private List<Resource> resources = null;

        public string Name { get => name; set => name = value; }
        public UnitStats Stats
        {
            get => stats ??= new UnitStats(this);
            set => stats = value;
        }


        public Unit Reference { get; internal set; }
        public int Level { get => level; set => level = value; }

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


        public Unit()
        {

        }
        public List<Skill> Skills { get; set; } = new List<Skill>();

        /// <summary>
        /// Prepare to combat
        /// </summary>
        public void InitToCombat()
        {

            GetRes(ResourceType.HP).ResVal = Stats.MaxHp;
            GetRes(ResourceType.Toughness).ResVal = Stats.MaxToughness;
            Fighter = null;

        }
        /// <summary>
        /// Get resource By Type
        /// </summary>
        public Resource GetRes(ResourceType rt)
        {
            if (!Resources.Any(x => x.ResType == rt))
                Resources.Add(new Resource() { ResType = rt, ResVal = 0 });
            return Resources.First(resource => resource.ResType == rt);
        }

        public double AllDmgBoost(Event ent = null)
        {
            return GetModsByType(EffectType.AllDamageBoost, ent: ent);
        }

        public double AdditiveShieldBonus(Event ent = null)
        {
            return GetModsByType(EffectType.AdditiveShieldBonus, ent: ent);
        }

        public double PrcShieldBonus(Event ent = null)
        {
            return GetModsByType(EffectType.PrcShieldBonus, ent: ent);
        }

        public double ResistsPenetration(ElementEnm elem, Event ent = null)
        {
            return GetModsByType(EffectType.ElementalPenetration, elem, ent: ent);
        }

        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/Damage_RES
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="targetForCondition"></param>
        /// <returns></returns>
        public double GetResists(ElementEnm elem, Event ent = null)
        {
            double res = 0;
            if (Fighter.Resists.Any(x => x.ResistType == elem))
            {
                res += Fighter.Resists.First(x => x.ResistType == elem).ResistVal;
            }

            res += GetModsByType(EffectType.ElementalResist, elem, ent: ent);
            return res;
        }

        public double GetDebuffResists(EffectType debuff, Event ent = null)
        {
            double res = 0;
            if (Fighter.DebuffResists.Any(x => x.Debuff == debuff))
            {
                res += Fighter.DebuffResists.First(x => x.Debuff == debuff).ResistVal;
            }

            res += GetModsByType(EffectType.DebufResist, ent: ent);
            return res;
        }
        public double DotBoost(Event ent = null)
        {
            return GetModsByType(EffectType.DoTBoost, ent: ent);
        }

        public double DefIgnore(Event ent = null)
        {
            return GetModsByType(EffectType.DefIgnore, ent: ent);
        }
        /// <summary>
        /// Get elem  boost
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public DamageBoostRec GetElemBoost(ElementEnm elem)
        {
            if (BaseDamageBoost.All(x => x.ElemType != elem))
                BaseDamageBoost.Add(new DamageBoostRec() { ElemType = elem, Value = 0 });
            return BaseDamageBoost.First(dmg => dmg.ElemType == elem);
        }
        public double GetElemBoostValue(ElementEnm elem, Event ent = null)
        {
            return GetElemBoost(elem).Value + GetModsByType(EffectType.ElementalBoost, elem, ent: ent);
        }

        /// <summary>
        /// Get total stat by Mods by type
        /// </summary>
        /// <returns></returns>
        public double GetModsByType(EffectType modType, ElementEnm? elem = null, AbilityTypeEnm? entAbilityValue = null, Event ent = null)
        {
            double res = 0;
            List<Mod> conditionsToCheck = new();
            //get all mods to check
            conditionsToCheck.AddRange(Mods);
            foreach (PassiveMod pmode in GetConditionMods(ent?.TargetUnit))
                conditionsToCheck.Add(pmode.Mod);


            foreach (Mod mod in conditionsToCheck)
            {
                foreach (Effect effect in mod.Effects.Where(y => y.EffType == modType
                                                                 && y.Element == elem
                                                                 && (y.AbilityTypes.Any(y => y == entAbilityValue) ||
                                                                     entAbilityValue == null)
                         )
                        )
                {
                    double finalValue;
                    if (effect.CalculateValue != null)
                    {
                        finalValue = (double)effect.CalculateValue(ent);
                    }
                    else
                    {
                        finalValue = (double)effect.Value;
                    }

                    res += finalValue * (effect.StackAffectValue ? mod.Stack : 1);
                }


            }

            return res;

        }

        /// <summary>
        /// search avalable for unit condition mods(planars self or ally)
        /// </summary>
        /// <returns></returns>
        public List<PassiveMod> GetConditionMods(Unit targetForCondition = null)
        {
            List<PassiveMod> res = new();
            if (ParentTeam == null)
                return res;



            foreach (Unit unit in ParentTeam.Units.Where(x => x.IsAlive))
            {
                if (unit.fighter.ConditionMods.Concat(unit.fighter.PassiveMods).Any())
                    res.AddRange(from cmod in unit.fighter.ConditionMods.Concat(unit.fighter.PassiveMods)
                                 where (cmod.Target == this || cmod.Target == this.ParentTeam) && (cmod is not ConditionMod mod || mod.Truly(targetForCondition))
                                 select cmod);
                if (unit.Fighter is DefaultFighter unitFighter)
                {
                    //LC

                    if (unitFighter.LightCone != null)
                        res.AddRange(from cmod in unitFighter.LightCone.ConditionMods.Concat(unitFighter.LightCone.PassiveMods)
                                     where (cmod.Target == this || cmod.Target == this.ParentTeam) && (cmod is not ConditionMod mod || mod.Truly(targetForCondition))
                                     select cmod);
                    //GEAR
                    foreach (IRelicSet relic in unitFighter.Relics)
                    {
                        res.AddRange(from cmod in relic.ConditionMods.Concat(relic.PassiveMods)
                                     where (cmod.Target == this || cmod.Target == this.ParentTeam) && (cmod is not ConditionMod mod || mod.Truly(targetForCondition))
                                     select cmod);
                    }

                }

            }



            return res;
        }

        public enum ElementEnm
        {
            Wind,
            Physical,
            Fire,
            Ice,
            Lightning,
            Quantum,
            Imaginary

        }

        public static Color GetColorByElem(ElementEnm? elem)
        {
           
           return elem  switch
           {
               ElementEnm.Wind=> Color.MediumSpringGreen,
               ElementEnm.Fire=> Color.OrangeRed,
               ElementEnm.Ice=> Color.LightSkyBlue,
               ElementEnm.Imaginary=>Color.Yellow,
               ElementEnm.Physical=>Color.WhiteSmoke,
               ElementEnm.Lightning=>Color.Violet,
               ElementEnm.Quantum=>Color.SlateBlue,
               
               _ => Color.Red
           };
        }

        public enum TypeEnm
        {
            Special,
            NPC,
            Character

        }

        /// <summary>
        /// Search mod by ref. and add stack or reset duration.
        /// If mod have no ref then search by itself
        /// </summary>
        /// <param name="mod"></param>
        /// <param name="abilityValue"></param>
        public void ApplyMod(Mod mod)
        {
            Mod srchMod = Mods.FirstOrDefault(x => (x.RefMod == (mod.RefMod ?? mod)
                                                   && (mod.UniqueUnit == null || x.UniqueUnit == mod.UniqueUnit))
                                                  || ((!String.IsNullOrEmpty(mod.UniqueStr) && String.Equals(x.UniqueStr, mod.UniqueStr))));



            //find existing by ref, or by UNIQUE tag
            if (srchMod != null)
            {
                //add stack
                srchMod.Stack = Math.Min(srchMod.MaxStack, srchMod.Stack + 1);
            }
            else
            {
                //or add new
                if (!mod.DoNotClone)
                {
                    srchMod = (Mod)mod.Clone();
                }
                else
                {
                    srchMod = mod;
                }
                srchMod.RefMod = mod.RefMod ?? mod;
                srchMod.Owner = this;
                Mods.Add(srchMod);




            }

            //reset duration
            srchMod.IsOld = false;//renew the flag
            srchMod.DurationLeft = srchMod.BaseDuration;


        }
        /// <summary>
        /// remove mod by ref
        /// </summary>
        /// <param name="mod"></param>
        public void RemoveMod(Mod mod)
        {
            if (Mods.Any(x => x.RefMod == (mod.RefMod ?? mod)))
            {
                Mods.Remove(Mods.First(x => x.RefMod == (mod.RefMod ?? mod)));
            }
        }

        /// <summary>
        /// Enemies
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
                    foreach (Wave wave in ParentTeam.ParentSim.NextFight.Waves)
                    {
                        nextEnemys.AddRange(wave.Units);

                    }

                    //get distinct
                    foreach (Unit unit in nextEnemys.DistinctBy(x => x.Name))
                    {
                        nextEnemysDistinct.Add(unit);
                    }

                    return nextEnemysDistinct;
                }
                else
                {
                    //return first other team
                    return ParentTeam.ParentSim.Teams
                        .First(x => x != ParentTeam && x.TeamType != Team.TeamTypeEnm.Special).Units;

                }
            }

        }
        /// <summary>
        /// Get Friends List
        /// </summary>
        /// <returns></returns>
        public List<Unit> Friends => ParentTeam.Units;


        public int Rank { get; set; }

        /// <summary>
        /// Init strings for class load after copying the object
        /// </summary>
        public string LightConeStringPath { get; set; }
        public int LightConeInitRank { get; set; }

        public List<KeyValuePair<string, int>> RelicsClasses { get; set; } = new List<KeyValuePair<string, int>>();
        public string FighterClassName { get; set; }

        //if unit under control
        public bool Controlled
        {
            get
            {
                return Mods.Any(x => x.CrowdControl);
            }

        }

        public IEnumerable<Unit> GetTargetsForUnit(TargetTypeEnm? targetType)
        {
            if (targetType == TargetTypeEnm.Friend)
                return Friends.Where(x => x.IsAlive);
            else if (targetType == TargetTypeEnm.Enemy)
                return Enemies.Where(x => x.IsAlive);

            throw new NotImplementedException();
        }

        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/Vulnerability
        /// </summary>
        /// <param name="attackElem"></param>
        /// <param name="targetForCondition"></param>
        /// <returns></returns>
        public double GetElemVulnerability(ElementEnm attackElem, Event ent = null)
        {
            return GetModsByType(EffectType.ElementalVulnerability, attackElem, ent: ent);

        }

        public double GetAllDamageVulnerability(Event ent = null)
        {
            return GetModsByType(EffectType.AllDamageVulnerability, ent: ent);

        }

        public double GetDotVulnerability(Event ent = null)
        {
            return GetModsByType(EffectType.DoTVulnerability, ent: ent);

        }
        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/DMG_Reduction
        /// </summary>
        /// <returns></returns>
        public double GetDamageReduction(Unit targetForCondition = null)
        {
            double res = 1;

            //MODS
            if (Mods != null)
                foreach (Mod mod in Mods)
                {
                    foreach (Effect effect in mod.Effects.Where(x => x.EffType == EffectType.DamageReduction))
                    {
                        for (int i = 0; i < mod.Stack; i++)
                            res *= 1 - effect.Value ?? 0;
                    }
                }
            //Condition
            foreach (PassiveMod pmod in GetConditionMods(targetForCondition))
            {
                foreach (Effect effect in pmod.Mod.Effects.Where(x => x.EffType == EffectType.DamageReduction))
                {
                    for (int i = 0; i < pmod.Mod.Stack; i++)
                        res *= 1 - effect.Value ?? 0;
                }
            }
            return res;

        }
        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
        /// </summary>
        /// <returns></returns>
        public double GetBrokenMultiplier()
        {
            if (GetRes(ResourceType.Toughness).ResVal > 0)
                return 0.9;
            else
                return 1;
        }

        public double GetAbilityTypeMultiplier(Ability entAbilityValue, Event ent = null)
        {
            return 1 + GetModsByType(EffectType.AbilityTypeBoost, null, entAbilityValue.AbilityType, ent: ent);
        }
    }

}
