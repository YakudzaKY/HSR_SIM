using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static HSR_SIM_LIB.Constant;
using static HSR_SIM_LIB.Utils;
using static HSR_SIM_LIB.Resource;

namespace HSR_SIM_LIB
{/// <summary>
/// Unit class. Stats skills etc
/// </summary>
    public class Unit : CheckEssence
    {

        string name = string.Empty;
        int level = 1;
        Bitmap portrait = null;
        UnitStats stats = null;
        List<Ability> abilities = null;
        public bool IsAlive = true;
        public Bitmap Portrait
        {
            get
            {
                if (portrait == null)
                {

                    //resize
                    portrait = new Bitmap(LoadBitmap(Name), PortraitSize);

                }
                return portrait;
            }
            set => portrait = value;
        }
        public List<Ability> Abilities { get => abilities; set => abilities = value; }
        public ElementEnm? Element { get => element; set => element = value; }
        public List<Mod> Mods { get; set; }= new List<Mod>();
        private ElementEnm? element;
        private List<ElementEnm> weaknesses = null;
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

        public List<ElementEnm> Weaknesses { get => weaknesses; set => weaknesses = value; }
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

        //TODO unit role on battlefield
        //role changes on PRE-FIGHT(depend on weakness). changes on party dead or enemy dead(depend on weakness)---


        public Unit()
        {
            Abilities = new List<Ability>();
        }

       
        /// <summary>
        /// Prepare to combat
        /// </summary>
        public void InitToCombat()
        {
            Stats.MaxHp = Stats.BaseMaxHp;
            GetRes(ResourceType.HP).ResVal = Stats.MaxHp;
            //Clone abilities from template
            List<Ability> clonedAbilities= new List<Ability>();
            foreach (Ability ability in Reference.Abilities)
            {
                Ability newAbility = (Ability)ability.Clone();
                newAbility.Parent = this;
                clonedAbilities.Add(newAbility);
            }
            Abilities = clonedAbilities;

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
        public enum unitHostility
        {
            Friendly,
            Hostile
        }

        public void ApplyMod(Mod mod)
        {
            Mods.Add(mod);
        }
        public void RemoveMod(Mod mod)
        {
            Mods.Remove(mod);
        }
    }

}
