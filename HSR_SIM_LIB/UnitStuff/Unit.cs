using System;
using System.Collections.Generic;
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
                portrait ??=  GraphicsCls.ResizeBitmap(Utl.LoadBitmap(UnitType.ToString() + "\\" + Name), PortraitSize.Height,PortraitSize.Width);
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
            get => resources = resources ?? new List<Resource>();
            set => resources = value;
        }


        public List<DamageBoostRec> BaseDamageBoost
        {
            get
            {
                return
                baseDamageBoost = baseDamageBoost ?? new List<DamageBoostRec>();
            }
            set => baseDamageBoost = value;
        }

        public TypeEnm UnitType { get; set; }


        public Unit()
        {

        }


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

        public double AllDmgBoost()
        {
            return GetModsByType(EffectType.AllDamageBoost);
        }

        public double ResistsPenetration(ElementEnm elem)//todo calc
        {
            return GetModsByType(EffectType.ElementalPenetration, elem);
        }
        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/Damage_RES
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public double GetResists(ElementEnm elem)
        {
            double res = 0;
            if (Fighter.Resists.Any(x => x.ResistType == elem))
            {
                res += Fighter.Resists.First(x => x.ResistType == elem).ResistVal;
            }

            res += GetModsByType(EffectType.ElementalResist, elem);
            return res;
        }

        public double GetDebuffResists(EffectType debuff)
        {
            double res = 0;
            if (Fighter.DebuffResists.Any(x => x.Debuff == debuff))
            {
                res += Fighter.DebuffResists.First(x => x.Debuff == debuff).ResistVal;
            }

            res += GetModsByType(EffectType.ElementalResist, debuff:debuff);
            return res;
        }
        public double DotBoost()//todo calc
        {
            return GetModsByType(EffectType.DoTBoost);
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
        public double GetElemBoostValue(ElementEnm elem)
        {
            return GetElemBoost(elem).Value + GetModsByType(EffectType.ElementalBoost, elem);
        }

        /// <summary>
        /// Get total stat by Mods by type
        /// </summary>
        /// <returns></returns>
        public double GetModsByType(EffectType modType, ElementEnm? elem = null, AbilityTypeEnm? entAbilityValue = null,EffectType? debuff=null)
        {
            double res = 0;
            if (Mods != null)
                foreach (Mod mod in Mods)
                {
                    foreach (Effect effect in mod.Effects.Where(y => y.EffType == modType&&y.Element == elem && (y.AbilityTypes.Any(y => y == entAbilityValue)||entAbilityValue==null)))
                    {
                        res += (double)effect.Value * (effect.StackAffectValue?mod.Stack:1);
                    }

                }
            //apply mod from Gear
            foreach (PassiveMod pmode in GetConditionMods())
            {
                foreach (Effect effect in pmode.Mod.Effects.Where(y =>
                             y.EffType == modType && y.Element == elem &&
                             (y.AbilityTypes.Any(y => y == entAbilityValue) || entAbilityValue == null)))
                {
                    res += (double)effect.Value * (effect.StackAffectValue?pmode.Mod.Stack:1);
                }
            }

            return res;
        }

        /// <summary>
        /// search avalable for unit condition mods(planars self or ally)
        /// </summary>
        /// <returns></returns>
        public List<PassiveMod> GetConditionMods()
        {
            List<PassiveMod> res = new();
            if (ParentTeam == null)
                return res;



            foreach (Unit unit in ParentTeam.Units.Where(x => x.IsAlive))
            {
                //TODO ADD PASIVE MODS
                if (unit.fighter.ConditionMods.Concat(unit.fighter.PassiveMods).Any())
                    res.AddRange(from cmod in unit.fighter.ConditionMods.Concat(unit.fighter.PassiveMods)
                                 where (cmod.Target == this || cmod.Target == this.ParentTeam) && (!(cmod is ConditionMod mod)|| mod.Truly)
                                 select cmod);
                if (unit.Fighter is DefaultFighter)
                {
                    var unitFighter = (DefaultFighter)unit.Fighter;
                    //LC
                   
                    res.AddRange(from cmod in unitFighter.LightCone.ConditionMods.Concat( unitFighter.LightCone.PassiveMods)
                                     where (cmod.Target == this || cmod.Target == this.ParentTeam)  && (!(cmod is ConditionMod mod)|| mod.Truly)
                                     select cmod);
                    //GEAR
                    foreach (IRelicSet relic in unitFighter.Relics)
                    {
                        res.AddRange(from cmod in relic.ConditionMods.Concat(relic.PassiveMods)
                                     where (cmod.Target == this || cmod.Target == this.ParentTeam)  && (!(cmod is ConditionMod mod)|| mod.Truly)
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
        public void ApplyMod(Mod mod)
        {
            Mod newMod = null;
            //find existing by ref, or by UNIQUE tag
            if (Mods.Any(x => x.RefMod == (mod.RefMod ?? mod)
                              && (String.IsNullOrEmpty(mod.UniqueStr) || String.Equals(x.UniqueStr, mod.UniqueStr)
                              )))
            {
                newMod = Mods.First(x => x.RefMod == (mod.RefMod ?? mod));
                //add stack
                newMod.Stack = Math.Min(newMod.MaxStack, newMod.Stack+1);
            }
            else
            {
                //or add new
                newMod = (Mod)mod.Clone();
                newMod.RefMod = mod.RefMod ?? mod;
                Mods.Add(newMod);

            }

            //reset duration
            newMod.DurationLeft = newMod.BaseDuration;
         

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

        public IEnumerable<CloneClass> GetTargets(TargetTypeEnm targetType)
        {
            if (targetType == TargetTypeEnm.Party)
                return Friends;
            else if (targetType == TargetTypeEnm.Hostiles)
                return Enemies;

            throw new NotImplementedException();
        }
        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/Vulnerability
        /// </summary>
        /// <param name="attackElem"></param>
        /// <returns></returns>
        public double GetElemVulnerability(ElementEnm attackElem)
        {
            return GetModsByType(EffectType.ElementalVulnerability, attackElem);

        }

        public double GetAllDamageVulnerability()
        {
            return GetModsByType(EffectType.AllDamageVulnerability);

        }

        public double GetDoteVulnerability()
        {
            return GetModsByType(EffectType.DoTVulnerability);

        }
        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/DMG_Reduction
        /// </summary>
        /// <returns></returns>
        public double GetDamageReduction()
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
            foreach (PassiveMod pmod in GetConditionMods())
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

        public double GetAbilityTypeMultiplier(Ability entAbilityValue)
        {
            return 1 + GetModsByType(EffectType.AbilityTypeBoost, null, entAbilityValue.AbilityType);
        }
    }

}
