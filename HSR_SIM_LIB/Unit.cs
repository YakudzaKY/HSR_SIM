using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static HSR_SIM_LIB.Constant;
using static HSR_SIM_LIB.Utils;

namespace HSR_SIM_LIB
{/// <summary>
/// Unit class. Stats skills etc
/// </summary>
    public class Unit : CheckEssence
    {

        string name = string.Empty;
        Bitmap portrait = null;
        UnitStats stats = null;
        List<Ability> abilities = null;
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

        private ElementEnm? element;
        private List<ElementEnm> weaknesses = null;
        //TODO unit role on battlefield
        //role changes on PRE-FIGHT(depend on weakness). changes on party dead or enemy dead(depend on weakness)---


        public Unit()
        {
            Abilities = new List<Ability>();
        }

        public bool IsAlive = true;
        /// <summary>
        /// Prepare to combat
        /// </summary>
        public void InitToCombat()
        {
            Stats.MaxHp = Stats.BaseMaxHp;
            Stats.CurrentHp = Stats.MaxHp;


        }



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

    }

}
