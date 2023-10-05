using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HSR_SIM_LIB.Constant;

namespace HSR_SIM_LIB
{
    public class Unit
    {
        
        string name = string.Empty;
        Bitmap portrait = null;
        UnitStats stats = null;
        public Bitmap Portrait { get => portrait; set => portrait = value; }
        public Unit() {        

        }
        /// <summary>
        /// Prepare to combat
        /// </summary>
        public void InitToCombat()
        {
            Stats.MaxHp = Stats.BaseMaxHp;
            Stats.CurrentHp = Stats.MaxHp;
            FileInfo fi = new FileInfo(Utils.getAvalableImageFile(Name));
            //load portrait
            Portrait = Utils.NewBitmap(fi);
            //resize
            Portrait = new Bitmap(Portrait,PortraitSize);
        }

        

        public string Name { get => name; set => name = value; }
        public UnitStats Stats { 
            get 
            { 
                if (stats==null)
                    stats = new UnitStats();
                return stats;
            } 
            set => stats = value; }

       
    }
}
