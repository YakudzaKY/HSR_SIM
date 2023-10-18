﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using HSR_SIM_LIB.Fighters;
using static HSR_SIM_LIB.Constant;
using static HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Resource;
using static HSR_SIM_LIB.Ability;
using static HSR_SIM_LIB.Unit;

namespace HSR_SIM_LIB
{
    /// <summary>
    /// Unit class. Stats skills etc
    /// </summary>
    public class Unit : CheckEssence
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
                portrait ??= new Bitmap(LoadBitmap(UnitType.ToString() + "\\" + Name), PortraitSize);
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
            get { return 
                baseDamageBoost = baseDamageBoost ?? new List<DamageBoostRec>(); }
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
            if (!Resources.Any(x=>x.ResType==rt))
                Resources.Add(new Resource(){ResType = rt,ResVal = 0});
            return Resources.First(resource => resource.ResType == rt);
        }

        public double AllDmgBoost()
        {
            return GetModsByType(Mod.ModifierType.AllDamageBoost);
        }

        public double ResistsPenetration(ElementEnm elem)//todo calc
        {
            return GetModsByType(Mod.ModifierType.ElementalPenetration, elem);
        }
        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/Damage_RES
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public double GetResists(ElementEnm elem)//todo self res+ buffs+debuffs
        {
            double res = 0;
            if (Fighter.Resists.Any(x => x.ResistType == elem))
            {
                res += Fighter.Resists.First(x => x.ResistType == elem).ResistVal ;
            }

            res += GetModsByType(Mod.ModifierType.ElementalResist,elem);
            return res;
        }
        public double DotBoost()//todo calc
        {
            return GetModsByType(Mod.ModifierType.DoTBoost);
        }
        /// <summary>
        /// Get elem  boost
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public DamageBoostRec GetElemBoost(ElementEnm elem)
        {
            if (BaseDamageBoost.All(x => x.ElemType != elem))
                BaseDamageBoost.Add(new DamageBoostRec(){ElemType = elem,Value = 0});
            return BaseDamageBoost.First(dmg => dmg.ElemType == elem);
        }
        public double GetElemBoostValue(ElementEnm elem)
        {
            return GetElemBoost(elem).Value +GetModsByType(Mod.ModifierType.ElementalBoost,elem);
        }

        /// <summary>
        /// Get total stat by Mods by type
        /// </summary>
        /// <returns></returns>
        public double GetModsByType(Mod.ModifierType modType, Unit.ElementEnm? elem=null)
        {
            double res = 0;
            if (Mods!=null)
                foreach (Mod mod in Mods.Where(x=>x.Modifier==modType&&x.Element==elem))
                {
                    res += mod.Value*mod.Stack??0;
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
            Mod newMod=null;
            //find existing
            if (Mods.Any(x => x.RefMod ==(mod.RefMod ?? mod)))
            {
                newMod = Mods.First(x => x.RefMod == (mod.RefMod ?? mod));
            }
            else
            {
                //or add new
                newMod = (Mod)mod.Clone();
                newMod.RefMod = (mod.RefMod ?? mod);
                Mods.Add(newMod);
            }

            //reset duration
            newMod.DurationLeft=newMod.BaseDuration;
            //add stack
            newMod.Stack = Math.Max(newMod.MaxStack,newMod.Stack+1);
            
        }
        /// <summary>
        /// remove mod by ref
        /// </summary>
        /// <param name="mod"></param>
        public void RemoveMod(Mod mod)
        {
            if (Mods.Any(x => x.RefMod ==(mod.RefMod ?? mod)))
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

        public string FighterClassName { get; set; }
        public int Rank { get; set; }

        public IEnumerable<CheckEssence> GetTargets(TargetTypeEnm targetType)
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
            return GetModsByType(Mod.ModifierType.ElementalVulnerability, attackElem);

        }

        public double GetAllDamageVulnerability()
        {
            return GetModsByType(Mod.ModifierType.AllDamageVulnerability);

        }

        public double GetDoteVulnerability()
        {
            return GetModsByType(Mod.ModifierType.DoTVulnerability);

        }
        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/DMG_Reduction
        /// </summary>
        /// <returns></returns>
        public double GetDamageReduction()
        {
            double res = 1;
 
            if (Mods!=null)
                foreach (Mod mod in Mods.Where(x=>x.Modifier==Mod.ModifierType.DamageReduction))
                {
                    for (int i = 0; i < mod.Stack; i++)
                        res *= (1-mod.Value??0);
                }

            return res;

        }
        /// <summary>
        /// https://honkai-star-rail.fandom.com/wiki/Toughness#Weakness_Break
        /// </summary>
        /// <returns></returns>
        public double GetBrokenMultiplier()
        {
            if (GetRes(ResourceType.Toughness).ResVal>0 )
                return 0.9; 
            else 
                return 1;
        }
    }

}
