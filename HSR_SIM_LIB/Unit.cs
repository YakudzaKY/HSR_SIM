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
using static HSR_SIM_LIB.Constant;
using static HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Resource;
using static HSR_SIM_LIB.Ability;

namespace HSR_SIM_LIB
{/// <summary>
/// Unit class. Stats skills etc
/// </summary>
    public class Unit : CheckEssence
    {
        public Team ParentTeam { get; set; } = null;
        string name = string.Empty;
        int level = 1;
        Bitmap portrait = null;
        UnitStats stats = null;
        public bool IsAlive = true;

        private IFighter fighter = null;
        public IFighter Fighter
        {
            get =>
                fighter = fighter ?? ((IFighter)Activator.CreateInstance(Type.GetType(FighterClassName)!, this));
            set => fighter = value;
        }

        public Bitmap Portrait
        {
            get
            {
                if (portrait == null)
                {

                    //resize
                    portrait = new Bitmap(LoadBitmap(UnitType.ToString() + "\\" + Name), PortraitSize);

                }
                return portrait;
            }
            set => portrait = value;
        }
        public List<Mod> Mods { get; set; } = new List<Mod>();
        private List<Resource> resources = null;
       
        public string Name { get => name; set => name = value; }
        public UnitStats Stats
        {
            get
            {
                if (stats == null)
                    stats = new UnitStats();
                return stats;
            }
            set => stats = value;
        }

       
        public Unit Reference { get; internal set; }
        public int Level { get => level; set => level = value; }

        public List<Resource> Resources
        {
            get
            {//create all resources
                if (resources == null)
                {
                    resources = new List<Resource>();
                    foreach (string name in Enum.GetNames<ResourceType>())
                    {
                        Resource res = new Resource();
                        res.ResType = (ResourceType)Enum.Parse(typeof(ResourceType), name);
                        res.ResVal = 0;
                        resources.Add(res);
                    }
                }
                return resources;
            }
            set => resources = value;
        }

        public TypeEnm UnitType { get; set; }

        //TODO unit role on battlefield
        //role changes on PRE-FIGHT(depend on weakness). changes on party dead or enemy dead(depend on weakness)---


        public Unit()
        {
        
        }


        /// <summary>
        /// Prepare to combat
        /// </summary>
        public void InitToCombat()
        {
            Stats.MaxHp = Stats.BaseMaxHp;
            GetRes(ResourceType.HP).ResVal = Stats.MaxHp;
            GetRes(ResourceType.Toughness).ResVal = Stats.MaxToughness;
            Fighter = null;

        }
        /// <summary>
        /// Get resource By Type
        /// </summary>
        public Resource GetRes(ResourceType rt)
        {
            return Resources.Where(resource => resource.ResType == rt).First();
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
 
        public void ApplyMod(Mod mod)
        {
            Mods.Add(mod);
        }
        public void RemoveMod(Mod mod)
        {
            Mods.Remove(mod);
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
                    List<Unit> nextEnemys = new List<Unit>();
                    List<Unit> nextEnemysDistinct = new List<Unit>();
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
                    //return first othjer team
                    return ParentTeam.ParentSim.Teams
                        .First(x => x != ParentTeam && x.TeamType != Team.TeamTypeEnm.Special).Units;

                }
            }
            set => throw new NotImplementedException();
        }
        /// <summary>
        /// Get Friends List
        /// </summary>
        /// <returns></returns>
        public List<Unit> Friends
        {
            get => ParentTeam.Units;

            set => throw new NotImplementedException();
        }

        public string FighterClassName { get; set; }

        public IEnumerable<CheckEssence> GetTargets(TargetTypeEnm targetType)
        {
            if (targetType == TargetTypeEnm.Party)
                return Friends;
            else if (targetType == TargetTypeEnm.Hostiles)
                return Enemies;

            throw new NotImplementedException();
        }
    }

}
